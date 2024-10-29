namespace SignalingServer;

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
        tbGeneral = new TabPage();
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
        tbSigServerNetwork = new TabPage();
        splitContainer3 = new SplitContainer();
        btnAddSignalingServer = new Button();
        dgvSignalingServers = new DataGridView();
        iPV4AddressDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
        isConnectedDataGridViewCheckBoxColumn = new DataGridViewCheckBoxColumn();
        CurrentUploadSpeed = new DataGridViewTextBoxColumn();
        PacketsUploadPerSecond = new DataGridViewTextBoxColumn();
        CurrentDownloadSpeed = new DataGridViewTextBoxColumn();
        dgvcPacketsDownloadPerSecond = new DataGridViewTextBoxColumn();
        maxUploadSpeedDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
        dgvcSafeBandWidth = new DataGridViewTextBoxColumn();
        mTUSizeDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
        nPubBech32DataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
        Delete = new DataGridViewButtonColumn();
        bsSignalingServers = new BindingSource(components);
        tbPeerNetwork = new TabPage();
        dgvPeers = new DataGridView();
        dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
        dataGridViewCheckBoxColumn1 = new DataGridViewCheckBoxColumn();
        dataGridViewTextBoxColumn2 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn3 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn4 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn5 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn6 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn7 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn8 = new DataGridViewTextBoxColumn();
        dataGridViewTextBoxColumn9 = new DataGridViewTextBoxColumn();
        bsPeers = new BindingSource(components);
        lbMessage = new Label();
        btnToggleServer = new Button();
        menu = new MenuStrip();
        keysManagerToolStripMenuItem = new ToolStripMenuItem();
        serverParametersToolStripMenuItem = new ToolStripMenuItem();
        ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
        splitContainer1.Panel1.SuspendLayout();
        splitContainer1.Panel2.SuspendLayout();
        splitContainer1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
        splitContainer2.Panel1.SuspendLayout();
        splitContainer2.Panel2.SuspendLayout();
        splitContainer2.SuspendLayout();
        tabs.SuspendLayout();
        tbGeneral.SuspendLayout();
        tbSigServerNetwork.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitContainer3).BeginInit();
        splitContainer3.Panel1.SuspendLayout();
        splitContainer3.Panel2.SuspendLayout();
        splitContainer3.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvSignalingServers).BeginInit();
        ((System.ComponentModel.ISupportInitialize)bsSignalingServers).BeginInit();
        tbPeerNetwork.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvPeers).BeginInit();
        ((System.ComponentModel.ISupportInitialize)bsPeers).BeginInit();
        menu.SuspendLayout();
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
        splitContainer1.Panel2.Controls.Add(btnToggleServer);
        splitContainer1.Size = new Size(1085, 495);
        splitContainer1.SplitterDistance = 453;
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
        splitContainer2.Size = new Size(1085, 453);
        splitContainer2.SplitterDistance = 422;
        splitContainer2.TabIndex = 0;
        // 
        // tabs
        // 
        tabs.Controls.Add(tbGeneral);
        tabs.Controls.Add(tbSigServerNetwork);
        tabs.Controls.Add(tbPeerNetwork);
        tabs.Dock = DockStyle.Fill;
        tabs.Location = new Point(0, 0);
        tabs.Name = "tabs";
        tabs.SelectedIndex = 0;
        tabs.Size = new Size(1085, 422);
        tabs.TabIndex = 0;
        // 
        // tbGeneral
        // 
        tbGeneral.Controls.Add(lbSendBuffersStatus);
        tbGeneral.Controls.Add(lbPacketsDownload);
        tbGeneral.Controls.Add(lbPacketsUpload);
        tbGeneral.Controls.Add(lbBytesDownload);
        tbGeneral.Controls.Add(label8);
        tbGeneral.Controls.Add(lbBytesUpload);
        tbGeneral.Controls.Add(label6);
        tbGeneral.Controls.Add(lbLocalEndPoint);
        tbGeneral.Controls.Add(label5);
        tbGeneral.Controls.Add(lbTotConnectedPeers);
        tbGeneral.Controls.Add(lbTotConnectedSigServers);
        tbGeneral.Controls.Add(label4);
        tbGeneral.Controls.Add(label3);
        tbGeneral.Controls.Add(lbStatus);
        tbGeneral.Controls.Add(label2);
        tbGeneral.Controls.Add(lbNPub);
        tbGeneral.Controls.Add(label1);
        tbGeneral.Location = new Point(4, 24);
        tbGeneral.Name = "tbGeneral";
        tbGeneral.Padding = new Padding(3);
        tbGeneral.Size = new Size(1077, 394);
        tbGeneral.TabIndex = 0;
        tbGeneral.Text = "General";
        tbGeneral.UseVisualStyleBackColor = true;
        // 
        // lbSendBuffersStatus
        // 
        lbSendBuffersStatus.AutoSize = true;
        lbSendBuffersStatus.Location = new Point(63, 250);
        lbSendBuffersStatus.Name = "lbSendBuffersStatus";
        lbSendBuffersStatus.Size = new Size(99, 15);
        lbSendBuffersStatus.TabIndex = 16;
        lbSendBuffersStatus.Text = "send buffer at 0%";
        // 
        // lbPacketsDownload
        // 
        lbPacketsDownload.AutoSize = true;
        lbPacketsDownload.Location = new Point(80, 292);
        lbPacketsDownload.Name = "lbPacketsDownload";
        lbPacketsDownload.Size = new Size(13, 15);
        lbPacketsDownload.TabIndex = 15;
        lbPacketsDownload.Text = "0";
        // 
        // lbPacketsUpload
        // 
        lbPacketsUpload.AutoSize = true;
        lbPacketsUpload.Location = new Point(63, 235);
        lbPacketsUpload.Name = "lbPacketsUpload";
        lbPacketsUpload.Size = new Size(13, 15);
        lbPacketsUpload.TabIndex = 14;
        lbPacketsUpload.Text = "0";
        // 
        // lbBytesDownload
        // 
        lbBytesDownload.AutoSize = true;
        lbBytesDownload.Location = new Point(80, 277);
        lbBytesDownload.Name = "lbBytesDownload";
        lbBytesDownload.Size = new Size(13, 15);
        lbBytesDownload.TabIndex = 13;
        lbBytesDownload.Text = "0";
        // 
        // label8
        // 
        label8.AutoSize = true;
        label8.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        label8.Location = new Point(8, 277);
        label8.Name = "label8";
        label8.Size = new Size(66, 15);
        label8.TabIndex = 12;
        label8.Text = "Download:";
        // 
        // lbBytesUpload
        // 
        lbBytesUpload.AutoSize = true;
        lbBytesUpload.Location = new Point(63, 220);
        lbBytesUpload.Name = "lbBytesUpload";
        lbBytesUpload.Size = new Size(13, 15);
        lbBytesUpload.TabIndex = 11;
        lbBytesUpload.Text = "0";
        // 
        // label6
        // 
        label6.AutoSize = true;
        label6.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        label6.Location = new Point(8, 220);
        label6.Name = "label6";
        label6.Size = new Size(49, 15);
        label6.TabIndex = 10;
        label6.Text = "Upload:";
        // 
        // lbLocalEndPoint
        // 
        lbLocalEndPoint.AutoSize = true;
        lbLocalEndPoint.Location = new Point(104, 54);
        lbLocalEndPoint.Name = "lbLocalEndPoint";
        lbLocalEndPoint.Size = new Size(0, 15);
        lbLocalEndPoint.TabIndex = 9;
        // 
        // label5
        // 
        label5.AutoSize = true;
        label5.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        label5.Location = new Point(8, 54);
        label5.Name = "label5";
        label5.Size = new Size(90, 15);
        label5.TabIndex = 8;
        label5.Text = "Local Endpoint:";
        // 
        // lbTotConnectedPeers
        // 
        lbTotConnectedPeers.AutoSize = true;
        lbTotConnectedPeers.Location = new Point(118, 147);
        lbTotConnectedPeers.Name = "lbTotConnectedPeers";
        lbTotConnectedPeers.Size = new Size(13, 15);
        lbTotConnectedPeers.TabIndex = 7;
        lbTotConnectedPeers.Text = "0";
        // 
        // lbTotConnectedSigServers
        // 
        lbTotConnectedSigServers.AutoSize = true;
        lbTotConnectedSigServers.Location = new Point(183, 115);
        lbTotConnectedSigServers.Name = "lbTotConnectedSigServers";
        lbTotConnectedSigServers.Size = new Size(13, 15);
        lbTotConnectedSigServers.TabIndex = 6;
        lbTotConnectedSigServers.Text = "0";
        // 
        // label4
        // 
        label4.AutoSize = true;
        label4.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        label4.Location = new Point(8, 147);
        label4.Name = "label4";
        label4.Size = new Size(104, 15);
        label4.TabIndex = 5;
        label4.Text = "Connected Peers:";
        // 
        // label3
        // 
        label3.AutoSize = true;
        label3.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        label3.Location = new Point(8, 115);
        label3.Name = "label3";
        label3.Size = new Size(169, 15);
        label3.TabIndex = 4;
        label3.Text = "Connected Signaling Servers:";
        // 
        // lbStatus
        // 
        lbStatus.AutoSize = true;
        lbStatus.Location = new Point(57, 23);
        lbStatus.Name = "lbStatus";
        lbStatus.Size = new Size(0, 15);
        lbStatus.TabIndex = 3;
        // 
        // label2
        // 
        label2.AutoSize = true;
        label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        label2.Location = new Point(8, 23);
        label2.Name = "label2";
        label2.Size = new Size(45, 15);
        label2.TabIndex = 2;
        label2.Text = "Status:";
        // 
        // lbNPub
        // 
        lbNPub.AutoSize = true;
        lbNPub.Location = new Point(148, 84);
        lbNPub.Name = "lbNPub";
        lbNPub.Size = new Size(0, 15);
        lbNPub.TabIndex = 1;
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        label1.Location = new Point(8, 84);
        label1.Name = "label1";
        label1.Size = new Size(136, 15);
        label1.TabIndex = 0;
        label1.Text = "Current Public Identity:";
        // 
        // tbSigServerNetwork
        // 
        tbSigServerNetwork.Controls.Add(splitContainer3);
        tbSigServerNetwork.Location = new Point(4, 24);
        tbSigServerNetwork.Name = "tbSigServerNetwork";
        tbSigServerNetwork.Padding = new Padding(3);
        tbSigServerNetwork.Size = new Size(1077, 394);
        tbSigServerNetwork.TabIndex = 1;
        tbSigServerNetwork.Text = "SigServer Network";
        tbSigServerNetwork.UseVisualStyleBackColor = true;
        // 
        // splitContainer3
        // 
        splitContainer3.Dock = DockStyle.Fill;
        splitContainer3.FixedPanel = FixedPanel.Panel1;
        splitContainer3.Location = new Point(3, 3);
        splitContainer3.Name = "splitContainer3";
        splitContainer3.Orientation = Orientation.Horizontal;
        // 
        // splitContainer3.Panel1
        // 
        splitContainer3.Panel1.Controls.Add(btnAddSignalingServer);
        // 
        // splitContainer3.Panel2
        // 
        splitContainer3.Panel2.Controls.Add(dgvSignalingServers);
        splitContainer3.Size = new Size(1071, 388);
        splitContainer3.SplitterDistance = 31;
        splitContainer3.TabIndex = 1;
        // 
        // btnAddSignalingServer
        // 
        btnAddSignalingServer.BackColor = Color.Bisque;
        btnAddSignalingServer.Dock = DockStyle.Fill;
        btnAddSignalingServer.FlatStyle = FlatStyle.Popup;
        btnAddSignalingServer.Location = new Point(0, 0);
        btnAddSignalingServer.Name = "btnAddSignalingServer";
        btnAddSignalingServer.Size = new Size(1071, 31);
        btnAddSignalingServer.TabIndex = 0;
        btnAddSignalingServer.Text = "Add Signaling Server";
        btnAddSignalingServer.UseVisualStyleBackColor = false;
        btnAddSignalingServer.Click += btnAddSignalingServer_Click;
        // 
        // dgvSignalingServers
        // 
        dgvSignalingServers.AllowUserToAddRows = false;
        dgvSignalingServers.AllowUserToDeleteRows = false;
        dgvSignalingServers.AutoGenerateColumns = false;
        dgvSignalingServers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvSignalingServers.Columns.AddRange(new DataGridViewColumn[] { iPV4AddressDataGridViewTextBoxColumn, isConnectedDataGridViewCheckBoxColumn, CurrentUploadSpeed, PacketsUploadPerSecond, CurrentDownloadSpeed, dgvcPacketsDownloadPerSecond, maxUploadSpeedDataGridViewTextBoxColumn, dgvcSafeBandWidth, mTUSizeDataGridViewTextBoxColumn, nPubBech32DataGridViewTextBoxColumn, Delete });
        dgvSignalingServers.DataSource = bsSignalingServers;
        dgvSignalingServers.Dock = DockStyle.Fill;
        dgvSignalingServers.Location = new Point(0, 0);
        dgvSignalingServers.MultiSelect = false;
        dgvSignalingServers.Name = "dgvSignalingServers";
        dgvSignalingServers.RowHeadersVisible = false;
        dgvSignalingServers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvSignalingServers.Size = new Size(1071, 353);
        dgvSignalingServers.TabIndex = 0;
        dgvSignalingServers.CellContentClick += dgvSignalingServers_CellContentClick;
        // 
        // iPV4AddressDataGridViewTextBoxColumn
        // 
        iPV4AddressDataGridViewTextBoxColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        iPV4AddressDataGridViewTextBoxColumn.DataPropertyName = "IPV4Address";
        iPV4AddressDataGridViewTextBoxColumn.HeaderText = "EndPoint";
        iPV4AddressDataGridViewTextBoxColumn.Name = "iPV4AddressDataGridViewTextBoxColumn";
        iPV4AddressDataGridViewTextBoxColumn.ReadOnly = true;
        iPV4AddressDataGridViewTextBoxColumn.Width = 80;
        // 
        // isConnectedDataGridViewCheckBoxColumn
        // 
        isConnectedDataGridViewCheckBoxColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        isConnectedDataGridViewCheckBoxColumn.DataPropertyName = "IsConnected";
        isConnectedDataGridViewCheckBoxColumn.HeaderText = "Connected";
        isConnectedDataGridViewCheckBoxColumn.Name = "isConnectedDataGridViewCheckBoxColumn";
        isConnectedDataGridViewCheckBoxColumn.ReadOnly = true;
        isConnectedDataGridViewCheckBoxColumn.Width = 71;
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
        // dgvcPacketsDownloadPerSecond
        // 
        dgvcPacketsDownloadPerSecond.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        dgvcPacketsDownloadPerSecond.DataPropertyName = "PacketsDownloadPerSecond";
        dgvcPacketsDownloadPerSecond.HeaderText = "Packets Down";
        dgvcPacketsDownloadPerSecond.Name = "dgvcPacketsDownloadPerSecond";
        dgvcPacketsDownloadPerSecond.ReadOnly = true;
        dgvcPacketsDownloadPerSecond.Width = 97;
        // 
        // maxUploadSpeedDataGridViewTextBoxColumn
        // 
        maxUploadSpeedDataGridViewTextBoxColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        maxUploadSpeedDataGridViewTextBoxColumn.DataPropertyName = "MaxUploadSpeed";
        maxUploadSpeedDataGridViewTextBoxColumn.HeaderText = "Max Up Speed";
        maxUploadSpeedDataGridViewTextBoxColumn.Name = "maxUploadSpeedDataGridViewTextBoxColumn";
        maxUploadSpeedDataGridViewTextBoxColumn.ReadOnly = true;
        maxUploadSpeedDataGridViewTextBoxColumn.Width = 99;
        // 
        // dgvcSafeBandWidth
        // 
        dgvcSafeBandWidth.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        dgvcSafeBandWidth.DataPropertyName = "SafeBandWidth";
        dgvcSafeBandWidth.HeaderText = "Safe Band Width";
        dgvcSafeBandWidth.Name = "dgvcSafeBandWidth";
        dgvcSafeBandWidth.ReadOnly = true;
        dgvcSafeBandWidth.Width = 109;
        // 
        // mTUSizeDataGridViewTextBoxColumn
        // 
        mTUSizeDataGridViewTextBoxColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        mTUSizeDataGridViewTextBoxColumn.DataPropertyName = "MTUSize";
        mTUSizeDataGridViewTextBoxColumn.HeaderText = "MTU";
        mTUSizeDataGridViewTextBoxColumn.Name = "mTUSizeDataGridViewTextBoxColumn";
        mTUSizeDataGridViewTextBoxColumn.ReadOnly = true;
        mTUSizeDataGridViewTextBoxColumn.Width = 57;
        // 
        // nPubBech32DataGridViewTextBoxColumn
        // 
        nPubBech32DataGridViewTextBoxColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
        nPubBech32DataGridViewTextBoxColumn.DataPropertyName = "NPubBech32";
        nPubBech32DataGridViewTextBoxColumn.HeaderText = "Identity";
        nPubBech32DataGridViewTextBoxColumn.Name = "nPubBech32DataGridViewTextBoxColumn";
        nPubBech32DataGridViewTextBoxColumn.ReadOnly = true;
        nPubBech32DataGridViewTextBoxColumn.Width = 72;
        // 
        // Delete
        // 
        Delete.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        Delete.DataPropertyName = "CurrentUploadSpeed";
        dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dataGridViewCellStyle1.BackColor = Color.Moccasin;
        Delete.DefaultCellStyle = dataGridViewCellStyle1;
        Delete.FlatStyle = FlatStyle.Popup;
        Delete.HeaderText = "Delete";
        Delete.Name = "Delete";
        Delete.ReadOnly = true;
        Delete.Width = 46;
        // 
        // bsSignalingServers
        // 
        bsSignalingServers.DataSource = typeof(Models.EPDetailsInfo);
        // 
        // tbPeerNetwork
        // 
        tbPeerNetwork.Controls.Add(dgvPeers);
        tbPeerNetwork.Location = new Point(4, 24);
        tbPeerNetwork.Name = "tbPeerNetwork";
        tbPeerNetwork.Size = new Size(1077, 394);
        tbPeerNetwork.TabIndex = 2;
        tbPeerNetwork.Text = "Peer Network";
        tbPeerNetwork.UseVisualStyleBackColor = true;
        // 
        // dgvPeers
        // 
        dgvPeers.AllowUserToAddRows = false;
        dgvPeers.AllowUserToDeleteRows = false;
        dgvPeers.AutoGenerateColumns = false;
        dgvPeers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvPeers.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn1, dataGridViewCheckBoxColumn1, dataGridViewTextBoxColumn2, dataGridViewTextBoxColumn3, dataGridViewTextBoxColumn4, dataGridViewTextBoxColumn5, dataGridViewTextBoxColumn6, dataGridViewTextBoxColumn7, dataGridViewTextBoxColumn8, dataGridViewTextBoxColumn9 });
        dgvPeers.DataSource = bsPeers;
        dgvPeers.Dock = DockStyle.Fill;
        dgvPeers.Location = new Point(0, 0);
        dgvPeers.MultiSelect = false;
        dgvPeers.Name = "dgvPeers";
        dgvPeers.RowHeadersVisible = false;
        dgvPeers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvPeers.Size = new Size(1077, 394);
        dgvPeers.TabIndex = 1;
        // 
        // dataGridViewTextBoxColumn1
        // 
        dataGridViewTextBoxColumn1.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        dataGridViewTextBoxColumn1.DataPropertyName = "IPV4Address";
        dataGridViewTextBoxColumn1.HeaderText = "EndPoint";
        dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
        dataGridViewTextBoxColumn1.ReadOnly = true;
        dataGridViewTextBoxColumn1.Width = 80;
        // 
        // dataGridViewCheckBoxColumn1
        // 
        dataGridViewCheckBoxColumn1.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        dataGridViewCheckBoxColumn1.DataPropertyName = "IsConnected";
        dataGridViewCheckBoxColumn1.HeaderText = "Connected";
        dataGridViewCheckBoxColumn1.Name = "dataGridViewCheckBoxColumn1";
        dataGridViewCheckBoxColumn1.ReadOnly = true;
        dataGridViewCheckBoxColumn1.Width = 71;
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
        // dataGridViewTextBoxColumn5
        // 
        dataGridViewTextBoxColumn5.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        dataGridViewTextBoxColumn5.DataPropertyName = "PacketsDownloadPerSecond";
        dataGridViewTextBoxColumn5.HeaderText = "Packets Down";
        dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
        dataGridViewTextBoxColumn5.ReadOnly = true;
        dataGridViewTextBoxColumn5.Width = 97;
        // 
        // dataGridViewTextBoxColumn6
        // 
        dataGridViewTextBoxColumn6.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        dataGridViewTextBoxColumn6.DataPropertyName = "MaxUploadSpeed";
        dataGridViewTextBoxColumn6.HeaderText = "Max Up Speed";
        dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
        dataGridViewTextBoxColumn6.ReadOnly = true;
        dataGridViewTextBoxColumn6.Width = 99;
        // 
        // dataGridViewTextBoxColumn7
        // 
        dataGridViewTextBoxColumn7.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        dataGridViewTextBoxColumn7.DataPropertyName = "SafeBandWidth";
        dataGridViewTextBoxColumn7.HeaderText = "Safe Band Width";
        dataGridViewTextBoxColumn7.Name = "dataGridViewTextBoxColumn7";
        dataGridViewTextBoxColumn7.ReadOnly = true;
        dataGridViewTextBoxColumn7.Width = 109;
        // 
        // dataGridViewTextBoxColumn8
        // 
        dataGridViewTextBoxColumn8.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        dataGridViewTextBoxColumn8.DataPropertyName = "MTUSize";
        dataGridViewTextBoxColumn8.HeaderText = "MTU";
        dataGridViewTextBoxColumn8.Name = "dataGridViewTextBoxColumn8";
        dataGridViewTextBoxColumn8.ReadOnly = true;
        dataGridViewTextBoxColumn8.Width = 57;
        // 
        // dataGridViewTextBoxColumn9
        // 
        dataGridViewTextBoxColumn9.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
        dataGridViewTextBoxColumn9.DataPropertyName = "NPubBech32";
        dataGridViewTextBoxColumn9.HeaderText = "Identity";
        dataGridViewTextBoxColumn9.Name = "dataGridViewTextBoxColumn9";
        dataGridViewTextBoxColumn9.ReadOnly = true;
        dataGridViewTextBoxColumn9.Width = 72;
        // 
        // bsPeers
        // 
        bsPeers.DataSource = typeof(Models.EPDetailsInfo);
        // 
        // lbMessage
        // 
        lbMessage.AutoSize = true;
        lbMessage.Dock = DockStyle.Fill;
        lbMessage.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lbMessage.Location = new Point(0, 0);
        lbMessage.Name = "lbMessage";
        lbMessage.Size = new Size(0, 21);
        lbMessage.TabIndex = 0;
        lbMessage.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // btnToggleServer
        // 
        btnToggleServer.BackColor = Color.LightGreen;
        btnToggleServer.Dock = DockStyle.Fill;
        btnToggleServer.FlatStyle = FlatStyle.Popup;
        btnToggleServer.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        btnToggleServer.Location = new Point(0, 0);
        btnToggleServer.Name = "btnToggleServer";
        btnToggleServer.Size = new Size(1085, 38);
        btnToggleServer.TabIndex = 5;
        btnToggleServer.Text = "Start";
        btnToggleServer.UseVisualStyleBackColor = false;
        btnToggleServer.Click += btnToggleServer_Click;
        // 
        // menu
        // 
        menu.Items.AddRange(new ToolStripItem[] { keysManagerToolStripMenuItem, serverParametersToolStripMenuItem });
        menu.Location = new Point(0, 0);
        menu.Name = "menu";
        menu.Size = new Size(1085, 24);
        menu.TabIndex = 1;
        menu.Text = "menuStrip1";
        // 
        // keysManagerToolStripMenuItem
        // 
        keysManagerToolStripMenuItem.Name = "keysManagerToolStripMenuItem";
        keysManagerToolStripMenuItem.Size = new Size(93, 20);
        keysManagerToolStripMenuItem.Text = "Keys Manager";
        keysManagerToolStripMenuItem.Click += keysManagerToolStripMenuItem_Click;
        // 
        // serverParametersToolStripMenuItem
        // 
        serverParametersToolStripMenuItem.Name = "serverParametersToolStripMenuItem";
        serverParametersToolStripMenuItem.Size = new Size(113, 20);
        serverParametersToolStripMenuItem.Text = "Server Parameters";
        serverParametersToolStripMenuItem.Click += serverParametersToolStripMenuItem_Click;
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1085, 519);
        Controls.Add(splitContainer1);
        Controls.Add(menu);
        MainMenuStrip = menu;
        Name = "Form1";
        Text = "Signaling Server";
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
        tbGeneral.ResumeLayout(false);
        tbGeneral.PerformLayout();
        tbSigServerNetwork.ResumeLayout(false);
        splitContainer3.Panel1.ResumeLayout(false);
        splitContainer3.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer3).EndInit();
        splitContainer3.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvSignalingServers).EndInit();
        ((System.ComponentModel.ISupportInitialize)bsSignalingServers).EndInit();
        tbPeerNetwork.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvPeers).EndInit();
        ((System.ComponentModel.ISupportInitialize)bsPeers).EndInit();
        menu.ResumeLayout(false);
        menu.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private SplitContainer splitContainer1;
    private Button btnToggleServer;
    private MenuStrip menu;
    private SplitContainer splitContainer2;
    private Label lbMessage;
    private TabControl tabs;
    private TabPage tbGeneral;
    private TabPage tbSigServerNetwork;
    private TabPage tbPeerNetwork;
    private Label lbNPub;
    private Label label1;
    private Label lbStatus;
    private Label label2;
    private Label label3;
    private Label label4;
    private Label lbTotConnectedPeers;
    private Label lbTotConnectedSigServers;
    private Label lbLocalEndPoint;
    private Label label5;
    private Label lbBytesDownload;
    private Label label8;
    private Label lbBytesUpload;
    private Label label6;
    private Label lbPacketsDownload;
    private Label lbPacketsUpload;
    private Label lbSendBuffersStatus;
    private DataGridViewCheckBoxColumn trustedDataGridViewCheckBoxColumn;
    private DataGridViewCheckBoxColumn connectedDataGridViewCheckBoxColumn;
    private DataGridViewTextBoxColumn sentBytesPerSecondDataGridViewTextBoxColumn;
    private DataGridViewTextBoxColumn sentPacketsPerSecondDataGridViewTextBoxColumn;
    private DataGridViewTextBoxColumn receivedBytesPerSecondDataGridViewTextBoxColumn;
    private DataGridViewTextBoxColumn receivedPacketsPerSecondDataGridViewTextBoxColumn;
    private DataGridViewTextBoxColumn iPV4AddressDataGridViewTextBoxColumn1;
    private DataGridViewCheckBoxColumn trustedDataGridViewCheckBoxColumn1;
    private DataGridViewCheckBoxColumn connectedDataGridViewCheckBoxColumn1;
    private DataGridViewCheckBoxColumn disconnectingDataGridViewCheckBoxColumn1;
    private DataGridViewTextBoxColumn maxUploadSpeedDataGridViewTextBoxColumn1;
    private DataGridViewTextBoxColumn safeBandWidthDataGridViewTextBoxColumn1;
    private DataGridViewTextBoxColumn sentBytesPerSecondDataGridViewTextBoxColumn1;
    private DataGridViewTextBoxColumn sentPacketsPerSecondDataGridViewTextBoxColumn1;
    private DataGridViewTextBoxColumn receivedBytesPerSecondDataGridViewTextBoxColumn1;
    private DataGridViewTextBoxColumn receivedPacketsPerSecondDataGridViewTextBoxColumn1;
    private DataGridViewTextBoxColumn mTUSizeDataGridViewTextBoxColumn1;
    private DataGridViewTextBoxColumn nPubBech32DataGridViewTextBoxColumn1;
    private SplitContainer splitContainer3;
    private Button btnAddSignalingServer;
    private BindingSource bsSignalingServers;
    private DataGridView dgvSignalingServers;
    private BindingSource bsPeers;
    private DataGridView dgvPeers;
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
    private DataGridViewButtonColumn Delete;
    private ToolStripMenuItem keysManagerToolStripMenuItem;
    private ToolStripMenuItem serverParametersToolStripMenuItem;
}
