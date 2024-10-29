namespace Client
{
    partial class KeysManagement
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
            tableLayoutPanel1 = new TableLayoutPanel();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            tbNSec = new TextBox();
            tbNPub = new TextBox();
            btnSave = new Button();
            btnGenerateNew = new Button();
            lbError = new Label();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(label2, 0, 1);
            tableLayoutPanel1.Controls.Add(label3, 0, 3);
            tableLayoutPanel1.Controls.Add(tbNSec, 0, 2);
            tableLayoutPanel1.Controls.Add(tbNPub, 0, 4);
            tableLayoutPanel1.Controls.Add(btnSave, 1, 7);
            tableLayoutPanel1.Controls.Add(btnGenerateNew, 0, 7);
            tableLayoutPanel1.Controls.Add(lbError, 0, 6);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 8;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 29F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 29F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel1.Size = new Size(800, 269);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(label1, 2);
            label1.Dock = DockStyle.Fill;
            label1.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Size = new Size(794, 50);
            label1.TabIndex = 0;
            label1.Text = "Insert the NSec of your choice (Bech32 or Hex) or generate a new identity";
            label1.TextAlign = ContentAlignment.TopCenter;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Dock = DockStyle.Fill;
            label2.Font = new Font("Segoe UI", 10F);
            label2.Location = new Point(3, 50);
            label2.Name = "label2";
            label2.Size = new Size(394, 30);
            label2.TabIndex = 1;
            label2.Text = "NSec";
            label2.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Dock = DockStyle.Fill;
            label3.Font = new Font("Segoe UI", 10F);
            label3.Location = new Point(3, 109);
            label3.Name = "label3";
            label3.Size = new Size(394, 30);
            label3.TabIndex = 2;
            label3.Text = "NPub";
            label3.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // tbNSec
            // 
            tableLayoutPanel1.SetColumnSpan(tbNSec, 2);
            tbNSec.Dock = DockStyle.Fill;
            tbNSec.Font = new Font("Segoe UI", 10F);
            tbNSec.Location = new Point(3, 83);
            tbNSec.Name = "tbNSec";
            tbNSec.Size = new Size(794, 25);
            tbNSec.TabIndex = 3;
            tbNSec.Leave += tbNSec_Leave;
            // 
            // tbNPub
            // 
            tableLayoutPanel1.SetColumnSpan(tbNPub, 2);
            tbNPub.Dock = DockStyle.Fill;
            tbNPub.Enabled = false;
            tbNPub.Font = new Font("Segoe UI", 10F);
            tbNPub.Location = new Point(3, 142);
            tbNPub.Name = "tbNPub";
            tbNPub.PlaceholderText = "corresponding npub will appear here...";
            tbNPub.ReadOnly = true;
            tbNPub.Size = new Size(794, 25);
            tbNPub.TabIndex = 4;
            // 
            // btnSave
            // 
            btnSave.BackColor = Color.LightGreen;
            btnSave.Dock = DockStyle.Fill;
            btnSave.FlatStyle = FlatStyle.Popup;
            btnSave.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSave.Location = new Point(403, 232);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(394, 34);
            btnSave.TabIndex = 6;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = false;
            btnSave.Click += btnSave_Click;
            // 
            // btnGenerateNew
            // 
            btnGenerateNew.BackColor = Color.PowderBlue;
            btnGenerateNew.Dock = DockStyle.Fill;
            btnGenerateNew.FlatStyle = FlatStyle.Popup;
            btnGenerateNew.Font = new Font("Segoe UI", 10F);
            btnGenerateNew.Location = new Point(3, 232);
            btnGenerateNew.Name = "btnGenerateNew";
            btnGenerateNew.Size = new Size(394, 34);
            btnGenerateNew.TabIndex = 5;
            btnGenerateNew.Text = "Create New identity";
            btnGenerateNew.UseVisualStyleBackColor = false;
            btnGenerateNew.Click += btnGenerateNew_Click;
            // 
            // lbError
            // 
            lbError.AutoSize = true;
            lbError.Dock = DockStyle.Fill;
            lbError.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lbError.ForeColor = Color.OrangeRed;
            lbError.Location = new Point(3, 188);
            lbError.Name = "lbError";
            lbError.Size = new Size(394, 41);
            lbError.TabIndex = 7;
            // 
            // KeysManagement
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 269);
            Controls.Add(tableLayoutPanel1);
            Name = "KeysManagement";
            StartPosition = FormStartPosition.CenterParent;
            Text = "KeysManagement";
            FormClosing += KeysManagement_FormClosing;
            Load += KeysManagement_Load;
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private Label label1;
        private Label label2;
        private Label label3;
        private TextBox tbNSec;
        private TextBox tbNPub;
        private Button btnSave;
        private Button btnGenerateNew;
        private Label lbError;
    }
}