using RUDP.Enums;
using RUDP.Extensions;
using RUDP.Models;
using RUDP.Utilities;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace RUDP
{
    public class MultiThreadUDPSocket : IDisposable
    {
        public delegate void EventHandler();
        public delegate void EventHandler<T1>(T1 p1);
        public delegate void EventHandler<T1, T2>(T1 p1, T2 p2);
        public delegate void EventHandler<T1, T2, T3>(T1 p1, T2 p2, T3 p3);
        public delegate void EventHandler<T1, T2, T3, T4>(T1 p1, T2 p2, T3 p3, T4 p4);
        public delegate void EventHandler<T1, T2, T3, T4, T5>(T1 p1, T2 p2, T3 p3, T4 p4, T5 t5);
        public delegate void EventHandler<T1, T2, T3, T4, T5, T6>(T1 p1, T2 p2, T3 p3, T4 p4, T5 t5, T6 t6);
        public event EventHandler<Exception> OnException;
        public event EventHandler<EndPoint, ushort> OnMTUSizeExceed;
        public event EventHandler<EndPoint> OnRemoteConnectionReset;
        public event EventHandler<EndPoint, byte[], long> OnReceive;
        public event EventHandler<Rates, Dictionary<EndPoint, Rates>, int> OnRateUpdated;


        public Socket? udp { get; private set; } = null;
        public EndPoint LocalEP { get; private set; }
        public SocketStatus Status { get; set; } = SocketStatus.NotInitialized;


        private byte[]? _heapBuffer { get; set; } = null;
        private int _customPort { get; set; } = 0;
        private int _sendBufferFillStatus { get; set; } = 0;
        private bool _sendRunning { get; set; } = false;
        private bool _receiveRunning { get; set; } = false;
        private bool _emitRunning { get; set; } = false;
        private bool _backgroundTaskRunning { get; set; } = false;


        private ConcurrentQueue<RawData> _receivedData { get; set; } = new();
        private ConcurrentQueue<RawData> _sendQueue { get; set; } = new();
        private Rates _totalRates { get; set; } = new();
        private ConcurrentDictionary<EndPoint, Rates> _epsRates { get; set; } = new();
        private ConcurrentDictionary<EndPoint, double> _epsMaxCongestionWindow { get; set; } = new();


        private double _maxUploadSpeed { get; set; } = 0;
        private ConcurrentDictionary<EndPoint, double> _epsMaxUploadSpeed { get; set; } = new();


        /// <summary>
        /// Initialize a multithreaded UDP socket instance.
        /// </summary>
        /// <param name="customPort">Optionally set the port to be used</param>
        public MultiThreadUDPSocket(int customPort = 0)
        {
            _customPort = customPort;
            Init();
        }
        private void Init()
        {
            try
            {
                udp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                udp.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    udp.SetIPProtectionLevel(IPProtectionLevel.Unrestricted); // Abilita NAT Traversal
                udp.DontFragment = true;
                udp.MulticastLoopback = false;
                udp.Ttl = 50;
                udp.SendBufferSize = int.MaxValue;
                udp.ReceiveBufferSize = int.MaxValue;
                udp.Blocking = true; // Quando il SendBuffer è pieno, il thread che invia viene bloccato fino a che non si libera spazio

                // Eseguo manualmente un binding su una porta solo se è specificata, altrimenti lascio decidere al sistema
                IPAddress localIPAddress = NetworkUtilities.GetLocalIPAddress();
                if (_customPort > 0)
                {
                    LocalEP = new IPEndPoint(localIPAddress, _customPort);
                    udp.Bind(LocalEP);
                }
                else
                {
                    // Invio un pacchetto a caso per bindare il socket in automatico
                    udp.SendTo(new byte[1], "192.168.1.1:80".ToEndPoint());
                    LocalEP = new IPEndPoint(localIPAddress, ((IPEndPoint)udp.LocalEndPoint).Port);
                }

                Status = SocketStatus.Stopped;
            }
            catch (Exception ex)
            {
                OnException?.Invoke(ex);
            }
        }


        /// <summary>
        /// Try to run the socket only if the current Status is Stopped.
        /// The status become Starting, then if everything goes well it change into Running, otherwise into Stopped.
        /// </summary>
        public SocketStatus Start()
        {
            if (Status != SocketStatus.Stopped)
                return Status;
            Status = SocketStatus.Starting;
            try
            {
                StartBackgroundTasks();
                StartSend();
                StartReceive();
                StartEmit();
                Status = SocketStatus.Running;
            }
            catch (Exception ex)
            {
                Status = SocketStatus.Stopped;
                OnException?.Invoke(ex);
            }
            return Status;
        }
        /// <summary>
        /// Try to restart the socket only if the current Status is Running.
        /// </summary>
        /// <param name="customPort">Optionally you can change the binded port</param>
        /// <returns></returns>
        public SocketStatus Restart(int customPort = 0)
        {
            if (Status != SocketStatus.Running)
                return Status;
            Stop();
            if (Status != SocketStatus.Stopped)
                return Status;

            _customPort = customPort == 0 ? _customPort : customPort;
            Init();

            // Wait for no longer than 1 second for the socket to come back in Running status
            int maxRetry = 10;
            for (int i = 0; i < maxRetry || !_backgroundTaskRunning || !_receiveRunning || !_sendRunning || !_emitRunning; i++)
                ThreadUtilities.PauseThread(100);
            return Start();
        }
        /// <summary>
        /// Try to stop the socket only if the current status is Running.
        /// The status become Stopping, then if everything goes well it change into Stopped, otherwise it remain Stopping.
        /// </summary>
        public SocketStatus Stop()
        {
            if (Status != SocketStatus.Running)
                return Status;
            Status = SocketStatus.Stopping;
            try
            {
                if (udp is not null)
                    udp.Shutdown(SocketShutdown.Both);
            }
            finally
            {
                if (udp is not null)
                {
                    udp.Close();
                    udp.Dispose();
                }

                udp = null;
                _receivedData.Clear();
                _heapBuffer = null;

                _epsRates.Clear();
            }
            Status = SocketStatus.Stopped;
            return Status;
        }


        /// <summary>
        /// Enqueue this data into an internal buffer which is consumed by another thread as fast as possible.
        /// This method should return as fast as possible, but in case the amount of bytes enqueued reach the 99% of the available socket send buffer, it freeze for 2 seconds.
        /// </summary>
        /// <param name="sendTo"></param>
        /// <param name="data"></param>
        public bool Send(EndPoint sendTo, byte[] data)
        {
            if (udp is null)
                return false;

            if (!_epsRates.ContainsKey(sendTo))
            {
                _epsRates.TryAdd(sendTo, new());
                _epsMaxCongestionWindow.TryAdd(sendTo, 0);
                _epsMaxUploadSpeed.TryAdd(sendTo, 0);
            }

            int sendBufferFillPercentage = Convert.ToInt32(Math.Round((double)_sendBufferFillStatus * 100 / (double)udp.SendBufferSize, MidpointRounding.AwayFromZero));
            if (sendBufferFillPercentage >= 99)
                ThreadUtilities.PauseThread(2000);

            _sendBufferFillStatus += data.Length;
            _sendQueue.Enqueue(new(sendTo, data));
            return true;
        }
        /// <summary>
        /// Set the amount of bytes per second available for this endpoint to use
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="congestionWindow">Expressed in bytes, is the max amount per second this endpoint can send</param>
        public void SetEPCongestionWindow(EndPoint ep, double congestionWindow)
        {
            _epsMaxCongestionWindow.AddOrUpdate(
                ep,
                addValue: congestionWindow,
                updateValueFactory: (ep, cw) => congestionWindow
            );
        }
        /// <summary>
        /// Set the max amount of bytes per send sendable to this specific EndPoint
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="maxUploadSpeed"></param>
        public void SetEPMaxUploadSpeed(EndPoint ep, double maxUploadSpeed)
        {
            _epsMaxUploadSpeed.AddOrUpdate(
                ep,
                addValue: maxUploadSpeed,
                updateValueFactory: (ep, ups) => maxUploadSpeed
            );
        }
        /// <summary>
        /// Set the max amount of bytes sendable per second.
        /// Doesn't discern between endpoints, just the total rate
        /// </summary>
        /// <param name="maxUploadSpeed"></param>
        public void SetMaxUploadSpeed(double maxUploadSpeed)
        {
            _maxUploadSpeed = maxUploadSpeed;
        }


        /// <summary>
        /// Use this method to remove a specific EndPoint from the internal buffer of known EndPoints.
        /// Should be called everytime an endpoint is disconnected manually or accidentally.
        /// </summary>
        /// <param name="ep"></param>
        public void DisconnectEndPoint(EndPoint ep)
        {
            if (_epsRates.ContainsKey(ep))
            {
                _epsRates.Remove(ep, out _);
                _epsMaxCongestionWindow.Remove(ep, out _);
                _epsMaxUploadSpeed.Remove(ep, out _);
            }
        }


        private void StartBackgroundTasks()
        {
            Task.Factory.StartNew(() =>
            {
                _backgroundTaskRunning = true;
                Stopwatch _backgroundTaskTimer = Stopwatch.StartNew();
                while (udp is not null)
                    try
                    {
                        Rates totalRates = new();
                        foreach (KeyValuePair<EndPoint, Rates> ep in _epsRates)
                        {
                            totalRates.SentBytesPerSecond += ep.Value.SentBytesPerSecond;
                            totalRates.ReceivedBytesPerSecond += ep.Value.ReceivedBytesPerSecond;
                            totalRates.SentPacketsPerSecond += ep.Value.SentPacketsPerSecond;
                            totalRates.ReceivedPacketsPerSecond += ep.Value.ReceivedPacketsPerSecond;
                            ep.Value.SentBytesPerSecond = 0;
                            ep.Value.SentPacketsPerSecond = 0;
                            ep.Value.ReceivedBytesPerSecond = 0;
                            ep.Value.ReceivedPacketsPerSecond = 0;
                        }
                        // It calculates an extimation of the current fill percentage of the send buffer
                        _totalRates = new(totalRates);
                        OnRateUpdated?.Invoke(_totalRates, _epsRates.ToDictionary(), Convert.ToInt32(Math.Floor(_sendBufferFillStatus * 100d / udp.SendBufferSize)));

                        while (_backgroundTaskTimer.Elapsed.TotalMilliseconds < 1000 && udp is not null)
                            ThreadUtilities.PauseThread(100);
                        _backgroundTaskTimer.Restart();
                    }
                    catch (Exception ex)
                    {
                        OnException?.Invoke(ex);
                    }
                _backgroundTaskTimer.Stop();
                _backgroundTaskRunning = false;
            }, TaskCreationOptions.LongRunning);
        }
        private void StartSend()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    _sendRunning = true;
                    int bytesSent = 0;
                    RawData pkt = new();
                    while (udp is not null)
                        try
                        {
                            if (!_sendQueue.TryDequeue(out pkt))
                            {
                                ThreadUtilities.PauseThread(100);
                                continue;
                            }

                            if (
                                // If this endpoint has a congestion window setted and has reached or surpassed the amount of sendable data
                                (_epsMaxCongestionWindow.ContainsKey(pkt.EP) && _epsMaxCongestionWindow[pkt.EP] > 0 && _epsRates[pkt.EP].SentBytesPerSecond + pkt.Data.Length >= _epsMaxCongestionWindow[pkt.EP])
                                // Or if the max upload speed for this endpoint has been reached
                                || (_epsMaxUploadSpeed.ContainsKey(pkt.EP) && _epsMaxUploadSpeed[pkt.EP] > 0 && _epsRates[pkt.EP].SentBytesPerSecond + pkt.Data.Length >= _epsMaxUploadSpeed[pkt.EP])
                            )
                            {
                                // The packet simply gets rescheduled
                                _sendQueue.Enqueue(new(pkt.EP, pkt.Data));
                                continue;
                            }
                            // If the global max upload speed has been reached
                            if (_maxUploadSpeed > 0)
                            {
                                while (_totalRates.SentBytesPerSecond + pkt.Data.Length >= _maxUploadSpeed)
                                    ThreadUtilities.PauseThread(100);
                            }

                            bytesSent = udp.SendTo(pkt.Data, pkt.Data.Length, SocketFlags.None, pkt.EP);

                            _sendBufferFillStatus -= bytesSent;
                            _epsRates[pkt.EP].SentBytesPerSecond += bytesSent;
                            _epsRates[pkt.EP].SentPacketsPerSecond += 1;

                            pkt.Dispose();
                        }
                        catch (SocketException sockEx)
                        {
                            if (pkt is not null)
                                ManageSocketException(sockEx, pkt.EP, pkt.Data.Length);
                        }
                        catch (Exception ex)
                        {
                            OnException?.Invoke(ex);
                        }
                }
                catch (Exception ex)
                {
                    OnException?.Invoke(ex);
                }
                _sendRunning = false;
            }, TaskCreationOptions.LongRunning);
        }
        private void StartReceive()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    _receiveRunning = true;
                    EndPoint? receivedFrom = new IPEndPoint(IPAddress.Any, 0);
                    int receivedBytes = 0;
                    _heapBuffer = GC.AllocateArray<byte>(length: 1500, pinned: true);
                    while (udp is not null)
                        try
                        {
                            receivedFrom = new IPEndPoint(IPAddress.Any, 0);
                            receivedBytes = 0;
                            if (_heapBuffer is not null && udp is not null)
                                receivedBytes = udp.ReceiveFrom(_heapBuffer, SocketFlags.None, ref receivedFrom);
                            if (_heapBuffer is null || udp is null || receivedBytes == 0 || receivedFrom.EqualTo(udp.LocalEndPoint))
                                continue;
                            if (!_epsRates.ContainsKey(receivedFrom))
                            {
                                _epsRates.TryAdd(receivedFrom, new());
                                _epsMaxCongestionWindow.TryAdd(receivedFrom, 0);
                                _epsMaxUploadSpeed.TryAdd(receivedFrom, 0);
                            }
                            _epsRates[receivedFrom].ReceivedBytesPerSecond += receivedBytes;
                            _epsRates[receivedFrom].ReceivedPacketsPerSecond += 1;

                            if (_heapBuffer is not null)
                                _receivedData.Enqueue(new(receivedFrom, _heapBuffer.AsSpan().Slice(0, receivedBytes).ToArray()));
                        }
                        catch (SocketException sockEx)
                        {
                            ManageSocketException(sockEx, receivedFrom, receivedBytes);
                        }
                        catch (Exception ex)
                        {
                            OnException?.Invoke(ex);
                        }
                }
                catch (Exception ex)
                {
                    OnException?.Invoke(ex);
                }
                _receiveRunning = false;
            }, TaskCreationOptions.LongRunning);
        }
        private void StartEmit()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    _emitRunning = true;
                    while (udp is not null)
                        try
                        {
                            if (!_receivedData.TryDequeue(out RawData? pkt))
                            {
                                ThreadUtilities.PauseThread(100);
                                continue;
                            }
                            if (pkt is null)
                                continue;

                            OnReceive?.Invoke(pkt.EP, pkt.Data, DateTime.Now.Ticks);
                            pkt.Dispose();
                        }
                        catch (Exception ex)
                        {
                            OnException?.Invoke(ex);
                        }
                }
                catch (Exception ex)
                {
                    OnException?.Invoke(ex);
                }
                _emitRunning = false;
            }, TaskCreationOptions.LongRunning);
        }


        private void ManageSocketException(SocketException sockEx, EndPoint ep, int mtuSizeExceededValue)
        {
            // Errore MessageSize (10040) indica che è stata sforata la dimensione massima per i pacchetti
            if (sockEx.ErrorCode == (int)SocketError.MessageSize)
                OnMTUSizeExceed?.Invoke(ep, Convert.ToUInt16(mtuSizeExceededValue));
            else if (sockEx.ErrorCode == (int)SocketError.ConnectionReset)
                OnRemoteConnectionReset?.Invoke(ep);
            else
                OnException?.Invoke(sockEx);
        }


        public void Dispose()
        {
            Stop();
            ThreadUtilities.PauseThread(200);
            _receivedData.Clear();
            _sendQueue.Clear();
            _epsRates.Clear();
            _epsMaxCongestionWindow.Clear();
            _epsMaxUploadSpeed.Clear();
        }
    }
}
