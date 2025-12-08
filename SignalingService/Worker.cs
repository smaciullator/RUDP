using ByteSizeLib;
using Newtonsoft.Json;
using RUDP;
using RUDP.Enums;
using RUDP.Extensions;
using RUDP.Models;
using RUDP.Utilities;
using System.Collections.Concurrent;
using System.Net;

namespace SignalingService
{
    public class Worker : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<Worker> _logger;


        private int Port { get; set; } = 40001;
        private string Bech32Identity { get; set; } = "";
        private int MaxUploadSpeed { get; set; } = 0;
        private List<string> SigServers { get; set; } = new();


        private RUDPSignalingServer _socket { get; set; }
        private ConcurrentDictionary<string, PeerEPs> _connectedPeers { get; set; } = new();


        public Worker(IConfiguration config, ILogger<Worker> logger)
        {
            _config = config;
            _logger = logger;


            Port = Convert.ToInt32(config["Port"]);
            Bech32Identity = config["Bech32Identity"];
            MaxUploadSpeed = Convert.ToInt32(config["MaxUploadSpeed"]);
            SigServers = JsonConvert.DeserializeObject<List<string>>(config["SigServers"] ?? "[]");

            _socket = new RUDPSignalingServer(true, Port, Bech32Identity);
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _socket.SetMaxUploadSpeed(MaxUploadSpeed);
            _socket.OnConnectionConfirmed += _socket_OnConnectionConfirmed;
            _socket.OnConnectionClosed += _socket_OnConnectionClosed;
            _socket.OnSignalingPropagation += _socket_OnSignalingPropagation;
            _socket.OnP2PCoordinationRequest += _socket_OnP2PCoordinationRequest;
            _socket.OnRateUpdated += _socket_OnRateUpdated;
            _socket.OnPacketReceived += _socket_OnPacketReceived;


            if (_socket.Start() != SocketStatus.Running)
                _logger.LogError("Unable to start this Signaling Server Instance");
            else
            {
                _logger.LogInformation($"Status: {_socket.Status.ToString()}");
                _logger.LogInformation($"Current Local Endpoint: {(_socket.LocalEndpoint is null ? "- not defined -" : _socket.LocalEndpoint.ToIPV4String())}");

                foreach (string sigServer in SigServers)
                    _socket.TryConnectWithSignalingServer(sigServer.ToEndPoint());
            }


            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }


        private void _socket_OnConnectionConfirmed(EndPoint ep, string bech32NPub, bool isSigServer, byte relativeIndex)
        {
            if (isSigServer)
            {
                _logger.LogInformation($"NEW Signaling Server connected with endpoint {ep.ToIPV4String()}");
                _logger.LogInformation($"Total Signaling Servers connected: {_socket.TotalConnectedSigServers}");
            }
            else
            {
                _logger.LogInformation($"NEW Peer connected with endpoint {ep.ToIPV4String()}");
                _logger.LogInformation($"Total Peers connected: {_socket.TotalConnectedPeers}");

                _connectedPeers.AddOrUpdate(
                    bech32NPub,
                    addValue: new(ep, relativeIndex, true),
                    updateValueFactory: (npub, eps) => eps.SetRelativeIndexEndPoint(ep, relativeIndex, true)
                );

                // Propagate immediately the freshly known peer
                _socket.SendSignalingPropagation(ep, relativeIndex);
            }
        }
        private void _socket_OnConnectionClosed(EndPoint ep, string bech32NPub)
        {
            _logger.LogInformation($"Connection Closed with endpoint {ep.ToIPV4String()} (bech32: {bech32NPub})");
            _logger.LogInformation($"Total Signaling Servers connected: {_socket.TotalConnectedSigServers}");
            _logger.LogInformation($"Total Peers connected: {_socket.TotalConnectedPeers}");
        }
        private void _socket_OnSignalingPropagation(EndPoint ep, string bech32NPub, byte relativeIndex)
        {
            _logger.LogInformation($"NEW Signaling Propagation received: '{ep.ToIPV4String()}' (idx: {relativeIndex}) with identity '{bech32NPub}'");
        }
        private void _socket_OnP2PCoordinationRequest(EndPoint receivedFrom, string a, string requestedBech32NPub)
        {
            if (
                // If we don't know this requested NPub or we don't have any valid endpoint to provide
                (!_connectedPeers.ContainsKey(requestedBech32NPub) || !_connectedPeers[requestedBech32NPub].IsValid())
                // Or if we don't know the NPub who has requested this coordination
                || (!_connectedPeers.Any(x => x.Value.IsValid() && x.Value.EndPointIsKnown(receivedFrom)))
            )
            {
                _logger.LogWarning($"Received a Coordination Request from '{receivedFrom.ToIPV4String()}' for an Unknown Identity '{requestedBech32NPub}'");
                _socket.SendUnknownIdentity(receivedFrom, requestedBech32NPub);
                return;
            }


            EndPoint? requestedEndPoint = _connectedPeers[requestedBech32NPub].GetKnownEndpoint();
            if (requestedEndPoint is null)
            {
                _logger.LogWarning($"Received a Coordination Request from '{receivedFrom.ToIPV4String()}' for an Unknown Identity '{requestedBech32NPub}'");
                _socket.SendUnknownIdentity(receivedFrom, requestedBech32NPub);
                return;
            }


            KeyValuePair<string, PeerEPs> sender = _connectedPeers.First(x => x.Value.IsValid() && x.Value.EndPointIsKnown(receivedFrom));
            // First, we send the requested npub's endpoints to the peer who asked this coordination
            bool sended1 = _socket.SendP2PConnectionCoordination(receivedFrom, requestedBech32NPub, _connectedPeers[requestedBech32NPub].EP1, _connectedPeers[requestedBech32NPub].EP2, _connectedPeers[requestedBech32NPub].EP3);
            if (sended1)
                _logger.LogInformation($"Sent a Coordination Request to REQUESTER for: identity '{requestedBech32NPub}' (ep1: '{_connectedPeers[requestedBech32NPub].EP1.ToIPV4String()}', ep2: '{_connectedPeers[requestedBech32NPub].EP2.ToIPV4String()}', ep3: '{_connectedPeers[requestedBech32NPub].EP3.ToIPV4String()}')");

            // Second, we send the requester's endpoints to the requested 
            bool sended2 = _socket.SendP2PConnectionCoordination(requestedEndPoint, sender.Key, sender.Value.EP1, sender.Value.EP2, sender.Value.EP3);
            if (sended2)
                _logger.LogInformation($"Sent a Coordination Request to RECEIVER for: identity '{sender.Key}' (ep1: '{sender.Value.EP1.ToIPV4String()}', ep2: '{sender.Value.EP2.ToIPV4String()}', ep3: '{sender.Value.EP3.ToIPV4String()}')");

            if (!sended1 || !sended2)
                _logger.LogError($"Not both the requester and the receiver have properly been notified with the Connection Coordination Packet");
        }
        private void _socket_OnRateUpdated(Rates rates, List<EPInfo> epsRates, int sendBufferFillPercentage)
        {
            //_logger.LogInformation($"UPLOAD: {new ByteSize(rates.SentBytesPerSecond).ToString() + "/s"} ({rates.SentPacketsPerSecond.ToString() + " packet/s"})");
            //_logger.LogInformation($"DOWNLOAD: {new ByteSize(rates.ReceivedBytesPerSecond).ToString() + "/s"} ({rates.ReceivedPacketsPerSecond.ToString() + " packet/s"})");
            //_logger.LogInformation($"Send buffer fill percentage: {sendBufferFillPercentage}%)");
        }
        private void _socket_OnPacketReceived(EndPoint ep, Header header, byte[] body, long timestamp)
        {
            if (header.Type == PacketType.RTTA || header.Type == PacketType.RTTB)
                return;
            _logger.LogInformation($"Received '{header.Type.ToString()}' from {ep.ToIPV4String()} - head size: {header.Length} - body size: {body.Length}");
        }
    }
}
