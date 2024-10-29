namespace Client
{
    partial class NostrContacts
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
            components = new System.ComponentModel.Container();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            bsContacts = new BindingSource(components);
            dgvContacts = new DataGridView();
            dgvImage = new DataGridViewImageColumn();
            displayNameDataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
            nip05DataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            Connect = new DataGridViewButtonColumn();
            PubKey = new DataGridViewTextBoxColumn();
            splitContainer1 = new SplitContainer();
            btnFilter = new Button();
            tbFilter = new TextBox();
            label1 = new Label();
            btnUpdate = new Button();
            ((System.ComponentModel.ISupportInitialize)bsContacts).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvContacts).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // bsContacts
            // 
            bsContacts.DataSource = typeof(NostrSharp.Nostr.Models.UserMetadata);
            // 
            // dgvContacts
            // 
            dgvContacts.AllowUserToAddRows = false;
            dgvContacts.AllowUserToDeleteRows = false;
            dgvContacts.AutoGenerateColumns = false;
            dgvContacts.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvContacts.Columns.AddRange(new DataGridViewColumn[] { dgvImage, displayNameDataGridViewTextBoxColumn1, nip05DataGridViewTextBoxColumn, Connect, PubKey });
            dgvContacts.DataSource = bsContacts;
            dgvContacts.Dock = DockStyle.Fill;
            dgvContacts.Location = new Point(0, 0);
            dgvContacts.Name = "dgvContacts";
            dgvContacts.ReadOnly = true;
            dgvContacts.Size = new Size(857, 407);
            dgvContacts.TabIndex = 0;
            dgvContacts.CellContentClick += dgvContacts_CellContentClick;
            dgvContacts.CellFormatting += dgvContacts_CellFormatting;
            // 
            // dgvImage
            // 
            dgvImage.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            dgvImage.HeaderText = "Image";
            dgvImage.Name = "dgvImage";
            dgvImage.ReadOnly = true;
            dgvImage.Width = 46;
            // 
            // displayNameDataGridViewTextBoxColumn1
            // 
            displayNameDataGridViewTextBoxColumn1.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            displayNameDataGridViewTextBoxColumn1.DataPropertyName = "Display_Name";
            displayNameDataGridViewTextBoxColumn1.HeaderText = "Name";
            displayNameDataGridViewTextBoxColumn1.Name = "displayNameDataGridViewTextBoxColumn1";
            displayNameDataGridViewTextBoxColumn1.ReadOnly = true;
            displayNameDataGridViewTextBoxColumn1.Width = 64;
            // 
            // nip05DataGridViewTextBoxColumn
            // 
            nip05DataGridViewTextBoxColumn.DataPropertyName = "Nip05";
            nip05DataGridViewTextBoxColumn.HeaderText = "Nip05";
            nip05DataGridViewTextBoxColumn.Name = "nip05DataGridViewTextBoxColumn";
            nip05DataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // Connect
            // 
            Connect.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = Color.Moccasin;
            Connect.DefaultCellStyle = dataGridViewCellStyle2;
            Connect.HeaderText = "Try Connect";
            Connect.Name = "Connect";
            Connect.ReadOnly = true;
            Connect.Text = "Try Connect";
            Connect.Width = 76;
            // 
            // PubKey
            // 
            PubKey.DataPropertyName = "PubKey";
            PubKey.HeaderText = "PubKey";
            PubKey.Name = "PubKey";
            PubKey.ReadOnly = true;
            PubKey.Visible = false;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.FixedPanel = FixedPanel.Panel1;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(btnUpdate);
            splitContainer1.Panel1.Controls.Add(btnFilter);
            splitContainer1.Panel1.Controls.Add(tbFilter);
            splitContainer1.Panel1.Controls.Add(label1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(dgvContacts);
            splitContainer1.Size = new Size(857, 474);
            splitContainer1.SplitterDistance = 63;
            splitContainer1.TabIndex = 1;
            // 
            // btnFilter
            // 
            btnFilter.BackColor = SystemColors.ActiveCaption;
            btnFilter.FlatStyle = FlatStyle.Popup;
            btnFilter.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnFilter.Location = new Point(414, 9);
            btnFilter.Name = "btnFilter";
            btnFilter.Size = new Size(203, 41);
            btnFilter.TabIndex = 2;
            btnFilter.Text = "Filter";
            btnFilter.UseVisualStyleBackColor = false;
            btnFilter.Click += btnFilter_Click;
            // 
            // tbFilter
            // 
            tbFilter.Location = new Point(12, 27);
            tbFilter.Name = "tbFilter";
            tbFilter.Size = new Size(396, 23);
            tbFilter.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(237, 15);
            label1.TabIndex = 0;
            label1.Text = "Filter by Name, Nip05 or NPub (hex format)";
            // 
            // btnUpdate
            // 
            btnUpdate.Location = new Point(751, 27);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(103, 23);
            btnUpdate.TabIndex = 3;
            btnUpdate.Text = "Update";
            btnUpdate.UseVisualStyleBackColor = true;
            btnUpdate.Click += btnUpdate_Click;
            // 
            // NostrContacts
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(857, 474);
            Controls.Add(splitContainer1);
            Name = "NostrContacts";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Nostr Contacts";
            FormClosing += NostrContacts_FormClosing;
            Load += NostrContacts_Load;
            ((System.ComponentModel.ISupportInitialize)bsContacts).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvContacts).EndInit();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private BindingSource bsContacts;
        private DataGridView dgvContacts;
        private SplitContainer splitContainer1;
        private Button btnFilter;
        private TextBox tbFilter;
        private Label label1;
        private DataGridViewImageColumn dgvImage;
        private DataGridViewTextBoxColumn displayNameDataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn nip05DataGridViewTextBoxColumn;
        private DataGridViewButtonColumn Connect;
        private DataGridViewTextBoxColumn PubKey;
        private Button btnUpdate;
    }
}