using RUDP;
using RUDP.Enums;
using RUDP.Extensions;
using System.Net;

namespace Client
{
    public partial class LocalClientSettings : Form
    {
        private BaseRUDPSocket _socket { get; set; }


        public LocalClientSettings()
        {
            InitializeComponent();
        }
        public LocalClientSettings(BaseRUDPSocket socket)
        {
            InitializeComponent();
            _socket = socket;
        }
        private void LocalSignalingServer_Load(object sender, EventArgs e)
        {
            tbMaxUploadSpeed.Text = Properties.Settings.Default.MaxUploadSpeed.ToString();
            tbPortNumber.Text = Properties.Settings.Default.ServerPort.ToString();
            tbSignalingServer1.Text = Properties.Settings.Default.SigServer1;
            tbSignalingServer2.Text = Properties.Settings.Default.SigServer2;
            tbSignalingServer3.Text = Properties.Settings.Default.SigServer3;
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
        private void tbSignalingServer1_Leave(object sender, EventArgs e)
        {
            EndPoint? ep = tbSignalingServer1.Text.ToEndPoint();
            if (ep is null)
                tbSignalingServer1.Text = Properties.Settings.Default.SigServer1;
        }
        private void tbSignalingServer2_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbSignalingServer2.Text))
                return;
            EndPoint? ep = tbSignalingServer2.Text.ToEndPoint();
            if (ep is null)
                tbSignalingServer2.Text = Properties.Settings.Default.SigServer2;
        }
        private void tbSignalingServer3_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbSignalingServer2.Text))
                return;
            EndPoint? ep = tbSignalingServer3.Text.ToEndPoint();
            if (ep is null)
                tbSignalingServer3.Text = Properties.Settings.Default.SigServer3;
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


            Properties.Settings.Default.SigServer1 = tbSignalingServer1.Text;
            Properties.Settings.Default.SigServer2 = tbSignalingServer2.Text;
            Properties.Settings.Default.SigServer3 = tbSignalingServer3.Text;
            Properties.Settings.Default.MaxUploadSpeed = maxUploadSpeed;
            Properties.Settings.Default.ServerPort = port;
            Properties.Settings.Default.Save();
            Close();
        }
    }
}
