namespace client
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.ip = new System.Windows.Forms.TextBox();
            this.port = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.username = new System.Windows.Forms.TextBox();
            this.messageBox = new System.Windows.Forms.RichTextBox();
            this.connect = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.answerBox = new System.Windows.Forms.TextBox();
            this.send = new System.Windows.Forms.Button();
            this.disconnect = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(84, 61);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 32);
            this.label1.TabIndex = 0;
            this.label1.Text = "IP:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(84, 131);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 32);
            this.label2.TabIndex = 1;
            this.label2.Text = "Port:";
            // 
            // ip
            // 
            this.ip.Location = new System.Drawing.Point(226, 61);
            this.ip.Name = "ip";
            this.ip.Size = new System.Drawing.Size(145, 39);
            this.ip.TabIndex = 2;
            // 
            // port
            // 
            this.port.Location = new System.Drawing.Point(226, 124);
            this.port.Name = "port";
            this.port.Size = new System.Drawing.Size(145, 39);
            this.port.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(84, 197);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(126, 32);
            this.label3.TabIndex = 4;
            this.label3.Text = "Username:";
            // 
            // username
            // 
            this.username.Location = new System.Drawing.Point(226, 191);
            this.username.Name = "username";
            this.username.Size = new System.Drawing.Size(145, 39);
            this.username.TabIndex = 5;
            // 
            // messageBox
            // 
            this.messageBox.Enabled = false;
            this.messageBox.Location = new System.Drawing.Point(446, 47);
            this.messageBox.Name = "messageBox";
            this.messageBox.Size = new System.Drawing.Size(471, 711);
            this.messageBox.TabIndex = 6;
            this.messageBox.Text = "";
            // 
            // connect
            // 
            this.connect.Location = new System.Drawing.Point(70, 268);
            this.connect.Name = "connect";
            this.connect.Size = new System.Drawing.Size(161, 39);
            this.connect.TabIndex = 7;
            this.connect.Text = "Connect";
            this.connect.UseVisualStyleBackColor = true;
            this.connect.Click += new System.EventHandler(this.connect_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(32, 578);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(106, 32);
            this.label4.TabIndex = 8;
            this.label4.Text = "Answers:";
            // 
            // answerBox
            // 
            this.answerBox.Enabled = false;
            this.answerBox.Location = new System.Drawing.Point(151, 575);
            this.answerBox.Name = "answerBox";
            this.answerBox.Size = new System.Drawing.Size(248, 39);
            this.answerBox.TabIndex = 9;
            // 
            // send
            // 
            this.send.Enabled = false;
            this.send.Location = new System.Drawing.Point(132, 652);
            this.send.Name = "send";
            this.send.Size = new System.Drawing.Size(143, 41);
            this.send.TabIndex = 10;
            this.send.Text = "Send";
            this.send.UseVisualStyleBackColor = true;
            this.send.Click += new System.EventHandler(this.send_Click);
            // 
            // disconnect
            // 
            this.disconnect.Enabled = false;
            this.disconnect.Location = new System.Drawing.Point(259, 268);
            this.disconnect.Name = "disconnect";
            this.disconnect.Size = new System.Drawing.Size(156, 39);
            this.disconnect.TabIndex = 11;
            this.disconnect.Text = "Disconnect";
            this.disconnect.UseVisualStyleBackColor = true;
            this.disconnect.Click += new System.EventHandler(this.disconnect_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(939, 790);
            this.Controls.Add(this.disconnect);
            this.Controls.Add(this.send);
            this.Controls.Add(this.answerBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.connect);
            this.Controls.Add(this.messageBox);
            this.Controls.Add(this.username);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.port);
            this.Controls.Add(this.ip);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label label1;
        private Label label2;
        private TextBox ip;
        private TextBox port;
        private Label label3;
        private TextBox username;
        private RichTextBox messageBox;
        private Button connect;
        private Label label4;
        private TextBox answerBox;
        private Button send;
        private Button disconnect;
    }
}