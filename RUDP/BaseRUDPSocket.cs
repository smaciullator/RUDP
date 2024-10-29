using RUDP.Enums;
using RUDP.Extensions;
using RUDP.Keys;
using RUDP.Models;
using RUDP.Utilities;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace RUDP
{
    public class BaseRUDPSocket : IDisposable
    {
        public delegate void EventHandler();
        public delegate void EventHandler<T1>(T1 p1);
        public delegate void EventHandler<T1, T2>(T1 p1, T2 p2);
        public delegate void EventHandler<T1, T2, T3>(T1 p1, T2 p2, T3 p3);
        public delegate void EventHandler<T1, T2, T3, T4>(T1 p1, T2 p2, T3 p3, T4 p4);
        public event EventHandler<Exception> OnException;
        /// <summary>
        /// Triggered when a connection is succesfully completed (path found, mtu negotiation, identities exchanged
        /// </summary>
        public event EventHandler<EndPoint, string, bool, byte> OnConnectionConfirmed;
        public event EventHandler<EndPoint> OnConnectionFailed;
        public event EventHandler<EndPoint, string> OnConnectionClosed;
        public event EventHandler<EndPoint, string, byte> OnSignalingPropagation;
        public event EventHandler<EndPoint, string, string> OnP2PCoordinationRequest;
        public event EventHandler<EndPoint, string> OnUnknownIdentity;
        /// <summary>
        /// Triggered when a viable network path has been found to connect with a peer.
        /// NOTE: only triggered if i first requested the P2P coordination (i asked for this connection)
        /// </summary>
        public event EventHandler<EndPoint, string> OnP2PConnectionPossible;
        /// <summary>
        /// Triggered when a viable network path has been found to connect with a peer.
        /// NOTE: only triggered if i received the P2P coordination (a peer is asking to connect)
        /// </summary>
        public event EventHandler<EndPoint, string> OnP2PConnectionRequest;
        public event EventHandler<EndPoint, string, byte[], long> OnData;
        public event EventHandler<EndPoint, string, byte[], long> OnStream;
        /// <summary>
        /// Triggered when a file reception has succesfully ended
        /// </summary>
        public event EventHandler<EndPoint, string, string> OnFile;
        /// <summary>
        /// Return each second some informations about the total upload/download rates, details for each single connected endpoint
        /// and a value indicating the fill percentage of the send buffer.
        /// </summary>
        public event EventHandler<Rates, List<EPInfo>, int> OnRateUpdated;


        public SocketStatus Status => socket is null ? SocketStatus.NotInitialized : socket.Status;
        public EndPoint? LocalEndpoint => socket is null ? null : socket.LocalEP;
        public int TotalConnectedSigServers => _epsInfo.Where(x => x.Value.IsSigServer && x.Value.IsConnected).Count();
        public int TotalConnectedPeers => _epsInfo.Where(x => !x.Value.IsSigServer && x.Value.IsConnected).Count();
        public IEnumerable<EPInfo> SignalingServersNetwork => _epsInfo.Where(x => x.Value.IsSigServer).Select(x => x.Value);
        public IEnumerable<EPInfo> PeersNetwork => _epsInfo.Where(x => !x.Value.IsSigServer).Select(x => x.Value);
        internal KeyPair Identity { get; set; }
        public string Bech32NPub => Identity is not null ? Identity.NPub.Bech32 : "";


        private MultiThreadUDPSocket? socket { get; set; } = null;
        private bool SigServer { get; set; } = false;
        private ConcurrentDictionary<EndPoint, EPInfo> _epsInfo { get; set; } = new();
        private ConcurrentDictionary<EndPoint, List<EndPoint>> _coordinatingServers { get; set; } = new();
        private ConcurrentDictionary<string, byte> _connectingToNPubs { get; set; } = new();
        private ConcurrentDictionary<EndPoint, long> _recentlyDisconnectedEndpoints { get; set; } = new();


        private string _defaultTempFileFolder { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RUDPSocket_TEMP");
        private ConcurrentDictionary<uint, FileData> _sendingFiles { get; set; } = new();
        private ConcurrentDictionary<uint, FileData> _receivingFiles { get; set; } = new();


        public BaseRUDPSocket(bool sigServer, int customPort = 0, string? bech32NSec = null)
        {
            SigServer = sigServer;
            socket = new MultiThreadUDPSocket(customPort);
            BindEvents();
            SetMyIdentity(bech32NSec);
        }
        public SocketStatus Start()
        {
            if (socket is null)
                return SocketStatus.NotInitialized;
            SocketStatus newStatus = socket.Start();
            TryStartBackgroundTasks();
            return newStatus;
        }
        public SocketStatus Restart()
        {
            if (socket is null)
                return SocketStatus.NotInitialized;
            return socket.Restart();
        }
        public SocketStatus Stop()
        {
            if (socket is null)
                return SocketStatus.NotInitialized;

            foreach (KeyValuePair<EndPoint, EPInfo> ep in _epsInfo)
                DisconnectFrom(ep.Key);

            // We try to wait up to 5 seconds for each peer to be safely disconnected
            Stopwatch limit = Stopwatch.StartNew();
            while (limit.Elapsed.TotalSeconds < 10 && _epsInfo.Count > 0)
                ThreadUtilities.PauseThread(50);
            limit.Stop();

            return socket.Stop();
        }


        public bool SetMyIdentity(string? bech32NSec)
        {
            try
            {
                NSec nsec = NSec.FromBech32(bech32NSec);
                Identity = string.IsNullOrEmpty(nsec.Bech32) ? KeyPair.GenerateNew() : KeyPair.From(nsec);
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Set the global upload speed limit for this socket instance, calculated with the sum of all bytes sent per second to
        /// Expressed in bytes
        /// </summary>
        /// <param name="maxUploadSpeed">Expressed in bytes</param>
        public void SetMaxUploadSpeed(double maxUploadSpeed)
        {
            socket?.SetMaxUploadSpeed(maxUploadSpeed);
        }
        /// <summary>
        /// Set the upload speed limit for a specific endpoint.
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="maxUploadSpeed">Expressed in bytes</param>
        public void SetMaxEndPointUploadSpeed(EndPoint ep, double maxUploadSpeed)
        {
            if (!_epsInfo.ContainsKey(ep))
                return;
            _epsInfo[ep].MaxUploadSpeed = maxUploadSpeed;
            socket?.SetEPMaxUploadSpeed(ep, maxUploadSpeed);
        }


        /// <summary>
        /// To be used to begin an MTU Size negotiation between you and the desired endpoint.
        /// Can be called to try the connection with a well-knwon Signaling Server's endpoint,
        /// or right after a CNP (Connection Possible) packet when trying to connect with a peer
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="relativeIndex">0 = try connect with a Peer, 1,2,3 = try connect with a signaling server. If not specified and sent to a Signaling Server, the value will be interpreted as index 1</param>
        public void TryConnectWith(EndPoint ep, byte relativeIndex = 0)
        {
            // It create the new EndPoint informations, or not if already exist
            _epsInfo.AddOrUpdate(
                ep,
                addValue: new(ep),
                updateValueFactory: (endpoint, value) => value
            );

            if (_epsInfo[ep].IsConnected || (_epsInfo[ep].AmIConnecting.HasValue && _epsInfo[ep].AmIConnecting.Value))
                return;

            _epsInfo[ep].SetAmIConnecting(true);
            SendMTUDiscovery(ep, Convert.ToInt32(_epsInfo[ep].MTUSize), relativeIndex);

            Task.Factory.StartNew(() =>
            {
                Stopwatch limit = Stopwatch.StartNew();
                while (limit.Elapsed.TotalSeconds < 10 && !_epsInfo[ep].IsConnected)
                    ThreadUtilities.PauseThread(50);
                limit.Stop();

                if (!_epsInfo[ep].IsConnected)
                {
                    _epsInfo.TryRemove(ep, out EPInfo? info);
                    if (info is not null)
                        info.Dispose();
                    OnConnectionFailed?.Invoke(ep);
                }
            }, TaskCreationOptions.LongRunning);
        }
        public void TryConnectLocallyWith(EndPoint ep, byte relativeIndex = 0)
        {
            Task.Factory.StartNew(() =>
            {
                Stopwatch limit = Stopwatch.StartNew();
                while (limit.Elapsed.TotalSeconds < 5 && !_epsInfo.ContainsKey(ep))
                {
                    // Send multiple packet in case they get dropped
                    SendConnectionPossible(ep);
                    ThreadUtilities.PauseThread(50);
                }
                SendConnectionPossible(ep);
                limit.Stop();

                if (!_epsInfo.ContainsKey(ep))
                    OnConnectionFailed?.Invoke(ep);

            }, TaskCreationOptions.LongRunning);
        }
        /// <summary>
        /// Used to begin a collaborative disconnection
        /// </summary>
        /// <param name="ep"></param>
        public void DisconnectFrom(EndPoint ep)
        {
            if (socket is null || !_epsInfo.ContainsKey(ep))
                return;
            _epsInfo[ep].SetAmIDisconnecting(true);
            SendDisconnection(ep);
        }
        private void RemovePeerConnection(EndPoint ep)
        {
            NPub? disconnectingNPub = GetEPNPub(ep);
            string disconnectedNPubBech32 = disconnectingNPub is null ? "" : disconnectingNPub.Bech32;
            // Recently disconnected eps get stacked on a temporary buffer to prevent further send/receive for a short amount of time
            _recentlyDisconnectedEndpoints.TryAdd(ep, DateTime.Now.Ticks);
            _epsInfo[ep].Dispose();
            _epsInfo.TryRemove(ep, out _);
            socket?.DisconnectEndPoint(ep);
            OnConnectionClosed?.Invoke(ep, disconnectedNPubBech32);

            //ThreadUtilities.PauseThread(1000);
        }


        /// <summary>
        /// Send some data to the specified endpoint.
        /// This packet could eventually be splitted into multiple chunks depending on its size and current MTU size with the endpoint.
        /// Also, this specific packet will trigger a confirmation packet (ACK) sent by the recipient when received.
        /// </summary>
        /// <param name="sendTo"></param>
        /// <param name="rawData"></param>
        /// <returns></returns>
        public bool SendData(EndPoint sendTo, byte[] rawData)
        {
            if (!_epsInfo.ContainsKey(sendTo))
                return false;
            byte[]? data = Body.TryEncryptData(Identity, GetEPNPub(sendTo), rawData, out byte[] ivBytes);
            if (data is null)
                return false;
            return Send(sendTo, Header.DATA(_epsInfo[sendTo].GetNextSendNumeration(), 0, ivBytes), data, true);
        }
        /// <summary>
        /// Send a stream to the specified endpoint.
        /// This packet could eventually be splitted into multiple chunks depending on its size and current MTU size with the endpoint.
        /// This packet will never be acknowledged, so if it gets lost it will not be sent again.
        /// </summary>
        /// <param name="sendTo"></param>
        /// <param name="rawData"></param>
        /// <returns></returns>
        public bool SendStream(EndPoint sendTo, byte[] rawData)
        {
            if (!_epsInfo.ContainsKey(sendTo))
                return false;
            byte[]? data = Body.TryEncryptData(Identity, GetEPNPub(sendTo), rawData, out byte[] ivBytes);
            if (data is null)
                return false;
            return Send(sendTo, Header.STREAM(ivBytes), data);
        }
        /// <summary>
        /// Send an RTTA Packet and store its timestamp for subsequent channel latency statistics
        /// </summary>
        /// <param name="sendTo"></param>
        /// <returns></returns>
        private bool SendRTTA(EndPoint sendTo)
        {
            if (!_epsInfo.ContainsKey(sendTo))
                return false;
            uint num = _epsInfo[sendTo].GetNextSendNumeration();
            _epsInfo.AddOrUpdate(sendTo, addValue: new(sendTo), updateValueFactory: (endpoint, value) => value.AddRTTA(num));
            return Send(sendTo, Header.RTTA(num));
        }
        /// <summary>
        /// Send an RTTB packet with the same packet identifier of the corresponding RTTA packet received
        /// </summary>
        /// <param name="sendTo"></param>
        /// <param name="RTTA"></param>
        /// <returns></returns>
        private bool SendRTTB(EndPoint sendTo, byte[] RTTA)
        {
            if (!_epsInfo.ContainsKey(sendTo))
                return false;
            uint? num = Header.Deserialize(RTTA).PacketIdentifier;
            if (num.HasValue)
                return Send(sendTo, Header.RTTB(num.Value));
            return false;
        }
        /// <summary>
        /// Send a P2PR (Peer To Peer Coordination Request) to the specified endpoint.
        /// NOTE: the endpoint has to be an already connected Signaling Server
        /// </summary>
        /// <param name="sendTo"></param>
        /// <param name="bech32PeerNPub"></param>
        /// <returns></returns>
        public bool SendP2PCoordinationRequest(EndPoint sendTo, string bech32PeerNPub)
        {
            if (!_epsInfo.ContainsKey(sendTo) || !_epsInfo[sendTo].IsSigServer)
                return false;

            try
            {
                NPub npub = NPub.FromBech32(bech32PeerNPub);
                byte[]? data = Body.TryEncryptData(Identity, GetEPNPub(sendTo), Body.P2P_COORDINATION_REQUEST(npub), out byte[] ivBytes);
                if (data is null)
                    return false;
                _connectingToNPubs.TryAdd(bech32PeerNPub, 0);
                return Send(sendTo, Header.P2P_COORDINATION_REQUEST(_epsInfo[sendTo].GetNextSendNumeration(), ivBytes), data, true);
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Used by Signaling Servers to answer a P2PR (Peer To Peer Coordination Request) from a peer
        /// who requested an NPub that the Signaling Server doesn't know.
        /// </summary>
        /// <param name="sendTo"></param>
        /// <param name="bech32PeerNPub"></param>
        /// <returns></returns>
        public bool SendUnknownIdentity(EndPoint sendTo, string bech32PeerNPub)
        {
            if (!_epsInfo.ContainsKey(sendTo))
                return false;
            try
            {
                NPub npub = NPub.FromBech32(bech32PeerNPub);
                byte[]? data = Body.TryEncryptData(Identity, GetEPNPub(sendTo), Body.UNKNOWN_IDENTITY(npub), out byte[] ivBytes);
                if (data is null)
                    return false;
                return Send(sendTo, Header.UNKNOWN_IDENTITY(_epsInfo[sendTo].GetNextSendNumeration(), ivBytes), data, true);
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Used by Signaling Server to coordinate 2 peers on they attempt to connect P2P
        /// </summary>
        /// <param name="sendTo"></param>
        /// <param name="bech32PeerNPub"></param>
        /// <param name="peerEP1"></param>
        /// <param name="peerEP2"></param>
        /// <param name="peerEP3"></param>
        /// <returns></returns>
        public bool SendP2PConnectionCoordination(EndPoint sendTo, string bech32PeerNPub, EndPoint? peerEP1, EndPoint? peerEP2, EndPoint? peerEP3)
        {
            try
            {
                for (int i = 0; i < 10; i++)
                    if (!Send(sendTo, Header.P2P_CONNECTION_COORDINATION(), Body.P2P_CONNECTION_COORDINATION(NPub.FromBech32(bech32PeerNPub), peerEP1, peerEP2, peerEP3), false))
                        return false;
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Used by the peers during the UDP Hole Punching phase, it's a 1 byte packet
        /// </summary>
        /// <param name="sendTo"></param>
        /// <returns></returns>
        private bool SendConnectionPossible(EndPoint sendTo)
        {
            // NOTE: this packet MUST be sent even if we don't already have it on _epsInfo
            return Send(sendTo, Header.CONNECTION_POSSIBLE(), Body.CONNECTION_POSSIBLE(Identity.NPub), false);

            //// We send directly because we don't yet know for sure the peer endpoint
            //byte[] header = Header.CONNECTION_POSSIBLE().Serialize();
            //byte[] body = Body.CONNECTION_POSSIBLE(Identity.NPub);
            //byte[] packet = new byte[header.Length + body.Length];
            //Array.Copy(header, 0, packet, 0, header.Length);
            //Array.Copy(body, 0, packet, header.Length, body.Length);
            //return socket.Send(sendTo, packet);
        }
        /// <summary>
        /// Send the ACKL packet to the specified endpoint.
        /// Also takes the original full packet (header + data) received to parse the header
        /// </summary>
        /// <param name="sendTo"></param>
        /// <param name="pkt"></param>
        /// <returns></returns>
        private bool SendAcknowledge(EndPoint sendTo, byte[] pkt)
        {
            if (!_epsInfo.ContainsKey(sendTo))
                return false;
            Header header = Header.Deserialize(pkt);
            return Send(sendTo, Header.ACKNOWLEDGEMENT(header.PacketIdentifier ?? 0, header.ChunkNumber));
        }
        /// <summary>
        /// Send an MTUD (MTU Discovery) packet to begin the channel size discovery with a peer
        /// </summary>
        /// <param name="sendTo"></param>
        /// <param name="dataSize"></param>
        /// <param name="relativeIndex"></param>
        /// <returns></returns>
        private bool SendMTUDiscovery(EndPoint sendTo, int dataSize, byte relativeIndex = 0)
        {
            if (!_epsInfo.ContainsKey(sendTo))
                return false;
            return Send(sendTo, Header.MTU_DISCOVERY(), Body.MTU_DISCOVERY(dataSize, Identity.NPub, relativeIndex), true);
        }
        /// <summary>
        /// Send an MTUF (MTU Found) packet after an MTUD is received, to end the channel size discovery and set the size on both peers.
        /// </summary>
        /// <param name="sendTo"></param>
        /// <returns></returns>
        private bool SendMTUFound(EndPoint sendTo, ushort dataLenght)
        {
            if (!_epsInfo.ContainsKey(sendTo))
                return false;
            return Send(sendTo, Header.MTU_FOUND(_epsInfo[sendTo].GetNextSendNumeration()), Body.MTU_FOUND(dataLenght, Identity.NPub), true);
        }
        /// <summary>
        /// Try to send an HNDS (Handshake) packet with the current secret set for this endpoint.
        /// In case the secret doesn't exists nothing gets sent and return false.
        /// </summary>
        /// <param name="sendTo"></param>
        /// <returns></returns>
        private bool SendHandShake(EndPoint sendTo, int? sentSecret, int? receivedSecret)
        {
            if (!_epsInfo.ContainsKey(sendTo))
                return false;
            byte[]? data = Body.TryEncryptData(Identity, GetEPNPub(sendTo), Body.HANDSHAKE(sentSecret, receivedSecret), out byte[] ivBytes);
            if (data is null)
                return false;
            return Send(sendTo, Header.HANDSHAKE(_epsInfo[sendTo].GetNextSendNumeration(), ivBytes), data, true);
        }
        /// <summary>
        /// Send a CNCF (Connection Confirm) packet to end the MTU Size Negotiation + Identity Check Phase
        /// </summary>
        /// <param name="sendTo"></param>
        /// <returns></returns>
        private bool SendConnectionConfirm(EndPoint sendTo)
        {
            if (!_epsInfo.ContainsKey(sendTo))
                return false;
            byte[]? data = Body.TryEncryptData(Identity, GetEPNPub(sendTo), Body.CONNECTION_CONFIRM(SigServer), out byte[] ivBytes);
            if (data is null)
                return false;
            return Send(sendTo, Header.CONNECTION_CONFIRM(_epsInfo[sendTo].GetNextSendNumeration(), ivBytes), data, true);
        }
        /// <summary>
        /// Try to send a DISCONNECTION packet to do a collaborative disconnection with the other peer
        /// </summary>
        /// <param name="sendTo"></param>
        /// <returns></returns>
        private bool SendDisconnection(EndPoint sendTo)
        {
            if (!_epsInfo.ContainsKey(sendTo) || !_epsInfo[sendTo].IsConnected)
                return false;
            byte[]? data = Body.TryEncryptData(Identity, GetEPNPub(sendTo), Body.DISCONNECTION(_epsInfo[sendTo].SentSecret.Value, _epsInfo[sendTo].ReceivedSecret.Value), out byte[] ivBytes);
            if (data is null)
                return false;
            return Send(sendTo, Header.DISCONNECTION(_epsInfo[sendTo].GetNextSendNumeration(), ivBytes), data, true);
        }
        /// <summary>
        /// Try to send a propagation packet from this Signaling Server to every other known and connected Signaling Servers
        /// </summary>
        /// <param name="peerEP">The endpoint of a peer we want to propagate</param>
        /// <param name="relativeIndex">The relative index we received this peer ep from</param>
        /// <returns></returns>
        public void SendSignalingPropagation(EndPoint peerEP, byte relativeIndex)
        {
            foreach (KeyValuePair<EndPoint, EPInfo> sigSer in _epsInfo.Where(x => x.Value.IsSigServer && x.Value.IsConnected))
                SendSignalingPropagation(sigSer.Key, peerEP, relativeIndex);
        }
        /// <summary>
        /// Try to send a propagation packet from this Signaling Server to another Signaling Server
        /// </summary>
        /// <param name="sendTo">The endpoint of the Signaling Server we want to inform</param>
        /// <param name="peerEP">The endpoint of a peer we want to propagate</param>
        /// <param name="relativeIndex">The relative index we received this peer ep from</param>
        /// <returns></returns>
        public bool SendSignalingPropagation(EndPoint sendTo, EndPoint peerEP, byte relativeIndex)
        {
            if (!SigServer || !_epsInfo.ContainsKey(sendTo) || !_epsInfo[sendTo].IsConnected || !_epsInfo[sendTo].IsSigServer)
                return false;
            byte[]? data = Body.TryEncryptData(Identity, GetEPNPub(sendTo), Body.SIGNALING_PROPAGATION(GetEPNPub(peerEP), peerEP, relativeIndex), out byte[] ivBytes);
            if (data is null)
                return false;
            return Send(sendTo, Header.SIGNALING_PROPAGATION(_epsInfo[sendTo].GetNextSendNumeration(), ivBytes), data, true);
        }

        #region Files
        public bool SendFile(EndPoint sendTo, string fileFullPath)
        {
            using (FileStream file = File.Open(fileFullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                return SendFilePresentation(sendTo, fileFullPath, file.Length);
        }

        private bool SendFilePresentation(EndPoint sendTo, string fileFullPath, long fileSize)
        {
            if (!_epsInfo.ContainsKey(sendTo))
                return false;

            long chunksNumber = fileSize / GetEPMTUSize(sendTo);
            if (fileSize % GetEPMTUSize(sendTo) > 0)
                chunksNumber++;
            byte[] cn = BitConverter.GetBytes(chunksNumber);
            byte[] fileName = Encoding.UTF8.GetBytes(Path.GetFileName(fileFullPath));
            byte[] body = new byte[cn.Length + fileName.Length];
            Array.Copy(cn, 0, body, 0, cn.Length);
            Array.Copy(fileName, 0, body, cn.Length, fileName.Length);

            byte[]? data = Body.TryEncryptData(Identity, GetEPNPub(sendTo), body, out byte[] ivBytes);
            if (data is null)
                return false;
            uint uniqueIdentifier = _epsInfo[sendTo].GetNextSendNumeration();
            _sendingFiles.AddOrUpdate(
                uniqueIdentifier,
                // NOTA: qui inserisco la FULL PATH invece del solo file name per poter successivamente inviare il file
                addValue: new(fileFullPath, chunksNumber),
                updateValueFactory: (identifier, value) => value
            );
            return Send(sendTo, Header.FILE_PRESENTATION(uniqueIdentifier, 0, ivBytes), data, true);
        }
        private bool SendFile(EndPoint sendTo, byte[] chunk)
        {
            if (!_epsInfo.ContainsKey(sendTo))
                return false;
            byte[]? data = Body.TryEncryptData(Identity, GetEPNPub(sendTo), chunk, out byte[] ivBytes);
            if (data is null)
                return false;
            return Send(sendTo, Header.FILE(_epsInfo[sendTo].GetNextSendNumeration(), 0, ivBytes), data, true);
        }

        private void ManageFilePresentation(uint packetIdentifier, byte[] decryptedData)
        {
            // Prendo il nome file
            byte[] cn = new byte[8];
            Array.Copy(decryptedData, 0, cn, 0, 8);
            long chunksNumber = BitConverter.ToInt64(cn);
            byte[] fn = new byte[decryptedData.Length - 8];
            string fileName = Encoding.UTF8.GetString(fn);

            // Creo il file temporaneo
            string fileFullPath = Path.Combine(_defaultTempFileFolder, fileName);
            if (!Directory.Exists(_defaultTempFileFolder))
                Directory.CreateDirectory(_defaultTempFileFolder);
            FileStream fs = File.Create(fileFullPath);

            // Aggiungo il riferimento alla path del file temporaneo
            _receivingFiles.AddOrUpdate(
                packetIdentifier,
                addValue: new(fileFullPath, chunksNumber, fs),
                updateValueFactory: (identifier, value) => value = new(fileFullPath, chunksNumber, fs)
            );
        }
        private void ManageFilePresentationConfirm(EndPoint receivedFrom, uint packetIdentifier)
        {
            if (!_sendingFiles.ContainsKey(packetIdentifier))
                return;
            Task.Run(() =>
            {
                using FileStream file = File.Open(_sendingFiles[packetIdentifier].FileFullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                int readBytes = 0,
                    position = 0;
                byte[] buffer = new byte[GetEPMTUSize(receivedFrom)];
                file.Position = position;
                while ((readBytes = file.Read(buffer, 0, buffer.Length)) > 0)
                {
                    position += readBytes;
                    // Devo essere sicuro che l'ultimo file chunk arrivi, altrimenti se venisse perso non potrei mai completare l'invio file
                    bool endOfFile = file.Position + readBytes == file.Length;

                    if (readBytes < buffer.Length)
                        SendFile(receivedFrom, new Span<byte>(buffer).Slice(0, readBytes).ToArray());
                    else
                        SendFile(receivedFrom, buffer);
                    file.Position = position;
                }
                _sendingFiles.TryRemove(packetIdentifier, out FileData? fd);
                if (fd is not null)
                    fd.Dispose();

                file.Dispose();
            });
        }
        private void ManageFileChunk(EndPoint receivedFrom, uint packetIdentifier, uint chunkNumber, byte[] data)
        {
            if (!_receivingFiles.ContainsKey(packetIdentifier))
                return;

            string bech32 = "";
            if (_epsInfo.ContainsKey(receivedFrom))
                bech32 = _epsInfo[receivedFrom].NPubBech32 ?? "";

            // Non-chunked file
            if (_receivingFiles[packetIdentifier].ChunksNumber == 0 && chunkNumber == 0)
            {
                _receivingFiles[packetIdentifier].Stream.Write(data, 0, data.Length);

                OnFile?.Invoke(receivedFrom, bech32, _receivingFiles[packetIdentifier].Stream.Name);
            }
            // Next adiacent chunk arrived
            else if (_receivingFiles[packetIdentifier].CurrentChunk + 1 == chunkNumber)
            {
                _receivingFiles[packetIdentifier].CurrentChunk++;
                _receivingFiles[packetIdentifier].Stream.Write(data, 0, data.Length);
                // If file ended
                if (_receivingFiles[packetIdentifier].ChunksNumber == chunkNumber)
                {
                    OnFile?.Invoke(receivedFrom, bech32, _receivingFiles[packetIdentifier].Stream.Name);
                    _receivingFiles.TryRemove(packetIdentifier, out FileData? fd);
                    if (fd is not null)
                        fd.Dispose();
                }
            }
            // Subsequent BUT non-adiacent packet arrived
            else
            {
                _receivingFiles[packetIdentifier].EarlyChunks.TryAdd(chunkNumber, data);

                if (_receivingFiles[packetIdentifier].Recovering)
                    return;
                _receivingFiles[packetIdentifier].Recovering = true;
                Task.Run(() =>
                {
                    while (_receivingFiles.ContainsKey(packetIdentifier) && _receivingFiles[packetIdentifier] is not null)
                        try
                        {
                            foreach (KeyValuePair<uint, byte[]> chunk in _receivingFiles[packetIdentifier].EarlyChunks.OrderBy(x => x.Key))
                                if (_receivingFiles[packetIdentifier].ChunksNumber + 1 == chunk.Key)
                                {
                                    _receivingFiles[packetIdentifier].CurrentChunk++;
                                    _receivingFiles[packetIdentifier].Stream.Write(data, 0, data.Length);
                                    _receivingFiles[packetIdentifier].EarlyChunks.TryRemove(chunk.Key, out _);
                                    // If file ended
                                    if (_receivingFiles[packetIdentifier].ChunksNumber == chunk.Key)
                                    {
                                        OnFile?.Invoke(receivedFrom, bech32, _receivingFiles[packetIdentifier].Stream.Name);
                                        _receivingFiles.TryRemove(packetIdentifier, out FileData? fd);
                                        if (fd is not null)
                                            fd.Dispose();
                                    }
                                }
                            ThreadUtilities.PauseThread(100);
                        }
                        catch (Exception) { }
                    _receivingFiles[packetIdentifier].Recovering = false;
                });
            }
        }
        #endregion


        private bool Send(EndPoint sendTo, Header header, byte[]? data = null, bool requireAck = false)
        {
            if (socket is null || _recentlyDisconnectedEndpoints.ContainsKey(sendTo))
                return false;

            List<byte[]>? chunks = GetChunks(sendTo, header, data);
            if (chunks is null)
                return false;
            foreach (byte[] chunk in chunks)
            {
                socket.Send(sendTo, chunk);
                if (requireAck)
                    _epsInfo.AddOrUpdate(sendTo, addValue: new(sendTo), updateValueFactory: (endpoint, value) => value.AddUnackPacket(chunk));
            }
            chunks.Clear();
            return true;
        }
        private List<byte[]>? GetChunks(EndPoint sendTo, Header header, byte[]? data = null)
        {
            // If the current MTU Size is too small we can't procede
            int maxPktSize = GetEPMTUSize(sendTo);
            if (maxPktSize < 65)
                return null;

            List<byte[]> chunks = new();
            int dataLength = data is null ? 0 : data.Length;

            // If this packet doesn't exceed the current MTU Size for the endpoint
            if (header.Length + dataLength <= maxPktSize)
                chunks.Add(PacketUtilities.CreatePacket(header, data));
            // If the packet have to be divided in chunks
            else
            {
                // If the MTU Size is smaller than 65 bytes the protocol doesn't work
                if (data is null)
                    return null;
                // Only DATA and STR packets can be chunked
                if (header.Type != PacketType.DATA && header.Type != PacketType.STREAM)
                    return null;

                int maxDataSize = maxPktSize - (header.Length - (header.IV is null ? 0 : header.IV.Length)); // the IV bytes are sent only on first chunk
                uint chunksNumber = Convert.ToUInt32(data.Length / maxDataSize);
                if (data.Length % maxDataSize > 0)
                    chunksNumber += 1;

                int offset = 0,
                    chunkSize;
                byte[] chunkData;


                // In case we have a DATA packet, only the first packet will send the Aes IV
                if (header.Type == PacketType.DATA)
                {
                    header.ChunkNumber = 1; // The packet sharing the Aes IV always need ChunkNumber = 1 (0 is reserved for non-chunked packets)
                    chunks.Add(PacketUtilities.CreatePacket(header, BitConverter.GetBytes(chunksNumber))); // We also send the total chunks expected
                    header.IV = null; // Now we can safely remove IV to prevent it from appearing on the next chunks
                }


                // All chunks start from number 1 (0 is reserved for IV sharing on DATA packets)
                for (uint i = 1; i <= chunksNumber; i++)
                {
                    chunkSize = Math.Min(data.Length - offset, maxDataSize);
                    chunkData = new byte[chunkSize];
                    for (int j = 0; j < chunkData.Length && (offset + j) < data.Length; j++)
                        chunkData[j] = data[offset + j];

                    // STR packets doesn't need a unique chunk identifier, so we can easily split each chunk
                    if (header.Type == PacketType.STREAM)
                        chunks.Add(PacketUtilities.CreatePacket(header, chunkData));
                    // But DATA packets need a unique numeration for each chunk
                    else if (header.Type == PacketType.DATA)
                    {
                        header.ChunkNumber = i + 1; // +1 because 1 is reserved for IV sharing chunk, and we also count in base 1
                        chunks.Add(PacketUtilities.CreatePacket(header, chunkData));
                    }
                    offset += chunkSize;
                }
            }

            return chunks;
        }


        public int GetEPMTUSize(EndPoint ep)
        {
            return _epsInfo.ContainsKey(ep) ? Convert.ToInt32(_epsInfo[ep].MTUSize) : 500;
        }
        private NPub? GetEPNPub(EndPoint ep)
        {
            return _epsInfo.ContainsKey(ep) ? _epsInfo[ep].NPub : null;
        }
        private bool IsEPTrusted(EndPoint ep)
        {
            return _epsInfo.ContainsKey(ep) ? _epsInfo[ep].IsTrusted : false;
        }
        private void SetEPMTUSize(EndPoint ep, ushort mtuSize)
        {
            _epsInfo.AddOrUpdate(ep, addValue: new(ep, mtuSize), updateValueFactory: (endpoint, value) => value.SetMTUSize(mtuSize));
        }


        private void Socket_OnReceive(EndPoint receivedFrom, byte[] packet, long timestamp)
        {
            if (packet.Length == 0 || _recentlyDisconnectedEndpoints.ContainsKey(receivedFrom))
                return;

            Header header = Header.Deserialize(packet);
            string bech32 = "";
            if (_epsInfo.ContainsKey(receivedFrom))
                bech32 = _epsInfo[receivedFrom].NPubBech32 ?? "";
            switch (header.Type)
            {
                case PacketType.DATA:
                case PacketType.FILE_PRESENTATION:
                case PacketType.FILE:
                    SendAcknowledge(receivedFrom, packet);

                    if (SigServer || !header.PacketIdentifier.HasValue || !header.ChunkNumber.HasValue)
                        break;

                    // If it's not a chunked packet
                    if (header.ChunkNumber == 0)
                    {
                        byte[] decryptedData = Identity.NSec.Decrypt(Body.ExtractFromPacket(packet), header.IV, GetEPNPub(receivedFrom));
                        if (header.Type == PacketType.DATA)
                            OnData?.Invoke(receivedFrom, bech32, decryptedData, timestamp);
                        else if (header.Type == PacketType.FILE_PRESENTATION)
                            ManageFilePresentation(header.PacketIdentifier.Value, decryptedData);
                        else if (header.Type == PacketType.FILE)
                            ManageFileChunk(receivedFrom, header.PacketIdentifier.Value, header.ChunkNumber.HasValue ? header.ChunkNumber.Value : 0, decryptedData);
                    }
                    // If it's a chunk
                    else if (ManageChunks(receivedFrom, header, packet, out byte[] fullDecryptedData))
                    {
                        if (header.Type == PacketType.DATA)
                            OnData?.Invoke(receivedFrom, bech32, fullDecryptedData, timestamp);
                        else if (header.Type == PacketType.FILE_PRESENTATION)
                            ManageFilePresentation(header.PacketIdentifier.Value, fullDecryptedData);
                        else if (header.Type == PacketType.FILE)
                            ManageFileChunk(receivedFrom, header.PacketIdentifier.Value, header.ChunkNumber.HasValue ? header.ChunkNumber.Value : 0, fullDecryptedData);
                    }
                    break;
                case PacketType.STREAM:
                    if (SigServer)
                        break;
                    OnStream?.Invoke(receivedFrom, bech32, Identity.NSec.Decrypt(Body.ExtractFromPacket(packet), header.IV, GetEPNPub(receivedFrom)), timestamp);
                    break;
                case PacketType.ACKNOWLEDGEMENT:
                    if (!_epsInfo.ContainsKey(receivedFrom))
                        break;
                    _epsInfo[receivedFrom]._unack.TryRemove(new UnackData(packet).UID, out UnackData? data);
                    if (data is not null)
                    {
                        if (data.Timestamp.HasValue)
                            socket?.SetEPCongestionWindow(receivedFrom, _epsInfo[receivedFrom].CalculateCongestionWindow((DateTime.Now.Ticks - Convert.ToDouble(data.Timestamp)) / 10000));

                        if (data.PacketType is not null && data.PacketIdentifier.HasValue)
                        {
                            // FILE_PRESENTATION has been confirmed, so i start sending the file data
                            if (data.PacketType == PacketType.FILE_PRESENTATION)
                                ManageFilePresentationConfirm(receivedFrom, data.PacketIdentifier.Value);
                            // A FILE (probably a chunk) has been confirmed, so i check if the file sending has ended
                            else if (data.PacketType == PacketType.FILE && data.ChunkNumber.HasValue)
                            {
                                if (_sendingFiles.ContainsKey(data.PacketIdentifier.Value)
                                    && _sendingFiles[data.PacketIdentifier.Value].ChunksNumber == Convert.ToInt64(data.ChunkNumber.Value))
                                {
                                }
                            }
                        }

                        data.Dispose();
                    }
                    break;
                case PacketType.RTTA:
                    SendRTTB(receivedFrom, packet);
                    break;
                case PacketType.RTTB:
                    uint? num = Header.Deserialize(packet).PacketIdentifier;
                    if (!num.HasValue)
                        break;

                    _epsInfo[receivedFrom]._rttaBuffer.TryRemove(num.Value, out long rttaTimestamp);
                    _epsInfo[receivedFrom].ClearEarlierRTTAs(rttaTimestamp);
                    socket?.SetEPCongestionWindow(receivedFrom, _epsInfo[receivedFrom].CalculateCongestionWindow((DateTime.Now.Ticks - rttaTimestamp) / 10000));
                    break;
                case PacketType.P2P_COORDINATION_REQUEST:
                    SendAcknowledge(receivedFrom, packet);

                    if (!SigServer)
                        break;
                    NPub? requesterNPub = GetEPNPub(receivedFrom);
                    if (
                        requesterNPub is null
                        || !Body.P2P_COORDINATION_REQUEST(packet, Identity.NSec, requesterNPub, out string? targetBech32NPub)
                        || string.IsNullOrEmpty(targetBech32NPub)
                    )
                        break;
                    OnP2PCoordinationRequest?.Invoke(receivedFrom, requesterNPub.Bech32, targetBech32NPub);
                    break;
                case PacketType.UNKNOWN_IDENTITY:
                    SendAcknowledge(receivedFrom, packet);

                    // Decrypt and excract body content
                    if (!Body.UNKNOWN_IDENTITY(packet, Identity.NSec, GetEPNPub(receivedFrom), out string? bech32UnknownNPub) || bech32UnknownNPub == null)
                        break;

                    OnUnknownIdentity?.Invoke(receivedFrom, bech32UnknownNPub);
                    break;
                case PacketType.P2P_CONNECTION_COORDINATION:
                    SendAcknowledge(receivedFrom, packet);

                    if (
                        SigServer
                        || !Body.P2P_CONNECTION_COORDINATION(packet, out NPub? p2pcNPub, out EndPoint? ep1, out EndPoint? ep2, out EndPoint? ep3)
                        || p2pcNPub == null
                        || ep1 is null
                    )
                        break;

                    // If we are receiving a coordination from a not-connected Signaling Server
                    //if (!_epsInfo.ContainsKey(receivedFrom))
                    //{
                    //    if (!_coordinatingServers.ContainsKey(receivedFrom))
                    //        _coordinatingServers.TryAdd(receivedFrom, new());
                    //    // If we already received a not yet finished coordination from this Signaling Server for this specific endpoint
                    //    if (_coordinatingServers[receivedFrom].Contains(ep1))
                    //        break;
                    //    // If this is the first time we receive this coordination from this Signaling Server
                    //    _coordinatingServers.TryAdd(receivedFrom, new() { ep1 });
                    //}

                    if (!_coordinatingServers.ContainsKey(receivedFrom))
                        _coordinatingServers.TryAdd(receivedFrom, new());
                    // If we already received a not yet finished coordination from this Signaling Server for this specific endpoint
                    if (_coordinatingServers[receivedFrom].Contains(ep1))
                        break;
                    // If this is the first time we receive this coordination from this Signaling Server
                    _coordinatingServers[receivedFrom].Add(ep1);


                    // TODO: setup whitelist/blacklist
                    TryUdpHolePunch(receivedFrom, ep1, ep2, ep3);
                    break;
                case PacketType.CONNECTION_POSSIBLE:
                    if (SigServer || (_epsInfo.ContainsKey(receivedFrom) && _epsInfo[receivedFrom].IsConnectionPossible))
                        break;

                    if (!Body.CONNECTION_POSSIBLE(packet, out string? bech32CpNPub) || string.IsNullOrEmpty(bech32CpNPub))
                        break;

                    _epsInfo.TryAdd(receivedFrom, new(receivedFrom));
                    _epsInfo[receivedFrom].SetIsConnectionPossible(true);
                    _epsInfo[receivedFrom].SetNPub(NPub.FromBech32(bech32CpNPub));

                    if (_connectingToNPubs.ContainsKey(bech32CpNPub))
                        OnP2PConnectionPossible?.Invoke(receivedFrom, bech32CpNPub);
                    else
                        OnP2PConnectionRequest?.Invoke(receivedFrom, bech32CpNPub);
                    break;
                case PacketType.MTU_DISCOVERY:
                    SendAcknowledge(receivedFrom, packet);
                    // MTU Size is not big enough
                    if (packet.Length < 65)
                        break;
                    if (!Body.MTU_DISCOVERY(packet, out NPub? npubMTUD, out byte relativeIndex) || npubMTUD is null)
                        break;

                    SetEPMTUSize(receivedFrom, Convert.ToUInt16(packet.Length));
                    _epsInfo[receivedFrom].SetNPub(npubMTUD);
                    _epsInfo[receivedFrom].SetAmIConnecting(false);
                    SendMTUFound(receivedFrom, Convert.ToUInt16(packet.Length));

                    if (SigServer)
                    {
                        if (relativeIndex == 0)
                            relativeIndex = 1;
                        _epsInfo[receivedFrom].SetRelativeIndex(relativeIndex);
                    }
                    break;
                case PacketType.MTU_FOUND:
                    SendAcknowledge(receivedFrom, packet);
                    if (!Body.MTU_FOUND(packet, out ushort? dataLength, out NPub? npubMTUF) || !dataLength.HasValue)
                        break;
                    SetEPMTUSize(receivedFrom, dataLength.Value);
                    // TODO: se esiste già una NPub (es. connessione con un peer tramite sig-server) allora le due NPub devono coincidere
                    _epsInfo[receivedFrom].SetNPub(npubMTUF);
                    _epsInfo[receivedFrom].SetSentSecret();
                    SendHandShake(receivedFrom, _epsInfo[receivedFrom].SentSecret, null);
                    break;
                case PacketType.HANDSHAKE:
                    SendAcknowledge(receivedFrom, packet);

                    // Decrypt and excract body content
                    if (!Body.HANDSHAKE(packet, Identity.NSec, GetEPNPub(receivedFrom), out int? sentSecret, out int? receivedSecret))
                        break;

                    // 1st HND (Handshake) step: sender secret reception
                    if (sentSecret.HasValue && !receivedSecret.HasValue)
                    {
                        _epsInfo[receivedFrom].SetReceivedSecret(sentSecret.Value);
                        _epsInfo[receivedFrom].SetSentSecret();
                        SendHandShake(receivedFrom, _epsInfo[receivedFrom].SentSecret, _epsInfo[receivedFrom].ReceivedSecret);
                    }
                    // 2nd HND (Handshake) step: sender solution check and receiver secret reception
                    else if (sentSecret.HasValue && receivedSecret.HasValue)
                    {
                        if (_epsInfo[receivedFrom].SentSecret != receivedSecret)
                        {
                            _epsInfo[receivedFrom].SetTrusted(false);
                            break;
                        }
                        _epsInfo[receivedFrom].SetReceivedSecret(sentSecret.Value);
                        SendHandShake(receivedFrom, null, _epsInfo[receivedFrom].ReceivedSecret);
                        _epsInfo[receivedFrom].SetTrusted(true);
                    }
                    // 3nd HND (Handshake) step: receiver solution check
                    else if (!sentSecret.HasValue && receivedSecret.HasValue)
                    {
                        if (_epsInfo[receivedFrom].SentSecret != receivedSecret)
                        {
                            _epsInfo[receivedFrom].SetTrusted(false);
                            break;
                        }
                        SendConnectionConfirm(receivedFrom);
                        _epsInfo[receivedFrom].SetTrusted(true);
                    }
                    break;
                case PacketType.CONNECTION_CONFIRM:
                    SendAcknowledge(receivedFrom, packet);

                    // Decrypt and excract body content
                    if (!Body.CONNECTION_CONFIRM(packet, Identity.NSec, GetEPNPub(receivedFrom), out bool? isSigServer) || !isSigServer.HasValue)
                        break;

                    if (IsEPTrusted(receivedFrom) && !_epsInfo[receivedFrom].IsConnected)
                    {
                        _epsInfo[receivedFrom].SetConnected(true);
                        _epsInfo[receivedFrom].SetAmIConnecting(null);
                        _epsInfo[receivedFrom].SetIsSigServer(isSigServer.Value);
                        SendConnectionConfirm(receivedFrom);
                        NPub? npub = _epsInfo[receivedFrom].NPub;
                        if (npub is not null)
                        {
                            OnConnectionConfirmed?.Invoke(receivedFrom, npub.Bech32, isSigServer.Value, _epsInfo[receivedFrom].RelativeIndex);
                            //// A client immediately disconnect from a Signaling Server after the first notification
                            //if (!SigServer && isSigServer.Value)
                            //    DisconnectFrom(receivedFrom);
                        }
                    }
                    break;
                case PacketType.DISCONNECTION:
                    SendAcknowledge(receivedFrom, packet);

                    ThreadUtilities.PauseThread(50);

                    // Decrypt and excract body content
                    if (!Body.DISCONNECTION(packet, Identity.NSec, GetEPNPub(receivedFrom), out int? sentSecret_D, out int? receivedSecret_D))
                        break;

                    // If sent or received or both secrets are missing this is not a collaborative disconnection
                    if (!sentSecret_D.HasValue || !receivedSecret_D.HasValue)
                        break;
                    // If both secrets are present we can check the autenticity
                    else
                    {
                        // If sent or received or both secrets doesn't match this is not a collaborative disconnection
                        if (_epsInfo[receivedFrom].SentSecret != receivedSecret_D || _epsInfo[receivedFrom].ReceivedSecret != sentSecret_D)
                            break;

                        // If the other peer is asking me to disconnect
                        if (!_epsInfo[receivedFrom].AmIDisconnecting.HasValue)
                        {
                            SendDisconnection(receivedFrom);
                            _epsInfo[receivedFrom].SetAmIDisconnecting(false);
                        }
                        else
                        {
                            if (_epsInfo[receivedFrom].AmIDisconnecting.Value)
                                SendDisconnection(receivedFrom);

                            RemovePeerConnection(receivedFrom);
                        }
                    }
                    break;
                case PacketType.SIGNALING_PROPAGATION:
                    SendAcknowledge(receivedFrom, packet);

                    // Decrypt and excract body content
                    if (!Body.SIGNALING_PROPAGATION(packet, Identity.NSec, GetEPNPub(receivedFrom), out NPub? sigPeerNPub, out EndPoint? sigPeerEP, out byte sigRelativeIndex)
                        || sigPeerNPub is null || sigPeerEP is null || sigRelativeIndex == 0)
                        break;

                    OnSignalingPropagation?.Invoke(sigPeerEP, sigPeerNPub.Bech32, sigRelativeIndex);
                    break;
            };
        }
        private bool ManageChunks(EndPoint ep, Header header, byte[] packet, out byte[] fullData)
        {
            fullData = new byte[0];

            // If it's the first packet
            if (header.ChunkNumber == 1)
            {
                if (header.IV is null)
                    return false;

                // We extract the expected chunks
                byte[] body = Body.ExtractFromPacket(packet);
                uint totalChunks = BitConverter.ToUInt32(body);
                // We add the UniqueIdentifier to the chunks list
                _epsInfo[ep]._chunks.AddOrUpdate(
                    header.PacketIdentifier.Value,
                    addValue: new(),
                    updateValueFactory: (uid, data) => data
                );

                // If we have receive a data chunk before this initial packet, we have to replace the "fake" placeholder key with the real ones with IV bytes
                if (_epsInfo[ep]._chunks[header.PacketIdentifier.Value].ContainsKey(header.PacketIdentifier.Value.ToString()))
                {
                    // We add the IV bytes key, also cloning all of the content
                    _epsInfo[ep]._chunks[header.PacketIdentifier.Value].AddOrUpdate(
                        header.IV.ToHexString(),
                        addValue: _epsInfo[ep]._chunks[header.PacketIdentifier.Value][header.PacketIdentifier.Value.ToString()],
                        updateValueFactory: (iv, data) => _epsInfo[ep]._chunks[header.PacketIdentifier.Value][header.PacketIdentifier.Value.ToString()]
                    );
                    _epsInfo[ep]._chunks[header.PacketIdentifier.Value][header.IV.ToUTF8String()].TotalChunks = totalChunks;
                    // And then we remove the "fake" placeholder
                    _epsInfo[ep]._chunks[header.PacketIdentifier.Value].Remove(header.PacketIdentifier.Value.ToString(), out ChunksInfo? datas);
                    if (datas is not null)
                        datas.Dispose();
                }
                // If this initial packet is the first received, we simply add the key
                else
                    _epsInfo[ep]._chunks[header.PacketIdentifier.Value].AddOrUpdate(
                        header.IV.ToHexString(),
                        addValue: new(totalChunks),
                        updateValueFactory: (iv, data) => data
                    );
            }
            // If it's a data chunk
            else
            {
                // We try to add the UniqueIdentifier to the chunks list in case this chunk has been received before the initial packet
                _epsInfo[ep]._chunks.AddOrUpdate(
                    header.PacketIdentifier.Value,
                    addValue: new(),
                    updateValueFactory: (uid, data) => data
                );

                // If in fact we don't have yet received the initial packet we initialize with a "fake" key with PacketIdentifier as a placeholder
                if (_epsInfo[ep]._chunks[header.PacketIdentifier.Value].Count == 0)
                    // Then we also add the IV as the key of the dictionary holding all chunks
                    _epsInfo[ep]._chunks[header.PacketIdentifier.Value].AddOrUpdate(
                        header.PacketIdentifier.Value.ToString(),
                        addValue: new(0),
                        updateValueFactory: (iv, data) => data
                    );

                // In every case we stash the chunk
                _epsInfo[ep]._chunks[header.PacketIdentifier.Value].ElementAt(0).Value.Chunks.Add(new ChunkData(header, packet));
            }


            // If we received all packets
            // NOTE: this must be checked for every packets (also the initial packet) because on UDP order is not guaranteed
            KeyValuePair<string, ChunksInfo> item = _epsInfo[ep]._chunks[header.PacketIdentifier.Value].ElementAt(0);
            if (Convert.ToUInt32(item.Value.Chunks.Count) == item.Value.TotalChunks)
            {
                ChunkData? min = item.Value.Chunks.MinBy(x => x.Chunk.Length);
                if (min is null)
                    return false;
                ChunkData? max = item.Value.Chunks.MaxBy(x => x.Chunk.Length);
                if (max is null)
                    return false;
                int shortestChunk = min.Chunk.Length;
                int largestChunk = max.Chunk.Length;
                long encryptedBodyLength = ((item.Value.TotalChunks - 1) * largestChunk) + shortestChunk;
                byte[] encryptedBody = new byte[encryptedBodyLength];
                long offset = 0;
                foreach (ChunkData? chk in item.Value.Chunks.OrderBy(x => x.ChunkNumber))
                {
                    Array.Copy(chk.Chunk, 0, encryptedBody, offset, chk.Chunk.Length);
                    offset += chk.Chunk.Length;
                }

                byte[] IV = item.Key.HexToByteArray();

                item.Value.Dispose();
                _epsInfo[ep]._chunks[header.PacketIdentifier.Value].Remove(item.Key, out _);
                _epsInfo[ep]._chunks[header.PacketIdentifier.Value].Clear();
                _epsInfo[ep]._chunks.Remove(header.PacketIdentifier.Value, out _);

                fullData = Identity.NSec.Decrypt(encryptedBody, IV, GetEPNPub(ep));
                return true;
            }

            return false;
        }
        private void Socket_OnMTUSizeExceed(EndPoint ep, ushort dataSize)
        {
            _epsInfo.AddOrUpdate(
                ep,
                addValue: new(ep),
                updateValueFactory: (endpoint, value) => value.ReduceMTUSize(dataSize)
            );
            if (dataSize < 65)
                return;
            SendMTUDiscovery(ep, Convert.ToInt32(_epsInfo[ep].MTUSize));
        }
        private void Socket_OnRemoteConnectionReset(EndPoint ep)
        {
            // TODO: found a use case for this event
        }
        private void Socket_OnRateUpdated(Rates totalRates, Dictionary<EndPoint, Rates> epsRates, int sendBufferFillPercentage)
        {
            List<EPInfo> epInfos = new();
            foreach (KeyValuePair<EndPoint, Rates> ep in epsRates)
            {
                if (!_epsInfo.ContainsKey(ep.Key))
                    continue;
                _epsInfo[ep.Key].SetRates(ep.Value);
                if (_epsInfo[ep.Key].IsConnected)
                    epInfos.Add(new EPInfo(_epsInfo[ep.Key]));
            }

            OnRateUpdated?.Invoke(totalRates, epInfos, sendBufferFillPercentage);
        }
        private void Socket_OnException(Exception ex) => OnException?.Invoke(ex);


        private void TryUdpHolePunch(EndPoint sigServerEP, EndPoint ep1, EndPoint? ep2, EndPoint? ep3)
        {
            if (NATUtilities.GuessUserNATPortAlgorithm(ep1, ep2, ep3, out int nextPort, out int skipRandomMaxInterval) == NATPortAlgorithm.Uncatchable)
            {
                _coordinatingServers[sigServerEP].RemoveAll(x => x.EqualTo(ep1));
                OnConnectionFailed?.Invoke(ep1);
                return;
            }

            // We start from the same ip but with the new port number
            string[] ipParts = ep1.ToIPV4String().Split(':');
            ipParts[1] = nextPort.ToString();
            EndPoint? remoteEP = string.Join(':', ipParts).ToEndPoint();
            if (remoteEP is null)
            {
                _coordinatingServers[sigServerEP].RemoveAll(x => x.EqualTo(ep1));
                OnConnectionFailed?.Invoke(ep1);
                return;
            }

            Task.Factory.StartNew(() =>
            {
                Stopwatch limit = Stopwatch.StartNew();
                while (limit.Elapsed.TotalSeconds < 5 && !_epsInfo.ContainsKey(remoteEP))
                {
                    // Send multiple packet in case they get dropped
                    SendConnectionPossible(remoteEP);
                    ThreadUtilities.PauseThread(50);
                }
                SendConnectionPossible(remoteEP);
                limit.Stop();

                if (!_epsInfo.ContainsKey(remoteEP) && !TryPunchSymmetricNAT(remoteEP, skipRandomMaxInterval))
                    OnConnectionFailed?.Invoke(remoteEP);

                // We remove the Signaling Server key here, because we either succeded or not but we can be open to new requests
                _coordinatingServers.TryRemove(sigServerEP, out _);
            }, TaskCreationOptions.LongRunning);
        }
        private bool TryPunchSymmetricNAT(EndPoint punchEP, int symmetricNATPortRangeToPunch)
        {
            // First we define the port range we want to check
            int originalPort = ((IPEndPoint)punchEP).Port,
                diff = originalPort - symmetricNATPortRangeToPunch,
                sum = originalPort + symmetricNATPortRangeToPunch,
                minPort = diff > 0 ? diff : 1,
                maxPort = Math.Min(diff > 0 ? sum + 1 : sum + Math.Abs(diff) + 1, Convert.ToInt32(ushort.MaxValue));

            List<EndPoint> eps = new();
            IPAddress ip = ((IPEndPoint)punchEP).Address;


            List<int> portsRange = Enumerable.Range(minPort, maxPort - minPort).ToList();
            portsRange.RemoveAll(x => x == originalPort);
            foreach (int port in portsRange)
            {
                if (_epsInfo.ContainsKey(punchEP) || eps.Any(x => _epsInfo.ContainsKey(x)))
                    return true;

                EndPoint? ep = new IPEndPoint(ip, port).ToIPV4String().ToEndPoint();
                if (ep is null)
                    continue;
                eps.Add(ep);

                // Send multiple packet in case they get dropped
                for (int i = 0; i < 10; i++)
                    SendConnectionPossible(ep);
            }

            bool succeded = false;
            Stopwatch limit = Stopwatch.StartNew();
            while (limit.Elapsed.TotalSeconds < 5)
            {
                if (eps.Any(x => _epsInfo.ContainsKey(x)))
                {
                    succeded = true;
                    break;
                }
                ThreadUtilities.PauseThread(100);
            }
            limit.Stop();
            eps.Clear();
            return succeded;
        }


        private void TryStartBackgroundTasks()
        {
            Task.Factory.StartNew(() =>
            {
                Stopwatch _backgroundTaskTimer = Stopwatch.StartNew();
                while (socket is not null && socket.Status == SocketStatus.Running)
                    try
                    {
                        long now = DateTime.Now.Ticks;
                        foreach (KeyValuePair<EndPoint, EPInfo> ep in _epsInfo)
                            // If this endpoint is not properly responding from a lot of time
                            if (ep.Value.NotResponding && ep.Value._rttaBuffer.Any(x => now - x.Value > ep.Value.NotRespondingAutoDisconnectionTime))
                            {
                                // If no collaborative disconnectin was initiated we try to start one
                                if (!ep.Value.AmIDisconnecting.HasValue)
                                    DisconnectFrom(ep.Key);
                                // If a collaborative disconnection haas been already started we simply remove the connection
                                else
                                    RemovePeerConnection(ep.Key);
                            }
                            // Else we keep sending RTTAs
                            else
                                SendRTTA(ep.Key);

                        // I remove the recently disconnected peers after a certain amount of time they got disconnected
                        foreach (KeyValuePair<EndPoint, long> ep in _recentlyDisconnectedEndpoints)
                            if (now - ep.Value >= 50000)
                                _recentlyDisconnectedEndpoints.TryRemove(ep);


                        while (_backgroundTaskTimer.Elapsed.TotalMilliseconds < 1000 && socket is not null && socket.Status == SocketStatus.Running)
                            ThreadUtilities.PauseThread(100);
                        _backgroundTaskTimer.Restart();
                    }
                    catch (Exception ex)
                    {
                        OnException?.Invoke(ex);
                    }
                _backgroundTaskTimer.Stop();
            }, TaskCreationOptions.LongRunning);



            Task.Factory.StartNew(() =>
            {
                Stopwatch _backgroundTaskTimer = Stopwatch.StartNew();
                while (socket is not null && socket.Status == SocketStatus.Running)
                    try
                    {
                        long now = DateTime.Now.Ticks;
                        foreach (KeyValuePair<EndPoint, EPInfo> ep in _epsInfo)
                        {
                            if (ep.Value.AmIDisconnecting.HasValue)
                                continue;

                            foreach (KeyValuePair<string, UnackData> unack in ep.Value._unack)
                            {
                                // If too much time has passed since the caching of this packet, we simply drop it as it's not deliverable
                                if (now - unack.Value.Timestamp > ep.Value.UnackDropTime)
                                    _epsInfo[ep.Key]._unack.TryRemove(unack.Key, out _);
                                // But if enough time has passed to make this packet sendable again
                                else if (now - unack.Value.Timestamp > ep.Value.UnackRetryTime && socket is not null)
                                    socket.Send(ep.Key, unack.Value.Packet);
                            }
                        }


                        while (_backgroundTaskTimer.Elapsed.TotalMilliseconds < 1000 && socket is not null && socket.Status == SocketStatus.Running)
                            ThreadUtilities.PauseThread(100);
                        _backgroundTaskTimer.Restart();
                    }
                    catch (Exception ex)
                    {
                        OnException?.Invoke(ex);
                    }
                _backgroundTaskTimer.Stop();
            }, TaskCreationOptions.LongRunning);
        }


        private void BindEvents()
        {
            if (socket is null)
                return;
            socket.OnException += Socket_OnException;
            socket.OnMTUSizeExceed += Socket_OnMTUSizeExceed;
            socket.OnRemoteConnectionReset += Socket_OnRemoteConnectionReset;
            socket.OnReceive += Socket_OnReceive;
            socket.OnRateUpdated += Socket_OnRateUpdated;
        }
        private void UnbindEvents()
        {
            if (socket is null)
                return;
            socket.OnException -= Socket_OnException;
            socket.OnMTUSizeExceed -= Socket_OnMTUSizeExceed;
            socket.OnRemoteConnectionReset -= Socket_OnRemoteConnectionReset;
            socket.OnReceive -= Socket_OnReceive;
            socket.OnRateUpdated -= Socket_OnRateUpdated;
        }
        public void Dispose()
        {
            UnbindEvents();
            if (socket is not null)
                socket.Dispose();
            socket = null;

            foreach (KeyValuePair<EndPoint, EPInfo> ep in _epsInfo)
                ep.Value.Dispose();
            _epsInfo.Clear();
        }
    }
}
