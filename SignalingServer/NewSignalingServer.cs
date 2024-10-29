using Newtonsoft.Json;
using RUDP;
using RUDP.Extensions;
using System.Net;

namespace SignalingServer
{
    public partial class NewSignalingServer : Form, IDisposable
    {
        private BaseRUDPSocket _socket { get; set; }


        public NewSignalingServer()
        {
            InitializeComponent();
        }
        public NewSignalingServer(BaseRUDPSocket socket)
        {
            InitializeComponent();

            _socket = socket;
            _socket.OnConnectionConfirmed += _socket_OnConnectionConfirmed;
            _socket.OnConnectionFailed += _socket_OnConnectionFailed;
        }


        private void btnConnect_Click(object sender, EventArgs e)
        {
            EndPoint? ep = tbEndPoint.Text.ToEndPoint();
            if (ep is null)
                return;

            Cursor = Cursors.WaitCursor;
            _socket.TryConnectWith(ep);
        }


        private void _socket_OnConnectionConfirmed(EndPoint ep, string bech32NPub, bool isSigServer, byte relativeIndex)
        {
            BeginInvoke((MethodInvoker)delegate
            {
                List<string> sigServers = JsonConvert.DeserializeObject<List<string>>(Properties.Settings.Default.SigServers) ?? new();
                sigServers.Add(ep.ToIPV4String());
                sigServers = sigServers.Distinct().ToList();
                Properties.Settings.Default.SigServers = JsonConvert.SerializeObject(sigServers);
                Properties.Settings.Default.Save();

                Cursor = Cursors.Default;
                Close();
            });
        }
        private void _socket_OnConnectionFailed(EndPoint ep)
        {
            BeginInvoke((MethodInvoker)delegate
            {
                Cursor = Cursors.Default;
                MessageBox.Show($"Connection with {ep.ToIPV4String()} failed");

                tbEndPoint.Focus();
                tbEndPoint.SelectAll();
            });
        }


        private void NewSignalingServer_FormClosing(object sender, FormClosingEventArgs e)
        {
            _socket.OnConnectionConfirmed -= _socket_OnConnectionConfirmed;
            _socket.OnConnectionFailed -= _socket_OnConnectionFailed;
        }
    }
}
