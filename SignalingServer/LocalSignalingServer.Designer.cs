namespace SignalingServer
{
    partial class LocalSignalingServer
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
            splitContainer1.Size = new Size(633, 396);
            splitContainer1.SplitterDistance = 354;
            splitContainer1.TabIndex = 0;
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
            tabControl1.Size = new Size(633, 354);
            tabControl1.TabIndex = 3;
            // 
            // tbGenerals
            // 
            tbGenerals.Controls.Add(tbMaxUploadSpeed);
            tbGenerals.Controls.Add(label1);
            tbGenerals.Controls.Add(label2);
            tbGenerals.Controls.Add(tbPortNumber);
            tbGenerals.Location = new Point(4, 24);
            tbGenerals.Name = "tbGenerals";
            tbGenerals.Padding = new Padding(3);
            tbGenerals.Size = new Size(625, 326);
            tbGenerals.TabIndex = 0;
            tbGenerals.Text = "Generals";
            tbGenerals.UseVisualStyleBackColor = true;
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
            label1.Location = new Point(6, 14);
            label1.Name = "label1";
            label1.Size = new Size(110, 15);
            label1.TabIndex = 0;
            label1.Text = "Local Port Number:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(6, 51);
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
            tbWhitelist.Size = new Size(941, 421);
            tbWhitelist.TabIndex = 1;
            tbWhitelist.Text = "Whitelist";
            tbWhitelist.UseVisualStyleBackColor = true;
            // 
            // tbBlacklist
            // 
            tbBlacklist.Location = new Point(4, 24);
            tbBlacklist.Name = "tbBlacklist";
            tbBlacklist.Size = new Size(941, 421);
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
            btnSaveConfigs.Size = new Size(633, 38);
            btnSaveConfigs.TabIndex = 0;
            btnSaveConfigs.Text = "Save";
            btnSaveConfigs.UseVisualStyleBackColor = false;
            btnSaveConfigs.Click += btnSaveConfigs_Click;
            // 
            // LocalSignalingServer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(633, 396);
            Controls.Add(splitContainer1);
            Name = "LocalSignalingServer";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Local Signaling Server Configurations";
            Load += LocalSignalingServer_Load;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            tabControl1.ResumeLayout(false);
            tbGenerals.ResumeLayout(false);
            tbGenerals.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitContainer1;
        private Button btnSaveConfigs;
        private Label label1;
        private TextBox tbPortNumber;
        private TabControl tabControl1;
        private TabPage tbGenerals;
        private Label label2;
        private TabPage tbWhitelist;
        private TabPage tbBlacklist;
        private TextBox tbMaxUploadSpeed;
    }
}