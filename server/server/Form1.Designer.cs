namespace server
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
            this.port = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.questionNumber = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.messageServer = new System.Windows.Forms.RichTextBox();
            this.start = new System.Windows.Forms.Button();
            this.startGame = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // port
            // 
            this.port.Location = new System.Drawing.Point(295, 100);
            this.port.Name = "port";
            this.port.Size = new System.Drawing.Size(162, 39);
            this.port.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(78, 100);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(156, 32);
            this.label1.TabIndex = 1;
            this.label1.Text = "Port Number:";
            // 
            // questionNumber
            // 
            this.questionNumber.Location = new System.Drawing.Point(295, 198);
            this.questionNumber.Name = "questionNumber";
            this.questionNumber.Size = new System.Drawing.Size(162, 39);
            this.questionNumber.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(78, 201);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(211, 32);
            this.label2.TabIndex = 3;
            this.label2.Text = "Question Number:";
            // 
            // messageServer
            // 
            this.messageServer.Location = new System.Drawing.Point(521, 42);
            this.messageServer.Name = "messageServer";
            this.messageServer.Size = new System.Drawing.Size(455, 544);
            this.messageServer.TabIndex = 4;
            this.messageServer.Text = "";
            // 
            // start
            // 
            this.start.Location = new System.Drawing.Point(76, 305);
            this.start.Name = "start";
            this.start.Size = new System.Drawing.Size(158, 45);
            this.start.TabIndex = 5;
            this.start.Text = "Start Server";
            this.start.UseVisualStyleBackColor = true;
            this.start.Click += new System.EventHandler(this.start_Click);
            // 
            // startGame
            // 
            this.startGame.Enabled = false;
            this.startGame.Location = new System.Drawing.Point(282, 305);
            this.startGame.Name = "startGame";
            this.startGame.Size = new System.Drawing.Size(175, 45);
            this.startGame.TabIndex = 6;
            this.startGame.Text = "Start Game";
            this.startGame.UseVisualStyleBackColor = true;
            this.startGame.Click += new System.EventHandler(this.startGame_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1010, 621);
            this.Controls.Add(this.startGame);
            this.Controls.Add(this.start);
            this.Controls.Add(this.messageServer);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.questionNumber);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.port);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TextBox port;
        private Label label1;
        private TextBox questionNumber;
        private Label label2;
        private RichTextBox messageServer;
        private Button start;
        private Button startGame;
    }
}