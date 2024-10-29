namespace Client
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            splitContainer1 = new SplitContainer();
            splitContainer2 = new SplitContainer();
            tabs = new TabControl();
            tbGenerals = new TabPage();
            lbSendBuffersStatus = new Label();
            lbPacketsDownload = new Label();
            lbPacketsUpload = new Label();
            lbBytesDownload = new Label();
            label8 = new Label();
            lbBytesUpload = new Label();
            label6 = new Label();
            lbLocalEndPoint = new Label();
            label5 = new Label();
            lbTotConnectedPeers = new Label();
            lbTotConnectedSigServers = new Label();
            label4 = new Label();
            label3 = new Label();
            lbStatus = new Label();
            label2 = new Label();
            lbNPub = new Label();
            label1 = new Label();
            tbPeers = new TabPage();
            dgvPeers = new DataGridView();
            dataGridViewTextBoxColumn2 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn3 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn4 = new DataGridViewTextBoxColumn();
            Disconnect = new DataGridViewButtonColumn();
            bsPeers = new BindingSource(components);
            tbSignalingServers = new TabPage();
            dgvSignalingServers = new DataGridView();
            CurrentUploadSpeed = new DataGridViewTextBoxColumn();
            PacketsUploadPerSecond = new DataGridViewTextBoxColumn();
            CurrentDownloadSpeed = new DataGridViewTextBoxColumn();
            bsSignalingServers = new BindingSource(components);
            lbMessage = new Label();
            btnToggleSocket = new Button();
            menuStrip1 = new MenuStrip();
            keysManagerToolStripMenuItem = new ToolStripMenuItem();
            clientToolStripMenuItem = new ToolStripMenuItem();
            nostrContactsToolStripMenuItem = new ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            tabs.SuspendLayout();
            tbGenerals.SuspendLayout();
            tbPeers.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPeers).BeginInit();
            ((System.ComponentModel.ISupportInitialize)bsPeers).BeginInit();
            tbSignalingServers.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvSignalingServers).BeginInit();
            ((System.ComponentModel.ISupportInitialize)bsSignalingServers).BeginInit();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.FixedPanel = FixedPanel.Panel2;
            splitContainer1.Location = new Point(0, 24);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(btnToggleSocket);
            splitContainer1.Size = new Size(1123, 596);
            splitContainer1.SplitterDistance = 552;
            splitContainer1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.FixedPanel = FixedPanel.Panel2;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(tabs);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(lbMessage);
            splitContainer2.Size = new Size(1123, 552);
            splitContainer2.SplitterDistance = 514;
            splitContainer2.TabIndex = 0;
            // 
            // tabs
            // 
            tabs.Controls.Add(tbGenerals);
            tabs.Controls.Add(tbPeers);
            tabs.Controls.Add(tbSignalingServers);
            tabs.Dock = DockStyle.Fill;
            tabs.Location = new Point(0, 0);
            tabs.Name = "tabs";
            tabs.SelectedIndex = 0;
            tabs.Size = new Size(1123, 514);
            tabs.TabIndex = 0;
            // 
            // tbGenerals
            // 
            tbGenerals.Controls.Add(lbSendBuffersStatus);
            tbGenerals.Controls.Add(lbPacketsDownload);
            tbGenerals.Controls.Add(lbPacketsUpload);
            tbGenerals.Controls.Add(lbBytesDownload);
            tbGenerals.Controls.Add(label8);
            tbGenerals.Controls.Add(lbBytesUpload);
            tbGenerals.Controls.Add(label6);
            tbGenerals.Controls.Add(lbLocalEndPoint);
            tbGenerals.Controls.Add(label5);
            tbGenerals.Controls.Add(lbTotConnectedPeers);
            tbGenerals.Controls.Add(lbTotConnectedSigServers);
            tbGenerals.Controls.Add(label4);
            tbGenerals.Controls.Add(label3);
            tbGenerals.Controls.Add(lbStatus);
            tbGenerals.Controls.Add(label2);
            tbGenerals.Controls.Add(lbNPub);
            tbGenerals.Controls.Add(label1);
            tbGenerals.Location = new Point(4, 24);
            tbGenerals.Name = "tbGenerals";
            tbGenerals.Padding = new Padding(3);
            tbGenerals.Size = new Size(1115, 486);
            tbGenerals.TabIndex = 0;
            tbGenerals.Text = "Generals";
            tbGenerals.UseVisualStyleBackColor = true;
            // 
            // lbSendBuffersStatus
            // 
            lbSendBuffersStatus.AutoSize = true;
            lbSendBuffersStatus.Location = new Point(63, 241);
            lbSendBuffersStatus.Name = "lbSendBuffersStatus";
            lbSendBuffersStatus.Size = new Size(99, 15);
            lbSendBuffersStatus.TabIndex = 33;
            lbSendBuffersStatus.Text = "send buffer at 0%";
            // 
            // lbPacketsDownload
            // 
            lbPacketsDownload.AutoSize = true;
            lbPacketsDownload.Location = new Point(80, 283);
            lbPacketsDownload.Name = "lbPacketsDownload";
            lbPacketsDownload.Size = new Size(13, 15);
            lbPacketsDownload.TabIndex = 32;
            lbPacketsDownload.Text = "0";
            // 
            // lbPacketsUpload
            // 
            lbPacketsUpload.AutoSize = true;
            lbPacketsUpload.Location = new Point(63, 226);
            lbPacketsUpload.Name = "lbPacketsUpload";
            lbPacketsUpload.Size = new Size(13, 15);
            lbPacketsUpload.TabIndex = 31;
            lbPacketsUpload.Text = "0";
            // 
            // lbBytesDownload
            // 
            lbBytesDownload.AutoSize = true;
            lbBytesDownload.Location = new Point(80, 268);
            lbBytesDownload.Name = "lbBytesDownload";
            lbBytesDownload.Size = new Size(13, 15);
            lbBytesDownload.TabIndex = 30;
            lbBytesDownload.Text = "0";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label8.Location = new Point(8, 268);
            label8.Name = "label8";
            label8.Size = new Size(66, 15);
            label8.TabIndex = 29;
            label8.Text = "Download:";
            // 
            // lbBytesUpload
            // 
            lbBytesUpload.AutoSize = true;
            lbBytesUpload.Location = new Point(63, 211);
            lbBytesUpload.Name = "lbBytesUpload";
            lbBytesUpload.Size = new Size(13, 15);
            lbBytesUpload.TabIndex = 28;
            lbBytesUpload.Text = "0";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label6.Location = new Point(8, 211);
            label6.Name = "label6";
            label6.Size = new Size(49, 15);
            label6.TabIndex = 27;
            label6.Text = "Upload:";
            // 
            // lbLocalEndPoint
            // 
            lbLocalEndPoint.AutoSize = true;
            lbLocalEndPoint.Location = new Point(104, 45);
            lbLocalEndPoint.Name = "lbLocalEndPoint";
            lbLocalEndPoint.Size = new Size(0, 15);
            lbLocalEndPoint.TabIndex = 26;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label5.Location = new Point(8, 45);
            label5.Name = "label5";
            label5.Size = new Size(90, 15);
            label5.TabIndex = 25;
            label5.Text = "Local Endpoint:";
            // 
            // lbTotConnectedPeers
            // 
            lbTotConnectedPeers.AutoSize = true;
            lbTotConnectedPeers.Location = new Point(118, 138);
            lbTotConnectedPeers.Name = "lbTotConnectedPeers";
            lbTotConnectedPeers.Size = new Size(13, 15);
            lbTotConnectedPeers.TabIndex = 24;
            lbTotConnectedPeers.Text = "0";
            // 
            // lbTotConnectedSigServers
            // 
            lbTotConnectedSigServers.AutoSize = true;
            lbTotConnectedSigServers.Location = new Point(183, 106);
            lbTotConnectedSigServers.Name = "lbTotConnectedSigServers";
            lbTotConnectedSigServers.Size = new Size(13, 15);
            lbTotConnectedSigServers.TabIndex = 23;
            lbTotConnectedSigServers.Text = "0";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label4.Location = new Point(8, 138);
            label4.Name = "label4";
            label4.Size = new Size(104, 15);
            label4.TabIndex = 22;
            label4.Text = "Connected Peers:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label3.Location = new Point(8, 106);
            label3.Name = "label3";
            label3.Size = new Size(169, 15);
            label3.TabIndex = 21;
            label3.Text = "Connected Signaling Servers:";
            // 
            // lbStatus
            // 
            lbStatus.AutoSize = true;
            lbStatus.Location = new Point(57, 14);
            lbStatus.Name = "lbStatus";
            lbStatus.Size = new Size(0, 15);
            lbStatus.TabIndex = 20;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label2.Location = new Point(8, 14);
            label2.Name = "label2";
            label2.Size = new Size(45, 15);
            label2.TabIndex = 19;
            label2.Text = "Status:";
            // 
            // lbNPub
            // 
            lbNPub.AutoSize = true;
            lbNPub.Location = new Point(148, 75);
            lbNPub.Name = "lbNPub";
            lbNPub.Size = new Size(0, 15);
            lbNPub.TabIndex = 18;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label1.Location = new Point(8, 75);
            label1.Name = "label1";
            label1.Size = new Size(136, 15);
            label1.TabIndex = 17;
            label1.Text = "Current Public Identity:";
            // 
            // tbPeers
            // 
            tbPeers.Controls.Add(dgvPeers);
            tbPeers.Location = new Point(4, 24);
            tbPeers.Name = "tbPeers";
            tbPeers.Padding = new Padding(3);
            tbPeers.Size = new Size(1115, 486);
            tbPeers.TabIndex = 1;
            tbPeers.Text = "Peers Network";
            tbPeers.UseVisualStyleBackColor = true;
            // 
            // dgvPeers
            // 
            dgvPeers.AllowUserToAddRows = false;
            dgvPeers.AllowUserToDeleteRows = false;
            dgvPeers.AutoGenerateColumns = false;
            dgvPeers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvPeers.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn2, dataGridViewTextBoxColumn3, dataGridViewTextBoxColumn4, Disconnect });
            dgvPeers.DataSource = bsPeers;
            dgvPeers.Dock = DockStyle.Fill;
            dgvPeers.Location = new Point(3, 3);
            dgvPeers.MultiSelect = false;
            dgvPeers.Name = "dgvPeers";
            dgvPeers.RowHeadersVisible = false;
            dgvPeers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPeers.Size = new Size(1109, 480);
            dgvPeers.TabIndex = 2;
            dgvPeers.CellContentClick += dgvPeers_CellContentClick;
            // 
            // dataGridViewTextBoxColumn2
            // 
            dataGridViewTextBoxColumn2.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewTextBoxColumn2.DataPropertyName = "CurrentUploadSpeed";
            dataGridViewTextBoxColumn2.HeaderText = "Upload Speed";
            dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            dataGridViewTextBoxColumn2.ReadOnly = true;
            dataGridViewTextBoxColumn2.Width = 97;
            // 
            // dataGridViewTextBoxColumn3
            // 
            dataGridViewTextBoxColumn3.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewTextBoxColumn3.DataPropertyName = "PacketsUploadPerSecond";
            dataGridViewTextBoxColumn3.HeaderText = "Packets Up";
            dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            dataGridViewTextBoxColumn3.ReadOnly = true;
            dataGridViewTextBoxColumn3.Width = 83;
            // 
            // dataGridViewTextBoxColumn4
            // 
            dataGridViewTextBoxColumn4.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewTextBoxColumn4.DataPropertyName = "CurrentDownloadSpeed";
            dataGridViewTextBoxColumn4.HeaderText = "Download Speed";
            dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            dataGridViewTextBoxColumn4.ReadOnly = true;
            dataGridViewTextBoxColumn4.Width = 111;
            // 
            // Disconnect
            // 
            Disconnect.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = Color.Moccasin;
            Disconnect.DefaultCellStyle = dataGridViewCellStyle1;
            Disconnect.FlatStyle = FlatStyle.Popup;
            Disconnect.HeaderText = "Disconnect";
            Disconnect.Name = "Disconnect";
            Disconnect.ReadOnly = true;
            Disconnect.Width = 72;
            // 
            // tbSignalingServers
            // 
            tbSignalingServers.Controls.Add(dgvSignalingServers);
            tbSignalingServers.Location = new Point(4, 24);
            tbSignalingServers.Name = "tbSignalingServers";
            tbSignalingServers.Size = new Size(1115, 486);
            tbSignalingServers.TabIndex = 2;
            tbSignalingServers.Text = "Signaling Servers";
            tbSignalingServers.UseVisualStyleBackColor = true;
            // 
            // dgvSignalingServers
            // 
            dgvSignalingServers.AllowUserToAddRows = false;
            dgvSignalingServers.AllowUserToDeleteRows = false;
            dgvSignalingServers.AutoGenerateColumns = false;
            dgvSignalingServers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvSignalingServers.Columns.AddRange(new DataGridViewColumn[] { CurrentUploadSpeed, PacketsUploadPerSecond, CurrentDownloadSpeed });
            dgvSignalingServers.DataSource = bsSignalingServers;
            dgvSignalingServers.Dock = DockStyle.Fill;
            dgvSignalingServers.Location = new Point(0, 0);
            dgvSignalingServers.MultiSelect = false;
            dgvSignalingServers.Name = "dgvSignalingServers";
            dgvSignalingServers.RowHeadersVisible = false;
            dgvSignalingServers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvSignalingServers.Size = new Size(1115, 486);
            dgvSignalingServers.TabIndex = 0;
            // 
            // CurrentUploadSpeed
            // 
            CurrentUploadSpeed.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            CurrentUploadSpeed.DataPropertyName = "CurrentUploadSpeed";
            CurrentUploadSpeed.HeaderText = "Upload Speed";
            CurrentUploadSpeed.Name = "CurrentUploadSpeed";
            CurrentUploadSpeed.ReadOnly = true;
            CurrentUploadSpeed.Width = 97;
            // 
            // PacketsUploadPerSecond
            // 
            PacketsUploadPerSecond.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            PacketsUploadPerSecond.DataPropertyName = "PacketsUploadPerSecond";
            PacketsUploadPerSecond.HeaderText = "Packets Up";
            PacketsUploadPerSecond.Name = "PacketsUploadPerSecond";
            PacketsUploadPerSecond.ReadOnly = true;
            PacketsUploadPerSecond.Width = 83;
            // 
            // CurrentDownloadSpeed
            // 
            CurrentDownloadSpeed.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            CurrentDownloadSpeed.DataPropertyName = "CurrentDownloadSpeed";
            CurrentDownloadSpeed.HeaderText = "Download Speed";
            CurrentDownloadSpeed.Name = "CurrentDownloadSpeed";
            CurrentDownloadSpeed.ReadOnly = true;
            CurrentDownloadSpeed.Width = 111;
            // 
            // lbMessage
            // 
            lbMessage.AutoSize = true;
            lbMessage.Dock = DockStyle.Fill;
            lbMessage.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lbMessage.Location = new Point(0, 0);
            lbMessage.Name = "lbMessage";
            lbMessage.Size = new Size(0, 20);
            lbMessage.TabIndex = 0;
            lbMessage.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnToggleSocket
            // 
            btnToggleSocket.BackColor = Color.LightGreen;
            btnToggleSocket.Dock = DockStyle.Fill;
            btnToggleSocket.FlatStyle = FlatStyle.Popup;
            btnToggleSocket.Location = new Point(0, 0);
            btnToggleSocket.Name = "btnToggleSocket";
            btnToggleSocket.Size = new Size(1123, 40);
            btnToggleSocket.TabIndex = 0;
            btnToggleSocket.Text = "Start";
            btnToggleSocket.UseVisualStyleBackColor = false;
            btnToggleSocket.Click += btnToggleSocket_Click;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { keysManagerToolStripMenuItem, clientToolStripMenuItem, nostrContactsToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1123, 24);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // keysManagerToolStripMenuItem
            // 
            keysManagerToolStripMenuItem.Name = "keysManagerToolStripMenuItem";
            keysManagerToolStripMenuItem.Size = new Size(93, 20);
            keysManagerToolStripMenuItem.Text = "Keys Manager";
            keysManagerToolStripMenuItem.Click += keysManagerToolStripMenuItem_Click;
            // 
            // clientToolStripMenuItem
            // 
            clientToolStripMenuItem.Name = "clientToolStripMenuItem";
            clientToolStripMenuItem.Size = new Size(112, 20);
            clientToolStripMenuItem.Text = "Client Parameters";
            clientToolStripMenuItem.Click += clientToolStripMenuItem_Click;
            // 
            // nostrContactsToolStripMenuItem
            // 
            nostrContactsToolStripMenuItem.Name = "nostrContactsToolStripMenuItem";
            nostrContactsToolStripMenuItem.Size = new Size(98, 20);
            nostrContactsToolStripMenuItem.Text = "Nostr Contacts";
            nostrContactsToolStripMenuItem.Click += nostrContactsToolStripMenuItem_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1123, 620);
            Controls.Add(splitContainer1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "Form1";
            Text = "Client";
            Load += Form1_Load;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            tabs.ResumeLayout(false);
            tbGenerals.ResumeLayout(false);
            tbGenerals.PerformLayout();
            tbPeers.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvPeers).EndInit();
            ((System.ComponentModel.ISupportInitialize)bsPeers).EndInit();
            tbSignalingServers.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvSignalingServers).EndInit();
            ((System.ComponentModel.ISupportInitialize)bsSignalingServers).EndInit();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private SplitContainer splitContainer1;
        private MenuStrip menuStrip1;
        private Button btnToggleSocket;
        private ToolStripMenuItem keysManagerToolStripMenuItem;
        private ToolStripMenuItem clientToolStripMenuItem;
        private SplitContainer splitContainer2;
        private Label lbMessage;
        private TabControl tabs;
        private TabPage tbGenerals;
        private TabPage tbPeers;
        private Label lbSendBuffersStatus;
        private Label lbPacketsDownload;
        private Label lbPacketsUpload;
        private Label lbBytesDownload;
        private Label label8;
        private Label lbBytesUpload;
        private Label label6;
        private Label lbLocalEndPoint;
        private Label label5;
        private Label lbTotConnectedPeers;
        private Label lbTotConnectedSigServers;
        private Label label4;
        private Label label3;
        private Label lbStatus;
        private Label label2;
        private Label lbNPub;
        private Label label1;
        private TabPage tbSignalingServers;
        private BindingSource bsSignalingServers;
        private BindingSource bsPeers;
        private DataGridView dgvSignalingServers;
        private DataGridView dgvPeers;
        private DataGridViewTextBoxColumn iPV4AddressDataGridViewTextBoxColumn;
        private DataGridViewCheckBoxColumn isConnectedDataGridViewCheckBoxColumn;
        private DataGridViewCheckBoxColumn disconnectingDataGridViewCheckBoxColumn;
        private DataGridViewTextBoxColumn CurrentUploadSpeed;
        private DataGridViewTextBoxColumn PacketsUploadPerSecond;
        private DataGridViewTextBoxColumn CurrentDownloadSpeed;
        private DataGridViewTextBoxColumn dgvcPacketsDownloadPerSecond;
        private DataGridViewTextBoxColumn maxUploadSpeedDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn dgvcSafeBandWidth;
        private DataGridViewTextBoxColumn mTUSizeDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn nPubBech32DataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn1;
        private DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn2;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn7;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn8;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn9;
        private DataGridViewButtonColumn Disconnect;
        private ToolStripMenuItem nostrContactsToolStripMenuItem;
    }
}
