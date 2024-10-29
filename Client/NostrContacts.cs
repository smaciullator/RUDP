using Newtonsoft.Json;
using NostrSharp;
using NostrSharp.Keys;
using NostrSharp.Nostr;
using NostrSharp.Nostr.Models;
using NostrSharp.Relay.Models;
using RUDP;
using RUDP.Extensions;
using RUDP.Utilities;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Client
{
    public partial class NostrContacts : Form
    {
        private RUDPClient _socket { get; set; }
        private List<NSRelayConfig> relaysDefaultList { get; set; } = new()
        {
            new("nos.lol"),
            new("relay.damus.io"),
            new("relay.nostr.band"),
            new("relay.snort.social"),
            new("nostr.orangepill.dev"),
            new("nostr.wine")
        };
        private NSMain ns { get; set; } = new();
        private Dictionary<string, UserMetadata> _cache { get; set; } = new();
        public List<UserMetadata> _contacts { get; set; } = new();
        public List<UserMetadata> _filteredContacts { get; set; } = new();


        public NostrContacts()
        {
            InitializeComponent();
        }
        public NostrContacts(RUDPClient socket)
        {
            InitializeComponent();
            _socket = socket;
        }
        private async void NostrContacts_Load(object sender, EventArgs e)
        {
            _cache = JsonConvert.DeserializeObject<Dictionary<string, UserMetadata>>(Properties.Settings.Default.NostrContactsCache) ?? new();
            _contacts = _cache.Values.DistinctBy(x => x.PubKey).ToList();

            if (ns.Init(NSec.FromBech32(Properties.Settings.Default.NSecBech32).DerivePublicKey()))
            {
                RunBackgroundTasks();
                ns.OnContacts += Ns_OnContacts;
                ns.OnMetadata += Ns_OnMetadata;
                await ns.ConnectRelays(relaysDefaultList);
            }
        }


        private void RunBackgroundTasks()
        {
            Task.Factory.StartNew(() =>
            {
                Stopwatch _backgroundTaskTimer = Stopwatch.StartNew();

                List<UserMetadata> _backup = new();
                while (ns.CanRead)
                {
                    try
                    {
                        while (_backgroundTaskTimer.Elapsed.TotalMilliseconds < 500)
                            ThreadUtilities.PauseThread(100);

                        if (string.IsNullOrEmpty(tbFilter.Text) && _filteredContacts.Count == 0)
                        {
                            if (_contacts.Any(x => !_backup.Any(y => y.PubKey == x.PubKey)))
                                BeginInvoke((MethodInvoker)delegate
                                {
                                    bsContacts.DataSource = null;
                                    bsContacts.DataSource = _contacts;
                                    bsContacts.ResetBindings(false);
                                });
                            _backup = _contacts;
                        }
                        else
                        {
                            if (
                                _filteredContacts.Count > 0
                                && (_filteredContacts.Count != _backup.Count || _filteredContacts.Any(x => !_backup.Any(y => y.PubKey == x.PubKey)))
                            )
                            {
                                BeginInvoke((MethodInvoker)delegate
                                {
                                    bsContacts.DataSource = null;
                                    bsContacts.DataSource = _filteredContacts;
                                    bsContacts.ResetBindings(false);
                                });
                                _backup = _filteredContacts;
                            }
                        }

                        _backgroundTaskTimer.Restart();
                    }
                    catch (Exception ex)
                    {

                    }
                }
                _backgroundTaskTimer.Stop();
            }, TaskCreationOptions.LongRunning);
        }


        private async void Ns_OnContacts(Uri relay, NEvent ev, List<RelayInfo> relays)
        {
            foreach (NTag tag in ev.GetTagsByKey("p"))
                if (tag.Value is not null)
                    await ns.GetPubkeyMetadata(tag.Value, relay);
        }
        private void Ns_OnMetadata(Uri relay, UserMetadata? data, NEvent ev)
        {
            if (string.IsNullOrEmpty(ev.PubKey) || data is null)
                return;

            if (_cache.ContainsKey(ev.PubKey))
                _cache[ev.PubKey] = data;
            else
                _cache.TryAdd(ev.PubKey, data);

            _contacts = _cache.Values.DistinctBy(x => x.PubKey).OrderBy(x => x.Display_Name).ToList();
        }
        private void btnFilter_Click(object sender, EventArgs e)
        {
            _filteredContacts = new();
            foreach (UserMetadata contact in _contacts)
            {
                if (
                    (!string.IsNullOrEmpty(contact.Username) && contact.Username.Contains(tbFilter.Text))
                    || (!string.IsNullOrEmpty(contact.Name) && contact.Name.Contains(tbFilter.Text))
                    || (!string.IsNullOrEmpty(contact.DisplayName) && contact.DisplayName.Contains(tbFilter.Text))
                    || (!string.IsNullOrEmpty(contact.Display_Name) && contact.Display_Name.Contains(tbFilter.Text))
                    || (!string.IsNullOrEmpty(contact.Nip05) && contact.Nip05.Contains(tbFilter.Text))
                    || (!string.IsNullOrEmpty(contact.PubKey) && contact.PubKey.Contains(tbFilter.Text))
                )
                    _filteredContacts.Add(contact);
            }
        }
        private async void btnUpdate_Click(object sender, EventArgs e)
        {
            await ns.GetMyContacts();
        }


        private void dgvContacts_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 3)
            {
                Focus();
                if (_socket is null)
                {
                    MessageBox.Show("Socket is not running.");
                    return;
                }


                string content = "Do you want to try a connection with this Peer?";
                if (MessageBox.Show(content, "Attention", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                    return;

                EndPoint? sigSerEP = Properties.Settings.Default.SigServer1.ToEndPoint();
                if (sigSerEP is null)
                {
                    sigSerEP = Properties.Settings.Default.SigServer2.ToEndPoint();
                    if (sigSerEP is null)
                    {
                        sigSerEP = Properties.Settings.Default.SigServer3.ToEndPoint();
                        if (sigSerEP is null)
                        {
                            MessageBox.Show("No valid Signaling Server endpoint has been set");
                            return;
                        }
                    }
                }

                string hexKey = dgvContacts[4, e.RowIndex].Value.ToString() ?? "";
                if (string.IsNullOrEmpty(hexKey))
                {
                    MessageBox.Show("Can't find a valid PubKey for the selected peer");
                    return;
                }

                try
                {
                    NPub npub = NPub.FromHex(hexKey);
                    _socket.OnP2PConnectionPossible += _socket_OnP2PConnectionPossible;
                    _socket.OnConnectionConfirmed += _socket_OnConnectionConfirmed;
                    _socket.OnConnectionFailed += _socket_OnConnectionFailed;
                    if (!_socket.SendP2PCoordinationRequest(sigSerEP, npub.Bech32))
                        MessageBox.Show("Error before attempting a connection with the selected peer");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Can't find a valid PubKey for the selected peer");
                }
            }
        }


        private void _socket_OnP2PConnectionPossible(EndPoint ep, string bech32NPub)
        {
            _socket.TryConnectWithPeer(ep);
        }
        private void _socket_OnConnectionConfirmed(EndPoint ep, string bech32NPub, bool isSigServer, byte relativeIndex)
        {
            MessageBox.Show($"Connection succesful with {ep.ToIPV4String()}");
            Close();
        }
        private void _socket_OnConnectionFailed(EndPoint ep)
        {
            MessageBox.Show($"Connection failed with {ep.ToIPV4String()}");
        }

        private void dgvContacts_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            //if (dgvContacts.Columns[e.ColumnIndex].Name == "Image")
            //{
            //    string img = _cache[dgvContacts["PubKey", e.RowIndex].Value.ToString()].Picture;
            //    if (
            //        !string.IsNullOrEmpty(img)
            //        && (img.EndsWith(".png") || img.EndsWith(".jpg"))
            //    )
            //        e.Value = GetImageFromUrl(img);
            //}
        }
        public static Image GetImageFromUrl(string url)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            using (HttpWebResponse httpWebReponse = (HttpWebResponse)httpWebRequest.GetResponse())
            using (Stream stream = httpWebReponse.GetResponseStream())
                return Image.FromStream(stream);
        }


        private void NostrContacts_FormClosing(object sender, FormClosingEventArgs e)
        {
            ns.Dispose();

            if (_socket is not null)
            {
                _socket.OnP2PConnectionPossible -= _socket_OnP2PConnectionPossible;
                _socket.OnConnectionConfirmed -= _socket_OnConnectionConfirmed;
                _socket.OnConnectionFailed -= _socket_OnConnectionFailed;
            }


            Properties.Settings.Default.NostrContactsCache = JsonConvert.SerializeObject(_cache);
            Properties.Settings.Default.Save();
        }
    }
}
