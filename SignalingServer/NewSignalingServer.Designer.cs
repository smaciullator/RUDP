namespace SignalingServer
{
    partial class NewSignalingServer
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
            label1 = new Label();
            tbEndPoint = new TextBox();
            btnConnect = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(351, 15);
            label1.TabIndex = 0;
            label1.Text = "Input the EndPoint of the Signaling Server you wish to connect to";
            // 
            // tbEndPoint
            // 
            tbEndPoint.Location = new Point(12, 27);
            tbEndPoint.Name = "tbEndPoint";
            tbEndPoint.PlaceholderText = "endpoint here (ip:port)";
            tbEndPoint.Size = new Size(322, 23);
            tbEndPoint.TabIndex = 1;
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(340, 27);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(128, 23);
            btnConnect.TabIndex = 2;
            btnConnect.Text = "Connect";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // NewSignalingServer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(483, 57);
            Controls.Add(btnConnect);
            Controls.Add(tbEndPoint);
            Controls.Add(label1);
            Name = "NewSignalingServer";
            Text = "NewSignalingServer";
            FormClosing += NewSignalingServer_FormClosing;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox tbEndPoint;
        private Button btnConnect;
    }
}