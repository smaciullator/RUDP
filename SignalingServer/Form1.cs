using ByteSizeLib;
using Newtonsoft.Json;
using NostrSharp.Keys;
using RUDP;
using RUDP.Enums;
using RUDP.Extensions;
using RUDP.Models;
using RUDP.Utilities;
using SignalingServer.Models;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;

namespace SignalingServer;

public partial class Form1 : Form
{
    private RUDPSignalingServer? _socket { get; set; } = null;


    private ConcurrentDictionary<string, EndPoint> _connectedSigServers { get; set; } = new();
    private ConcurrentDictionary<string, PeerEPs> _connectedPeers { get; set; } = new();
    private List<EPDetailsInfo> _signalingServersInfos { get; set; } = new();
    private List<EPDetailsInfo> _peersInfos { get; set; } = new();


    public Form1()
    {
        InitializeComponent();
    }
    private void Form1_Load(object sender, EventArgs e)
    {
        RunBackgroundTask();
    }


    private void btnToggleServer_Click(object sender, EventArgs e)
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


            _socket = new RUDPSignalingServer(true, Properties.Settings.Default.ServerPort, Properties.Settings.Default.NSecBech32);
            _socket.SetMaxUploadSpeed(Properties.Settings.Default.MaxUploadSpeed);
            BindEvents();
            if (_socket.Start() != SocketStatus.Running)
                lbMessage.Text = "Unable to start this Signaling Server Instance";
            else
            {
                lbNPub.Text = NSec.FromBech32(Properties.Settings.Default.NSecBech32).DerivePublicKey().Bech32;
                lbStatus.Text = _socket.Status.ToString();
                lbLocalEndPoint.Text = _socket.LocalEndpoint is null ? "- not defined -" : _socket.LocalEndpoint.ToIPV4String();

                RunBackgroundTask();

                List<string> sigServers = JsonConvert.DeserializeObject<List<string>>(Properties.Settings.Default.SigServers) ?? new();
                foreach (string sigServer in sigServers)
                    _socket.TryConnectWithSignalingServer(sigServer.ToEndPoint());
            }

            btnToggleServer.Text = "Stop";
            btnToggleServer.BackColor = Color.IndianRed;
        }
        else
        {
            string content = "Do you want to stop the current Signaling Server instance?";
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


            btnToggleServer.Text = "Start";
            btnToggleServer.BackColor = Color.LightGreen;

            tabs.SelectedIndex = 0;
        }


        tabs.Enabled = _socket is not null && _socket.Status == SocketStatus.Running;
        Cursor = Cursors.Default;
    }
    private void BindEvents()
    {
        if (_socket is null)
            return;
        _socket.OnConnectionConfirmed += _socket_OnConnectionConfirmed;
        _socket.OnConnectionClosed += _socket_OnConnectionClosed;
        _socket.OnSignalingPropagation += _socket_OnSignalingPropagation;
        _socket.OnP2PCoordinationRequest += _socket_OnP2PCoordinationRequest;
        _socket.OnRateUpdated += _socket_OnRateUpdated;
    }
    private void UnbindEvents()
    {
        if (_socket is null)
            return;
        _socket.OnConnectionConfirmed -= _socket_OnConnectionConfirmed;
        _socket.OnConnectionClosed -= _socket_OnConnectionClosed;
        _socket.OnSignalingPropagation -= _socket_OnSignalingPropagation;
        _socket.OnP2PCoordinationRequest -= _socket_OnP2PCoordinationRequest;
        _socket.OnRateUpdated -= _socket_OnRateUpdated;
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


    private void keysManagerToolStripMenuItem_Click(object sender, EventArgs e)
    {
        new KeysManagement().ShowDialog(this);
    }
    private void serverParametersToolStripMenuItem_Click(object sender, EventArgs e)
    {
        new LocalSignalingServer(_socket).ShowDialog(this);
    }


    private void _socket_OnConnectionConfirmed(EndPoint ep, string bech32NPub, bool isSigServer, byte relativeIndex)
    {
        BeginInvoke((MethodInvoker)delegate
        {
            lbTotConnectedSigServers.Text = _socket.TotalConnectedSigServers.ToString();
            lbTotConnectedPeers.Text = _socket.TotalConnectedPeers.ToString();


            if (isSigServer)
            {
                List<string> sigServers = JsonConvert.DeserializeObject<List<string>>(Properties.Settings.Default.SigServers) ?? new();
                sigServers.Add(ep.ToIPV4String());
                sigServers = sigServers.Distinct().ToList();
                Properties.Settings.Default.SigServers = JsonConvert.SerializeObject(sigServers);
                Properties.Settings.Default.Save();

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

                // Propagate immediately the freshly known peer
                _socket.SendSignalingPropagation(ep, relativeIndex);
            }
        });
    }
    private void _socket_OnConnectionClosed(EndPoint ep, string bech32NPub)
    {
        BeginInvoke((MethodInvoker)delegate
        {
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
    private void _socket_OnSignalingPropagation(EndPoint ep, string bech32NPub, byte relativeIndex)
    {
        _connectedPeers.AddOrUpdate(
            bech32NPub,
            addValue: new PeerEPs().SetEndPoint(ep, relativeIndex, false),
            updateValueFactory: (npub, eps) => eps.SetEndPoint(ep, relativeIndex, eps.Connected)
        );
    }
    private void _socket_OnP2PCoordinationRequest(EndPoint receivedFrom, string requestedBech32NPub, string a)
    {
        if (
            // If we don't know this requested NPub or we don't have any valid endpoint to provide
            (!_connectedPeers.ContainsKey(requestedBech32NPub) || !_connectedPeers[requestedBech32NPub].IsValid())
            // Or if we don't know the NPub who has requested this coordination
            || (!_connectedPeers.Any(x => x.Value.IsValid() && x.Value.EndPointIsKnown(receivedFrom)))
        )
        {
            _socket.SendUnknownIdentity(receivedFrom, requestedBech32NPub);
            return;
        }


        EndPoint? requestedEndPoint = _connectedPeers[requestedBech32NPub].GetKnownEndpoint();
        if (requestedEndPoint is null)
        {
            _socket.SendUnknownIdentity(receivedFrom, requestedBech32NPub);
            return;
        }


        KeyValuePair<string, PeerEPs> sender = _connectedPeers.First(x => x.Value.IsValid() && x.Value.EndPointIsKnown(receivedFrom));
        // First, we send the requested npub's endpoints to the peer who asked this coordination
        _socket.SendP2PConnectionCoordination(receivedFrom, requestedBech32NPub, _connectedPeers[requestedBech32NPub].EP1, _connectedPeers[requestedBech32NPub].EP2, _connectedPeers[requestedBech32NPub].EP3);
        // Second, we send the requester's endpoints to the requested 
        _socket.SendP2PConnectionCoordination(requestedEndPoint, sender.Key, sender.Value.EP1, sender.Value.EP2, sender.Value.EP3);
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


    private void btnAddSignalingServer_Click(object sender, EventArgs e)
    {
        new NewSignalingServer(_socket).ShowDialog();
    }


    private void dgvSignalingServers_CellContentClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.ColumnIndex == 11)
        {
            EPDetailsInfo item = _signalingServersInfos.ElementAt(e.RowIndex);
            string content = "Do you want to close and delete this connection?";
            if (MessageBox.Show(content, "Attention", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                return;
            _socket.DisconnectFrom(item.EndPoint);
        }
    }
}
