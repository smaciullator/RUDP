using ByteSizeLib;
using Client.Models;
using NostrSharp.Keys;
using RUDP;
using RUDP.Enums;
using RUDP.Extensions;
using RUDP.Models;
using RUDP.Utilities;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;

namespace Client
{
    public partial class Form1 : Form
    {
        private RUDPClient? _socket { get; set; } = null;


        private ConcurrentDictionary<string, EndPoint> _connectedSigServers { get; set; } = new();
        private ConcurrentDictionary<string, PeerEPs> _connectedPeers { get; set; } = new();
        private List<EPDetailsInfo> _signalingServersInfos { get; set; } = new();
        private List<EPDetailsInfo> _peersInfos { get; set; } = new();

        private SemaphoreSlim _disconnectionSemaphore { get; set; } = new(1);
        private string _disconnectingNPub { get; set; } = "";


        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            RunBackgroundTask();
        }


        private void btnToggleSocket_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            lbMessage.Text = "";


            if (_socket is null || _socket.Status == SocketStatus.Stopped)
            {
                if (Properties.Settings.Default.ServerPort <= 0)
                {
                    lbMessage.Text = "Port number not properly setted";
                    Cursor = Cursors.Default;
                    return;
                }
                if (string.IsNullOrEmpty(Properties.Settings.Default.NSecBech32))
                {
                    lbMessage.Text = "Key Pair (NSec/Npub) not properly setted";
                    Cursor = Cursors.Default;
                    return;
                }

                _socket = new RUDPClient(false, Properties.Settings.Default.ServerPort, Properties.Settings.Default.NSecBech32);
                _socket.SetMaxUploadSpeed(Properties.Settings.Default.MaxUploadSpeed);
                BindEvents();
                if (_socket.Start() != SocketStatus.Running)
                    lbMessage.Text = "Unable to start this Client Instance";
                else
                {
                    lbNPub.Text = NSec.FromBech32(Properties.Settings.Default.NSecBech32).DerivePublicKey().Bech32;
                    lbStatus.Text = _socket.Status.ToString();
                    lbLocalEndPoint.Text = _socket.LocalEndpoint is null ? "- not defined -" : _socket.LocalEndpoint.ToIPV4String();

                    RunBackgroundTask();

                    if (!string.IsNullOrEmpty(Properties.Settings.Default.SigServer1))
                        _socket.TryConnectWithSignalingServer(Properties.Settings.Default.SigServer1.ToEndPoint(), 1);
                    if (!string.IsNullOrEmpty(Properties.Settings.Default.SigServer2))
                        _socket.TryConnectWithSignalingServer(Properties.Settings.Default.SigServer2.ToEndPoint(), 2);
                    if (!string.IsNullOrEmpty(Properties.Settings.Default.SigServer3))
                        _socket.TryConnectWithSignalingServer(Properties.Settings.Default.SigServer3.ToEndPoint(), 3);
                }

                btnToggleSocket.Text = "Stop";
                btnToggleSocket.BackColor = Color.IndianRed;
            }
            else
            {
                string content = "Do you want to stop the current Client instance?";
                if (MessageBox.Show(content, "Attention", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                {
                    Cursor = Cursors.Default;
                    return;
                }

                UnbindEvents();
                if (_socket.Stop() != SocketStatus.Stopped)
                    lbMessage.Text = "Unable to turn off this Signaling Server Instance";
                else
                {
                    lbNPub.Text = "";
                    lbStatus.Text = "";
                    lbLocalEndPoint.Text = "";
                    lbTotConnectedSigServers.Text = "0";
                    lbTotConnectedPeers.Text = "0";
                    lbBytesUpload.Text = "0";
                    lbPacketsUpload.Text = "0";
                    lbBytesDownload.Text = "0";
                    lbPacketsDownload.Text = "0";
                }

                _signalingServersInfos.Clear();
                _peersInfos.Clear();


                btnToggleSocket.Text = "Start";
                btnToggleSocket.BackColor = Color.LightGreen;

                tabs.SelectedIndex = 0;
            }


            tabs.Enabled = _socket is not null && _socket.Status == SocketStatus.Running;
            Cursor = Cursors.Default;
        }
        private void BindEvents()
        {
            if (_socket is null)
                return;
            _socket.OnP2PConnectionRequest += _socket_OnP2PConnectionRequest;
            _socket.OnConnectionConfirmed += _socket_OnConnectionConfirmed;
            _socket.OnConnectionClosed += _socket_OnConnectionClosed;
            _socket.OnRateUpdated += _socket_OnRateUpdated;
        }
        private void UnbindEvents()
        {
            if (_socket is null)
                return;
            _socket.OnP2PConnectionRequest -= _socket_OnP2PConnectionRequest;
            _socket.OnConnectionConfirmed -= _socket_OnConnectionConfirmed;
            _socket.OnConnectionClosed -= _socket_OnConnectionClosed;
            _socket.OnRateUpdated += _socket_OnRateUpdated;
        }
        private void RunBackgroundTask()
        {
            //Task.Factory.StartNew(() =>
            //{
            //    Stopwatch _backgroundTaskTimer = Stopwatch.StartNew();

            //    while (_socket is not null)
            //    {
            //        while (_backgroundTaskTimer.Elapsed.TotalMilliseconds < 500)
            //            ThreadUtilities.PauseThread(100);

            //        BeginInvoke((MethodInvoker)delegate
            //        {
            //            bsSignalingServers.DataSource = null;
            //            bsSignalingServers.DataSource = _signalingServersInfos;
            //            bsSignalingServers.ResetBindings(false);

            //            bsPeers.DataSource = null;
            //            bsPeers.DataSource = _peersInfos;
            //            bsPeers.ResetBindings(false);
            //        });

            //        _backgroundTaskTimer.Restart();
            //    }
            //    _backgroundTaskTimer.Stop();
            //}, TaskCreationOptions.LongRunning);
        }


        private void _socket_OnP2PConnectionRequest(EndPoint ep, string bech32NPub)
        {
            BeginInvoke((MethodInvoker)delegate
            {
                Activate();
                Focus();
                string content = $"{ep.ToIPV4String()} wants to connect with you. Do you want to allow it?";
                if (MessageBox.Show(content, "Attention", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                    return;
                _socket.TryConnectWithPeer(ep);
            });
        }
        private void _socket_OnConnectionConfirmed(EndPoint ep, string bech32NPub, bool isSigServer, byte relativeIndex)
        {
            BeginInvoke((MethodInvoker)delegate
            {
                lbTotConnectedSigServers.Text = _socket.TotalConnectedSigServers.ToString();
                lbTotConnectedPeers.Text = _socket.TotalConnectedPeers.ToString();


                if (isSigServer)
                {
                    _connectedSigServers.AddOrUpdate(
                        bech32NPub,
                        addValue: ep,
                        updateValueFactory: (npub, eps) => ep
                    );
                    lbMessage.Text = $"Signaling Server connected with endpoint {ep.ToIPV4String()}";
                }
                else
                {
                    _connectedPeers.AddOrUpdate(
                        bech32NPub,
                        addValue: new(ep, relativeIndex, true),
                        updateValueFactory: (npub, eps) => eps.SetRelativeIndexEndPoint(ep, relativeIndex, true)
                    );
                    lbMessage.Text = $"Peer connected with endpoint {ep.ToIPV4String()}";


                    Task.Factory.StartNew(async () =>
                    {
                        EndPoint localEP = ep;
                        string bech32 = bech32NPub;
                        while (_connectedPeers.ContainsKey(bech32) && _socket is not null)
                            try
                            {
                                _disconnectionSemaphore.Wait();
                                if (!string.IsNullOrEmpty(_disconnectingNPub) && _disconnectingNPub == bech32)
                                {
                                    _disconnectingNPub = "";
                                    break;
                                }
                                _socket.SendData(localEP, new byte[500]);
                                _disconnectionSemaphore.Release();
                            }
                            catch {
                                _disconnectionSemaphore.Release();
                            }
                    }, TaskCreationOptions.LongRunning);
                }
            });
        }
        private void _socket_OnConnectionClosed(EndPoint ep, string bech32NPub)
        {
            BeginInvoke((MethodInvoker)delegate
            {
                _disconnectionSemaphore.Wait();
                _disconnectingNPub = bech32NPub;
                _disconnectionSemaphore.Release();


                lbTotConnectedSigServers.Text = _socket.TotalConnectedSigServers.ToString();
                lbTotConnectedPeers.Text = _socket.TotalConnectedPeers.ToString();

                if (_connectedSigServers.ContainsKey(bech32NPub))
                    _connectedSigServers.TryRemove(bech32NPub, out _);
                if (_connectedPeers.ContainsKey(bech32NPub))
                    _connectedPeers.TryRemove(bech32NPub, out _);

                _signalingServersInfos.RemoveAll(x => x.EndPoint.Equals(ep));
                _peersInfos.RemoveAll(x => x.EndPoint.Equals(ep));
            });
        }
        private void _socket_OnRateUpdated(Rates rates, List<EPInfo> epsRates, int sendBufferFillPercentage)
        {
            BeginInvoke((MethodInvoker)delegate
            {
                lbBytesUpload.Text = new ByteSize(rates.SentBytesPerSecond).ToString() + "/s";
                lbPacketsUpload.Text = rates.SentPacketsPerSecond.ToString() + " packet/s";
                lbBytesDownload.Text = new ByteSize(rates.ReceivedBytesPerSecond).ToString() + "/s";
                lbPacketsDownload.Text = rates.ReceivedPacketsPerSecond.ToString() + " packet/s";
            });
        }
        private void _socket_OnRateUpdated(EndPoint ep, EPInfo info)
        {
            //BeginInvoke((MethodInvoker)delegate
            //{
            //    if (info.IsSigServer)
            //    {
            //        EPInfo? sigServ = _signalingServersInfos.FirstOrDefault(x => x.EndPoint.EqualTo(ep));
            //        if (sigServ is null)
            //            _signalingServersInfos.Add(new EPDetailsInfo(info));
            //        else
            //            sigServ = info;
            //    }
            //    else
            //    {
            //        EPInfo? peer = _peersInfos.FirstOrDefault(x => x.EndPoint.EqualTo(ep));
            //        if (peer is null)
            //            _peersInfos.Add(new EPDetailsInfo(info));
            //        else
            //            peer = info;
            //    }
            //});
        }


        private void keysManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new KeysManagement().ShowDialog();
        }
        private void clientToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new LocalClientSettings(_socket).ShowDialog();
        }
        private void nostrContactsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Properties.Settings.Default.NSecBech32))
            {
                lbMessage.Text = "Key Pair (NSec/Npub) not properly setted";
                return;
            }

            new NostrContacts(_socket).ShowDialog();
        }


        private void dgvPeers_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 11)
            {
                EPDetailsInfo item = _peersInfos.ElementAt(e.RowIndex);
                string content = "Do you want to close and delete the connection with this Peer?";
                if (MessageBox.Show(content, "Attention", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                    return;
                _socket.DisconnectFrom(item.EndPoint);
            }
        }
    }
}
