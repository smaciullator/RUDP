namespace Client
{
    partial class LocalClientSettings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            splitContainer1 = new SplitContainer();
            tabControl1 = new TabControl();
            tbGenerals = new TabPage();
            groupBox1 = new GroupBox();
            tbSignalingServer3 = new TextBox();
            label5 = new Label();
            tbSignalingServer2 = new TextBox();
            label4 = new Label();
            tbSignalingServer1 = new TextBox();
            label3 = new Label();
            tbMaxUploadSpeed = new TextBox();
            label1 = new Label();
            label2 = new Label();
            tbPortNumber = new TextBox();
            tbWhitelist = new TabPage();
            tbBlacklist = new TabPage();
            btnSaveConfigs = new Button();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            tabControl1.SuspendLayout();
            tbGenerals.SuspendLayout();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.FixedPanel = FixedPanel.Panel2;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(tabControl1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(btnSaveConfigs);
            splitContainer1.Size = new Size(800, 412);
            splitContainer1.SplitterDistance = 370;
            splitContainer1.TabIndex = 1;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tbGenerals);
            tabControl1.Controls.Add(tbWhitelist);
            tabControl1.Controls.Add(tbBlacklist);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(800, 370);
            tabControl1.TabIndex = 3;
            // 
            // tbGenerals
            // 
            tbGenerals.Controls.Add(groupBox1);
            tbGenerals.Controls.Add(tbMaxUploadSpeed);
            tbGenerals.Controls.Add(label1);
            tbGenerals.Controls.Add(label2);
            tbGenerals.Controls.Add(tbPortNumber);
            tbGenerals.Location = new Point(4, 24);
            tbGenerals.Name = "tbGenerals";
            tbGenerals.Padding = new Padding(3);
            tbGenerals.Size = new Size(792, 342);
            tbGenerals.TabIndex = 0;
            tbGenerals.Text = "Generals";
            tbGenerals.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(tbSignalingServer3);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(tbSignalingServer2);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(tbSignalingServer1);
            groupBox1.Controls.Add(label3);
            groupBox1.Location = new Point(9, 102);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(296, 230);
            groupBox1.TabIndex = 4;
            groupBox1.TabStop = false;
            groupBox1.Text = "Signaling Servers";
            // 
            // tbSignalingServer3
            // 
            tbSignalingServer3.Location = new Point(6, 193);
            tbSignalingServer3.Name = "tbSignalingServer3";
            tbSignalingServer3.PlaceholderText = "signaling server 3 (optional)";
            tbSignalingServer3.Size = new Size(284, 23);
            tbSignalingServer3.TabIndex = 5;
            tbSignalingServer3.Leave += tbSignalingServer3_Leave;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 9F);
            label5.Location = new Point(6, 175);
            label5.Name = "label5";
            label5.Size = new Size(155, 15);
            label5.TabIndex = 4;
            label5.Text = "Signaling Server 3 (optional)";
            // 
            // tbSignalingServer2
            // 
            tbSignalingServer2.Location = new Point(6, 139);
            tbSignalingServer2.Name = "tbSignalingServer2";
            tbSignalingServer2.PlaceholderText = "signaling server 2 (optional)";
            tbSignalingServer2.Size = new Size(284, 23);
            tbSignalingServer2.TabIndex = 3;
            tbSignalingServer2.Leave += tbSignalingServer2_Leave;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 9F);
            label4.Location = new Point(6, 121);
            label4.Name = "label4";
            label4.Size = new Size(155, 15);
            label4.TabIndex = 2;
            label4.Text = "Signaling Server 2 (optional)";
            // 
            // tbSignalingServer1
            // 
            tbSignalingServer1.Location = new Point(6, 61);
            tbSignalingServer1.Name = "tbSignalingServer1";
            tbSignalingServer1.PlaceholderText = "signaling server 1 (mandatory)";
            tbSignalingServer1.Size = new Size(284, 23);
            tbSignalingServer1.TabIndex = 1;
            tbSignalingServer1.Leave += tbSignalingServer1_Leave;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label3.Location = new Point(6, 34);
            label3.Name = "label3";
            label3.Size = new Size(113, 15);
            label3.TabIndex = 0;
            label3.Text = "*Signaling Server 1";
            // 
            // tbMaxUploadSpeed
            // 
            tbMaxUploadSpeed.Location = new Point(224, 48);
            tbMaxUploadSpeed.Name = "tbMaxUploadSpeed";
            tbMaxUploadSpeed.PlaceholderText = "leave empty or 0 for 'no limit'...";
            tbMaxUploadSpeed.Size = new Size(223, 23);
            tbMaxUploadSpeed.TabIndex = 3;
            tbMaxUploadSpeed.Leave += tbMaxUploadSpeed_Leave;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(9, 17);
            label1.Name = "label1";
            label1.Size = new Size(110, 15);
            label1.TabIndex = 0;
            label1.Text = "Local Port Number:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(9, 54);
            label2.Name = "label2";
            label2.Size = new Size(212, 15);
            label2.TabIndex = 2;
            label2.Text = "Max Upload Speed (bytes per second): ";
            // 
            // tbPortNumber
            // 
            tbPortNumber.Location = new Point(122, 11);
            tbPortNumber.Name = "tbPortNumber";
            tbPortNumber.PlaceholderText = "port number here...";
            tbPortNumber.Size = new Size(122, 23);
            tbPortNumber.TabIndex = 1;
            tbPortNumber.Leave += tbPortNumber_Leave;
            // 
            // tbWhitelist
            // 
            tbWhitelist.Location = new Point(4, 24);
            tbWhitelist.Name = "tbWhitelist";
            tbWhitelist.Padding = new Padding(3);
            tbWhitelist.Size = new Size(792, 342);
            tbWhitelist.TabIndex = 1;
            tbWhitelist.Text = "Whitelist";
            tbWhitelist.UseVisualStyleBackColor = true;
            // 
            // tbBlacklist
            // 
            tbBlacklist.Location = new Point(4, 24);
            tbBlacklist.Name = "tbBlacklist";
            tbBlacklist.Size = new Size(792, 342);
            tbBlacklist.TabIndex = 2;
            tbBlacklist.Text = "Blacklist";
            tbBlacklist.UseVisualStyleBackColor = true;
            // 
            // btnSaveConfigs
            // 
            btnSaveConfigs.BackColor = Color.LightGreen;
            btnSaveConfigs.Dock = DockStyle.Fill;
            btnSaveConfigs.FlatStyle = FlatStyle.Popup;
            btnSaveConfigs.Location = new Point(0, 0);
            btnSaveConfigs.Name = "btnSaveConfigs";
            btnSaveConfigs.Size = new Size(800, 38);
            btnSaveConfigs.TabIndex = 0;
            btnSaveConfigs.Text = "Save";
            btnSaveConfigs.UseVisualStyleBackColor = false;
            btnSaveConfigs.Click += btnSaveConfigs_Click;
            // 
            // LocalClientSettings
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 412);
            Controls.Add(splitContainer1);
            Name = "LocalClientSettings";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Local Client Settings";
            Load += LocalSignalingServer_Load;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            tabControl1.ResumeLayout(false);
            tbGenerals.ResumeLayout(false);
            tbGenerals.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitContainer1;
        private TabControl tabControl1;
        private TabPage tbGenerals;
        private TextBox tbMaxUploadSpeed;
        private Label label1;
        private Label label2;
        private TextBox tbPortNumber;
        private TabPage tbWhitelist;
        private TabPage tbBlacklist;
        private Button btnSaveConfigs;
        private GroupBox groupBox1;
        private TextBox tbSignalingServer3;
        private Label label5;
        private TextBox tbSignalingServer2;
        private Label label4;
        private TextBox tbSignalingServer1;
        private Label label3;
    }
}