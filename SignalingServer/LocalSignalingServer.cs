using RUDP;
using RUDP.Enums;

namespace SignalingServer
{
    public partial class LocalSignalingServer : Form
    {
        private BaseRUDPSocket _socket { get; set; }


        public LocalSignalingServer()
        {
            InitializeComponent();
        }
        public LocalSignalingServer(BaseRUDPSocket socket)
        {
            InitializeComponent();
            _socket = socket;
        }
        private void LocalSignalingServer_Load(object sender, EventArgs e)
        {
            tbMaxUploadSpeed.Text = Properties.Settings.Default.MaxUploadSpeed.ToString();
            tbPortNumber.Text = Properties.Settings.Default.ServerPort.ToString();
        }


        private void tbPortNumber_Leave(object sender, EventArgs e)
        {
            int.TryParse(tbPortNumber.Text, out int port);
            if (port <= 0 || port > Convert.ToInt32(ushort.MaxValue))
                tbPortNumber.Text = Properties.Settings.Default.ServerPort.ToString();
        }
        private void tbMaxUploadSpeed_Leave(object sender, EventArgs e)
        {
            int.TryParse(tbMaxUploadSpeed.Text, out int port);
            if (port < 0)
                tbMaxUploadSpeed.Text = Properties.Settings.Default.MaxUploadSpeed.ToString();
        }


        private void btnSaveConfigs_Click(object sender, EventArgs e)
        {
            int.TryParse(tbMaxUploadSpeed.Text, out int maxUploadSpeed);
            int.TryParse(tbPortNumber.Text, out int port);


            if (_socket is not null && _socket.Status == SocketStatus.Running)
            {
                _socket.SetMaxUploadSpeed(maxUploadSpeed);
                if (Properties.Settings.Default.ServerPort != port)
                    MessageBox.Show("Port number change will take effect only after a full restart");
            }


            Properties.Settings.Default.MaxUploadSpeed = maxUploadSpeed;
            Properties.Settings.Default.ServerPort = port;
            Properties.Settings.Default.Save();
            Close();
        }
    }
}
