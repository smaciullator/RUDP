using NostrSharp.Keys;
using RUDP.Utilities;

namespace Client
{
    public partial class KeysManagement : Form
    {
        private NSKeyPair? _keys { get; set; } = null;
        private bool _pendingSave { get; set; } = false;


        public KeysManagement()
        {
            InitializeComponent();
        }
        private void KeysManagement_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Properties.Settings.Default.NSecBech32))
                try { _keys = NSKeyPair.From(NSec.FromBech32(Properties.Settings.Default.NSecBech32)); }
                catch { _keys = null; }

            if (_keys is not null)
            {
                tbNSec.Text = _keys.NSec.Bech32;
                tbNPub.Text = _keys.NPub.Bech32;
            }
        }


        private void tbNSec_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbNSec.Text))
                return;
            try
            {
                NSKeyPair keys = NSKeyPair.From(NSec.FromBech32(tbNSec.Text));
                tbNPub.Text = keys.NPub.Bech32;
            }
            catch { }
        }
        private void btnGenerateNew_Click(object sender, EventArgs e)
        {
            if (_keys is not null)
            {
                string content = "This will create a new Key Pair, but your current keys will not be replace until you press 'Save'. Do you want to continue?";
                if (MessageBox.Show(content, "Attention", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                    return;
            }

            _keys = NSKeyPair.GenerateNew();
            tbNSec.Text = _keys.NSec.Bech32;
            tbNPub.Text = _keys.NPub.Bech32;
            _pendingSave = true;
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            lbError.Text = "";


            if (string.IsNullOrEmpty(tbNSec.Text))
            {
                lbError.Text = "NSec not inserted";
                Cursor = Cursors.Default;
                return;
            }

            NSec? nsec = null;
            try { nsec = NSec.FromBech32(tbNSec.Text); }
            catch
            {
                try { nsec = NSec.FromHex(tbNSec.Text); }
                catch { nsec = null; }
            }
            if (nsec is null)
            {
                lbError.Text = "NSec format incorrect";
                Cursor = Cursors.Default;
                return;
            }


            Properties.Settings.Default.NSecBech32 = nsec.Bech32;
            Properties.Settings.Default.Save();
            _pendingSave = false;


            ThreadUtilities.PauseThread(300);
            Cursor = Cursors.Default;
        }


        private void KeysManagement_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_pendingSave)
            {
                string content = "You have some unsaved changes, if you close now you will lose them. Do you want to continue?";
                DialogResult result = MessageBox.Show(content, "Attention", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                e.Cancel = result == DialogResult.No;
            }
        }
    }
}
