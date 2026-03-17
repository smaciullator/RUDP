using RUDP.Enums;
using RUDP.Extensions;
using RUDP.Keys;
using RUDP.Models;
using RUDP.Utilities;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;
using static RUDP.MultiThreadUDPSocket;

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
        public event EventHandler<EndPoint, Header, byte[], long> OnPacketReceived;
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
        /// Triggered when a file sending has succesfully ended
        /// </summary>
        public event EventHandler<EndPoint, string> OnFileSent;
        /// <summary>
        /// Triggered when a file reception has succesfully ended
        /// </summary>
        public event EventHandler<EndPoint, string, string> OnFileReceived;
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
        private ConcurrentDictionary<uint, uint> _receivingChunks { get; set; } = new();
        private ConcurrentDictionary<uint, string> _sendingFiles { get; set; } = new();
        private ConcurrentDictionary<uint, FileData> _receivingFiles { get; set; } = new();
        private const string _encryptedFileTag = "_ENCRYPTED";


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
                if (string.IsNullOrEmpty(bech32NSec))
                    Identity = KeyPair.GenerateNew();
                else
                {
                    NSec nsec = NSec.FromBech32(bech32NSec);
                    Identity = string.IsNullOrEmpty(nsec.Bech32) ? KeyPair.GenerateNew() : KeyPair.From(nsec);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Set the global upload speed limit for this socket instance, calculated with the sum of all bytes sent per second to any endpoint
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
        public void TryConnectWith(EndPoint ep, byte relativeIndex = 0, int secondsTimeout = 15)
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
                while (limit.Elapsed.TotalSeconds < secondsTimeout && !_epsInfo[ep].IsConnected)
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
        public void TryConnectLocallyWith(EndPoint ep)
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
            return Send(sendTo, PacketUtilities.RTTA(Identity, num));
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
                return Send(sendTo, PacketUtilities.RTTB(Identity, num.Value));
            return false;
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
            return Send(sendTo, PacketUtilities.ACKNOWLEDGEMENT(Identity, header.PacketIdentifier ?? 0, header.ChunkNumber));
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
            return Send(sendTo, PacketUtilities.MTU_DISCOVERY(Identity, _epsInfo[sendTo].GetNextSendNumeration(), dataSize, relativeIndex), true);
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
            return Send(sendTo, PacketUtilities.MTU_FOUND(Identity, _epsInfo[sendTo].GetNextSendNumeration(), dataLenght, SigServer), true);
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
            return Send(sendTo, PacketUtilities.DISCONNECTION(Identity, _epsInfo[sendTo].GetNextSendNumeration()), true);
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
                _connectingToNPubs.TryAdd(bech32PeerNPub, 0);
                return Send(sendTo, PacketUtilities.P2P_COORDINATION_REQUEST(Identity, _epsInfo[sendTo].GetNextSendNumeration(), NPub.FromBech32(bech32PeerNPub)), true);
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
                return Send(sendTo, PacketUtilities.UNKNOWN_IDENTITY(Identity, _epsInfo[sendTo].GetNextSendNumeration(), NPub.FromBech32(bech32PeerNPub)), true);
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
                return Send(sendTo, PacketUtilities.P2P_CONNECTION_COORDINATION(Identity, _epsInfo[sendTo].GetNextSendNumeration(), NPub.FromBech32(bech32PeerNPub), peerEP1, peerEP2, peerEP3), true);
            }
            catch
            {
                return false;
            }
        }
        public bool SendConnectionPossible(EndPoint sendTo)
        {
            try
            {
                return Send(sendTo, PacketUtilities.CONNECTION_POSSIBLE(Identity), true);
            }
            catch
            {
                return false;
            }
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
            return Send(sendTo, PacketUtilities.SIGNALING_PROPAGATION(Identity, _epsInfo[sendTo].GetNextSendNumeration(), GetEPNPub(peerEP), peerEP, relativeIndex, GetEPNPub(sendTo)), true);
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
            return Send(sendTo, PacketUtilities.DATA(Identity, _epsInfo[sendTo].GetNextSendNumeration(), 0, rawData, GetEPNPub(sendTo)), true);
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
            return Send(sendTo, PacketUtilities.STREAM(Identity, rawData, GetEPNPub(sendTo)));
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

            byte[] body = Path.GetFileName(fileFullPath).UTF8AsByteArray();

            uint uniqueIdentifier = _epsInfo[sendTo].GetNextSendNumeration();
            _sendingFiles.AddOrUpdate(
                uniqueIdentifier,
                // NOTA: qui inserisco la FULL PATH invece del solo file name per poter successivamente inviare il file
                addValue: fileFullPath,
                updateValueFactory: (identifier, value) => value
            );
            return Send(sendTo, PacketUtilities.FILE_PRESENTATION(Identity, uniqueIdentifier, 0, body, GetEPNPub(sendTo)), true);
        }
        private bool SendFileContent(EndPoint sendTo, uint packetIdentifier, uint chunkNumber, byte[] chunk)
        {
            if (!_epsInfo.ContainsKey(sendTo))
                return false;

            return Send(sendTo, PacketUtilities.FILE_CONTENT(Identity, packetIdentifier, chunkNumber, chunk, GetEPNPub(sendTo)), true);
        }
        #endregion


        private bool Send(EndPoint sendTo, (Header header, byte[]? data) pkt, bool requireAck = false)
        {
            if (socket is null || _recentlyDisconnectedEndpoints.ContainsKey(sendTo))
                return false;

            // If the current MTU Size is too small we can't procede
            int maxPktSize = GetEPMTUSize(sendTo);
            if (maxPktSize < Header._minSize)
                return false;

            if (pkt.header.Type == PacketType.FILE_CONTENT)
            {
                byte[] data = PacketUtilities.CreatePacket(pkt.header, pkt.data);
                bool sent = socket.Send(sendTo, data);
                if (requireAck)
                    _epsInfo.AddOrUpdate(sendTo, addValue: new(sendTo), updateValueFactory: (endpoint, value) => value.AddUnackPacket(pkt.header, data));
                return sent;
            }
            else
            {
                List<byte[]>? chunks = GetChunks(sendTo, pkt, maxPktSize);
                if (chunks is null)
                    return false;
                foreach (byte[] chunk in chunks)
                {
                    socket.Send(sendTo, chunk);
                    if (requireAck)
                        _epsInfo.AddOrUpdate(sendTo, addValue: new(sendTo), updateValueFactory: (endpoint, value) => value.AddUnackPacket(pkt.header, chunk));
                }
                chunks.Clear();
                return true;
            }
        }
        private List<byte[]>? GetChunks(EndPoint sendTo, (Header header, byte[]? data) pkt, int maxPktSize)
        {
            List<byte[]> chunks = new();
            int dataLength = pkt.data is null ? 0 : pkt.data.Length;

            // If this packet doesn't exceed the current MTU Size for the endpoint
            if (pkt.header.Length + dataLength <= maxPktSize)
                chunks.Add(PacketUtilities.CreatePacket(pkt.header, pkt.data));
            // If the packet must be divided in chunks
            else
            {
                // If we are exceeding the max packet size only with header wen cannot send anything
                if (dataLength == 0)
                    return null;

                // Only DATA and FILE_PRESENTATION packets can be chunked
                if (pkt.header.Type != PacketType.DATA && pkt.header.Type != PacketType.FILE_PRESENTATION)
                    return null;

                NPub? epNPub = GetEPNPub(sendTo);
                if (epNPub is null)
                    return null;

                int maxDataSize = maxPktSize - pkt.header.Length;
                uint chunksNumber = Convert.ToUInt32(dataLength / maxDataSize);
                if (dataLength % maxDataSize > 0)
                    chunksNumber += 1;

                int offset = 0,
                    chunkSize;
                byte[] chunkData;

                Send(sendTo, PacketUtilities.CHUNKS_PRESENTATION(Identity, pkt.header.PacketIdentifier.Value, chunksNumber, epNPub), true);

                // All chunks start from number 0
                for (uint i = 0; i < chunksNumber; i++)
                {
                    chunkSize = Math.Min(dataLength - offset, maxDataSize);
                    chunkData = new byte[chunkSize];
                    for (int j = 0; j < chunkData.Length && (offset + j) < dataLength; j++)
                        chunkData[j] = pkt.data[offset + j];

                    (Header header, byte[]? data) chunkedPacket = new(new(), null);
                    if (pkt.header.Type == PacketType.DATA)
                        chunkedPacket = PacketUtilities.DATA(Identity, pkt.header.PacketIdentifier.Value, i, chunkData, epNPub);
                    if (pkt.header.Type == PacketType.FILE_PRESENTATION)
                        chunkedPacket = PacketUtilities.FILE_PRESENTATION(Identity, pkt.header.PacketIdentifier.Value, i, chunkData, epNPub);

                    chunks.Add(PacketUtilities.CreatePacket(chunkedPacket.header, chunkedPacket.data));
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
        private void SetEPTrusted(EndPoint ep, bool trusted)
        {
            if (_epsInfo.ContainsKey(ep))
                _epsInfo[ep].SetTrusted(trusted);
        }


        private void Socket_OnReceive(EndPoint receivedFrom, byte[] packet, long timestamp)
        {
            if (packet.Length == 0 || _recentlyDisconnectedEndpoints.ContainsKey(receivedFrom))
                return;

            Header header = Header.Deserialize(packet);
            Span<byte> rawBody = default;
            if (packet.Length > header.Length)
                rawBody = new Span<byte>(packet).Slice(header.Length);
            string bech32 = "";
            if (_epsInfo.ContainsKey(receivedFrom))
                bech32 = _epsInfo[receivedFrom].NPubBech32 ?? "";
            NPub? senderNPub = GetEPNPub(receivedFrom);

            OnPacketReceived?.Invoke(receivedFrom, header, rawBody.ToArray(), timestamp);

            switch (header.Type)
            {
                case PacketType.CHUNKS_PRESENTATION:
                    SendAcknowledge(receivedFrom, packet);

                    if (SigServer || !header.PacketIdentifier.HasValue || !header.ChunkNumber.HasValue)
                        break;

                    if (senderNPub is null || !PacketUtilities.IsSignatureValid(Identity, senderNPub, header, rawBody))
                    {
                        SetEPTrusted(receivedFrom, false);
                        break;
                    }

                    _receivingChunks.TryAdd(header.PacketIdentifier.Value, header.ChunkNumber.Value);
                    break;
                case PacketType.DATA:
                    SendAcknowledge(receivedFrom, packet);

                    if (SigServer || !header.PacketIdentifier.HasValue || !header.ChunkNumber.HasValue)
                        break;

                    if (senderNPub is null || !PacketUtilities.IsSignatureValid(Identity, senderNPub, header, rawBody))
                    {
                        SetEPTrusted(receivedFrom, false);
                        break;
                    }

                    if (!ManageDataChunks(receivedFrom, header, Body.ExtractFromPacket(packet), out byte[] fullData))
                        break;

                    OnData?.Invoke(receivedFrom, bech32, PacketUtilities.DecryptMessage(fullData, Identity.NSec, senderNPub), timestamp);
                    break;
                case PacketType.FILE_PRESENTATION:
                    SendAcknowledge(receivedFrom, packet);

                    if (SigServer || !header.PacketIdentifier.HasValue || !header.ChunkNumber.HasValue)
                        break;

                    if (senderNPub is null || !PacketUtilities.IsSignatureValid(Identity, senderNPub, header, rawBody))
                    {
                        SetEPTrusted(receivedFrom, false);
                        break;
                    }

                    if (!ManageFilePresentationChunks(receivedFrom, header, packet, out byte[] fullFilePresentation))
                        break;

                    break;
                case PacketType.FILE_CONTENT:
                    SendAcknowledge(receivedFrom, packet);

                    if (SigServer || !header.PacketIdentifier.HasValue || !header.ChunkNumber.HasValue)
                        break;

                    if (senderNPub is null || !PacketUtilities.IsSignatureValid(Identity, senderNPub, header, rawBody))
                    {
                        SetEPTrusted(receivedFrom, false);
                        break;
                    }
                    ManageFileContentChunks(receivedFrom, header.PacketIdentifier.Value, header.ChunkNumber.Value, rawBody.ToArray());
                    break;
                case PacketType.STREAM:
                    if (SigServer)
                        break;

                    if (senderNPub is null || !PacketUtilities.IsSignatureValid(Identity, senderNPub, header, rawBody))
                    {
                        SetEPTrusted(receivedFrom, false);
                        break;
                    }

                    byte[] decryptedStream = PacketUtilities.DecryptMessage(Body.ExtractFromPacket(packet), Identity.NSec, senderNPub);

                    OnStream?.Invoke(receivedFrom, bech32, decryptedStream, timestamp);
                    break;
                case PacketType.ACKNOWLEDGEMENT:
                    if (senderNPub is null || !PacketUtilities.IsSignatureValid(Identity, senderNPub, header, rawBody))
                    {
                        SetEPTrusted(receivedFrom, false);
                        break;
                    }

                    bool removed = _epsInfo[receivedFrom]._unack.TryRemove(new UnackData(header, packet).UID, out UnackData? data);
                    if (data is not null)
                    {
                        if (data.Timestamp.HasValue)
                            socket?.SetEPCongestionWindow(receivedFrom, _epsInfo[receivedFrom].CalculateCongestionWindow((DateTime.Now.Ticks - Convert.ToDouble(data.Timestamp)) / 10000));

                        // FILE_PRESENTATION has been confirmed, so i start sending the file data
                        if (data.PacketType is not null && data.PacketIdentifier.HasValue)
                        {
                            if (data.PacketType == PacketType.FILE_PRESENTATION)
                                ManageFilePresentationAcknowledgment(receivedFrom, data.PacketIdentifier.Value);
                            else if (data.PacketType == PacketType.FILE_CONTENT)
                            {
                                _sendingFiles.TryRemove(data.PacketIdentifier.Value, out string fileName);
                                OnFileSent?.Invoke(receivedFrom, fileName);
                            }
                        }

                        data.Dispose();
                    }
                    break;
                case PacketType.RTTA:
                    if (senderNPub is null || !PacketUtilities.IsSignatureValid(Identity, senderNPub, header, rawBody))
                    {
                        SetEPTrusted(receivedFrom, false);
                        break;
                    }

                    SendRTTB(receivedFrom, packet);
                    break;
                case PacketType.RTTB:
                    if (senderNPub is null || !PacketUtilities.IsSignatureValid(Identity, senderNPub, header, rawBody))
                    {
                        SetEPTrusted(receivedFrom, false);
                        break;
                    }

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

                    if (senderNPub is null || !PacketUtilities.IsSignatureValid(Identity, senderNPub, header, rawBody))
                    {
                        SetEPTrusted(receivedFrom, false);
                        break;
                    }

                    if (!Body.P2P_COORDINATION_REQUEST(rawBody, out NPub? targetNPub_1) || targetNPub_1 is null)
                        break;

                    OnP2PCoordinationRequest?.Invoke(receivedFrom, senderNPub.Bech32, targetNPub_1.Bech32);
                    break;
                case PacketType.UNKNOWN_IDENTITY:
                    SendAcknowledge(receivedFrom, packet);

                    if (senderNPub is null || !PacketUtilities.IsSignatureValid(Identity, senderNPub, header, rawBody))
                    {
                        SetEPTrusted(receivedFrom, false);
                        break;
                    }

                    if (
                        SigServer
                        || !Body.UNKNOWN_IDENTITY(rawBody, out NPub? targetNPub_2)
                        || targetNPub_2 is null
                    )
                        break;

                    OnUnknownIdentity?.Invoke(receivedFrom, targetNPub_2.Bech32);
                    break;
                case PacketType.P2P_CONNECTION_COORDINATION:
                    SendAcknowledge(receivedFrom, packet);

                    if (senderNPub is null || !PacketUtilities.IsSignatureValid(Identity, senderNPub, header, rawBody))
                    {
                        SetEPTrusted(receivedFrom, false);
                        break;
                    }

                    if (
                        SigServer
                        || !Body.P2P_CONNECTION_COORDINATION(rawBody, out NPub? p2pNPub, out EndPoint? ep1, out EndPoint? ep2, out EndPoint? ep3)
                        || p2pNPub == null
                        || ep1 is null
                    )
                        break;

                    if (!_coordinatingServers.ContainsKey(receivedFrom))
                        _coordinatingServers.TryAdd(receivedFrom, new());
                    // If we already received a not yet finished coordination from this Signaling Server for this specific endpoint
                    if (_coordinatingServers[receivedFrom].Contains(ep1))
                        break;
                    // If this is the first time we receive this coordination from this Signaling Server
                    _coordinatingServers[receivedFrom].Add(ep1);

                    TryUdpHolePunch(receivedFrom, ep1, ep2, ep3);
                    break;
                case PacketType.CONNECTION_POSSIBLE:
                    if (packet.Length < Header._minSize)
                        break;

                    if (SigServer)
                        break;

                    if (!Body.CONNECTION_POSSIBLE(rawBody, out NPub? npubCNPO) || npubCNPO is null)
                        break;

                    if (!PacketUtilities.IsSignatureValid(Identity, npubCNPO, header, rawBody))
                        break;

                    // If it's me who started this connection attempt
                    if (_connectingToNPubs.ContainsKey(npubCNPO.Bech32))
                        OnP2PConnectionPossible?.Invoke(receivedFrom, npubCNPO.Bech32);
                    else
                        OnP2PConnectionRequest?.Invoke(receivedFrom, npubCNPO.Bech32);
                    break;
                case PacketType.MTU_DISCOVERY:
                    SendAcknowledge(receivedFrom, packet);

                    if (packet.Length < Header._minSize)
                        break;

                    if (!Body.MTU_DISCOVERY(rawBody, out NPub? npubMTUD, out byte relativeIndexMTU) || npubMTUD is null)
                        break;

                    if (!PacketUtilities.IsSignatureValid(Identity, npubMTUD, header, rawBody))
                    {
                        SetEPTrusted(receivedFrom, false);
                        break;
                    }

                    SetEPMTUSize(receivedFrom, Convert.ToUInt16(packet.Length));
                    _epsInfo[receivedFrom].SetNPub(npubMTUD);
                    _epsInfo[receivedFrom].SetAmIConnecting(false);
                    if (SigServer)
                        _epsInfo[receivedFrom].SetRelativeIndex(relativeIndexMTU == 0 ? (byte)1 : relativeIndexMTU);

                    SendMTUFound(receivedFrom, Convert.ToUInt16(packet.Length));

                    if (_connectingToNPubs.ContainsKey(npubMTUD.Bech32))
                        _connectingToNPubs.TryRemove(npubMTUD.Bech32, out _);
                    break;
                case PacketType.MTU_FOUND:
                    SendAcknowledge(receivedFrom, packet);

                    if (!Body.MTU_FOUND(rawBody, out NPub? npubMTUF, out ushort? dataLength, out bool? isSigServer) || !dataLength.HasValue || !isSigServer.HasValue)
                        break;

                    if (
                        (senderNPub is null && !PacketUtilities.IsSignatureValid(Identity, npubMTUF, header, rawBody))
                        || (senderNPub is not null && !PacketUtilities.IsSignatureValid(Identity, senderNPub, header, rawBody))
                    )
                    {
                        SetEPTrusted(receivedFrom, false);
                        break;
                    }

                    SetEPMTUSize(receivedFrom, dataLength.Value);
                    SetEPTrusted(receivedFrom, true);

                    if (senderNPub is null)
                    {
                        _epsInfo[receivedFrom].SetNPub(npubMTUF);
                        senderNPub = GetEPNPub(receivedFrom);
                        SendMTUFound(receivedFrom, Convert.ToUInt16(packet.Length));
                    }

                    _epsInfo[receivedFrom].SetConnected(true);
                    _epsInfo[receivedFrom].SetAmIConnecting(null);
                    _epsInfo[receivedFrom].SetIsSigServer(isSigServer.Value);

                    OnConnectionConfirmed?.Invoke(receivedFrom, senderNPub.Bech32, isSigServer.Value, _epsInfo[receivedFrom].RelativeIndex);
                    break;
                case PacketType.DISCONNECTION:
                    SendAcknowledge(receivedFrom, packet);

                    if (senderNPub is null || !PacketUtilities.IsSignatureValid(Identity, senderNPub, header, rawBody))
                    {
                        SetEPTrusted(receivedFrom, false);
                        break;
                    }

                    if (!_epsInfo[receivedFrom].AmIDisconnecting.HasValue)
                    {
                        SendDisconnection(receivedFrom);
                        RemovePeerConnection(receivedFrom);
                    }
                    else if (_epsInfo[receivedFrom].AmIDisconnecting.Value)
                        RemovePeerConnection(receivedFrom);
                    break;
                case PacketType.SIGNALING_PROPAGATION:
                    SendAcknowledge(receivedFrom, packet);

                    if (senderNPub is null || !PacketUtilities.IsSignatureValid(Identity, senderNPub, header, rawBody))
                    {
                        SetEPTrusted(receivedFrom, false);
                        break;
                    }

                    byte[] decryptedSigProp = PacketUtilities.DecryptMessage(Body.ExtractFromPacket(packet), Identity.NSec, senderNPub);
                    if (!Body.SIGNALING_PROPAGATION(decryptedSigProp, out NPub? peerNPubSigProp, out EndPoint? peerEPSigProp, out byte relativeIndexSigProp)
                        || peerNPubSigProp is null || peerEPSigProp is null || relativeIndexSigProp == 0)
                        break;

                    OnSignalingPropagation?.Invoke(peerEPSigProp, peerNPubSigProp.Bech32, relativeIndexSigProp);
                    break;
            };
        }
        private bool ManageDataChunks(EndPoint ep, Header header, byte[] body, out byte[] fullData)
        {
            fullData = new byte[0];

            if (header.ChunkNumber.HasValue && _receivingChunks.ContainsKey(header.PacketIdentifier.Value))
            {
                _epsInfo[ep]._chunks.AddOrUpdate(
                    header.PacketIdentifier.Value,
                    addValue: new(),
                    updateValueFactory: (packetIdentifier, data) => data
                );
                _epsInfo[ep]._chunks[header.PacketIdentifier.Value].AddOrUpdate(
                    header.ChunkNumber.Value,
                    addValue: body,
                    updateValueFactory: (chunkNumber, data) => data
                );

                // If we received all packets
                if (_receivingChunks.ContainsKey(header.PacketIdentifier.Value) && _receivingChunks[header.PacketIdentifier.Value] == _epsInfo[ep]._chunks[header.PacketIdentifier.Value].Count)
                {
                    int offset = 0;
                    foreach (KeyValuePair<uint, byte[]> chunk in _epsInfo[ep]._chunks[header.PacketIdentifier.Value].OrderBy(x => x.Key))
                    {
                        Array.Copy(chunk.Value, 0, fullData, offset, chunk.Value.Length);
                        offset += chunk.Value.Length;
                    }
                    _receivingChunks.TryRemove(header.PacketIdentifier.Value, out _);
                    return true;
                }
            }
            else
            {
                fullData = body;
                return true;
            }

            return false;
        }
        private void ManageFilePresentationAcknowledgment(EndPoint receivedFrom, uint packetIdentifier)
        {
            if (!_sendingFiles.ContainsKey(packetIdentifier))
                return;
            Task.Run(() =>
            {
                using FileStream file = File.Open(_sendingFiles[packetIdentifier], FileMode.Open, FileAccess.Read, FileShare.Read);
                int readBytes = 0;
                uint chunkNumber = 0;
                file.Position = 0;

                int chunkContentSize = GetEPMTUSize(receivedFrom) - Header._minFileContentSize;
                if (chunkContentSize <= 0)
                {
                    file.Dispose();
                    return;
                }

                byte[] buffer = new byte[chunkContentSize];
                while ((readBytes = file.Read(buffer, 0, buffer.Length)) > 0)
                {
                    SendFileContent(receivedFrom, _epsInfo[receivedFrom].GetNextSendNumeration(), chunkNumber++, new Span<byte>(buffer).Slice(0, readBytes).ToArray());
                    file.Position += readBytes;
                }

                file.Dispose();
            });
        }
        private bool ManageFilePresentationChunks(EndPoint ep, Header header, byte[] body, out byte[] decryptedFullData)
        {
            decryptedFullData = new byte[0];

            if (header.ChunkNumber.HasValue)
            {
                _epsInfo[ep]._chunks.AddOrUpdate(
                    header.PacketIdentifier.Value,
                    addValue: new(),
                    updateValueFactory: (packetIdentifier, data) => data
                );
                _epsInfo[ep]._chunks[header.PacketIdentifier.Value].AddOrUpdate(
                    header.ChunkNumber.Value,
                    addValue: body,
                    updateValueFactory: (chunkNumber, data) => data
                );


                // If we received all packets
                if (_receivingChunks.ContainsKey(header.PacketIdentifier.Value) && _receivingChunks[header.PacketIdentifier.Value] == _epsInfo[ep]._chunks[header.PacketIdentifier.Value].Count)
                {
                    int offset = 0;
                    foreach (KeyValuePair<uint, byte[]> chunk in _epsInfo[ep]._chunks[header.PacketIdentifier.Value].OrderBy(x => x.Key))
                    {
                        Array.Copy(chunk.Value, 0, body, offset, chunk.Value.Length);
                        offset += chunk.Value.Length;
                    }
                    _receivingChunks.TryRemove(header.PacketIdentifier.Value, out uint totalChunksNumber);

                    decryptedFullData = PacketUtilities.DecryptMessage(body, Identity.NSec, GetEPNPub(ep));

                    string fileName = Encoding.UTF8.GetString(decryptedFullData);
                    string temporaryFileName = $"{Path.GetFileNameWithoutExtension(fileName)}{_encryptedFileTag}{Path.GetExtension(fileName)}";
                    string fileFullPath = Path.Combine(_defaultTempFileFolder, temporaryFileName);
                    int counter = 1;

                    // We create a temporary file to write file data to disk as soon as they arrive, to prevent abusing ram
                    if (!Directory.Exists(_defaultTempFileFolder))
                        Directory.CreateDirectory(_defaultTempFileFolder);
                    FileStream fs = File.Create(fileFullPath);

                    while (File.Exists(fileFullPath))
                    {
                        temporaryFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{counter++}_{Path.GetExtension(fileName)}";
                        fileFullPath = Path.Combine(_defaultTempFileFolder, temporaryFileName);
                    }

                    _receivingFiles.AddOrUpdate(
                        header.PacketIdentifier.Value,
                        addValue: new(fileFullPath, totalChunksNumber, fs),
                        updateValueFactory: (identifier, value) => value = new(fileFullPath, totalChunksNumber, fs)
                    );

                    return true;
                }
            }
            else
            {
                decryptedFullData = PacketUtilities.DecryptMessage(body, Identity.NSec, GetEPNPub(ep));
                return true;
            }

            return false;
        }
        private void ManageFileContentChunks(EndPoint receivedFrom, uint packetIdentifier, uint chunkNumber, byte[] data)
        {
            if (!_receivingFiles.ContainsKey(packetIdentifier))
                return;


            _receivingFiles[packetIdentifier].ChunksBuffer.TryAdd(chunkNumber, data);
            while (
                _receivingFiles[packetIdentifier].ChunksBuffer.Any()
                && _receivingFiles[packetIdentifier].ChunksBuffer.OrderBy(x => x.Key).First().Key == _receivingFiles[packetIdentifier].CurrentChunk
            )
            {
                uint newChunkNumber = _receivingFiles[packetIdentifier].ChunksBuffer.OrderBy(x => x.Key).First().Key;
                _receivingFiles[packetIdentifier].CurrentChunk = newChunkNumber;
                _receivingFiles[packetIdentifier].Stream.Write(data, 0, data.Length);
                _receivingFiles[packetIdentifier].ChunksBuffer.TryRemove(newChunkNumber, out _);
            }


            // If file ended
            if (_receivingFiles[packetIdentifier].AllChunksReceived)
            {
                string encryptedFileFullPath = _receivingFiles[packetIdentifier].FileFullPath;
                _receivingFiles[packetIdentifier].Stream.Dispose();

                // We create a temporary file to write file data to disk as soon as they arrive, to prevent abusing ram
                if (!Directory.Exists(_defaultTempFileFolder))
                    Directory.CreateDirectory(_defaultTempFileFolder);

                byte[] encryptedData = File.ReadAllBytes(encryptedFileFullPath);
                byte[] decryptedData = PacketUtilities.DecryptMessage(encryptedData, Identity.NSec, GetEPNPub(receivedFrom));
                encryptedData = new byte[0];

                string decryptedFileFullPath = encryptedFileFullPath.Replace(_encryptedFileTag, "");
                int counter = 1;
                while (File.Exists(decryptedFileFullPath))
                    decryptedFileFullPath = $"{Path.GetDirectoryName(decryptedFileFullPath)}{Path.GetFileNameWithoutExtension(decryptedFileFullPath)}_{counter++}_{Path.GetExtension(decryptedFileFullPath)}";
                File.WriteAllBytes(decryptedFileFullPath, decryptedData);
                decryptedData = new byte[0];

                string bech32 = "";
                if (_epsInfo.ContainsKey(receivedFrom))
                    bech32 = _epsInfo[receivedFrom].NPubBech32 ?? "";

                OnFileReceived?.Invoke(receivedFrom, bech32, _receivingFiles[packetIdentifier].FileFullPath);
                _receivingFiles.TryRemove(packetIdentifier, out FileData? fd);
                if (fd is not null)
                    fd.Dispose();
            }
        }


        private void Socket_OnMTUSizeExceed(EndPoint ep, ushort dataSize)
        {
            _epsInfo.AddOrUpdate(
                ep,
                addValue: new(ep),
                updateValueFactory: (endpoint, value) => value.ReduceMTUSize(dataSize)
            );

            foreach (KeyValuePair<string, UnackData> packet in _epsInfo[ep]._unack.Where(x => x.Value.PacketType.HasValue && x.Value.PacketType.Value == PacketType.MTU_DISCOVERY))
            {
                bool removed = _epsInfo[ep]._unack.TryRemove(packet.Value.UID, out UnackData? data);
                data.Dispose();
            }

            if (dataSize < Header._minSize)
                throw new ApplicationException("MTU size is not large enough to contain minimum packet size");
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
                SendConnectionPossible(remoteEP);
                ThreadUtilities.PauseThread(15000);

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
                                // If no collaborative disconnection was initiated we try to start one
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
