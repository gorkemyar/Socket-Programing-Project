using Microsoft.VisualBasic.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace client
{
    public partial class Form1 : Form
    {
        bool terminating = false;
        bool connected = false;
        Socket clientSocket;
        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();
        }

        private void connect_Click(object sender, EventArgs e)
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            string IP = ip.Text;

            int portNum;
            if (Int32.TryParse(port.Text, out portNum))
            {
                try
                {
                    clientSocket.Connect(IP, portNum);
                    connect.Enabled = false;
                    messageBox.Enabled = true;
                    connected = true;
                    messageBox.AppendText("Connected to the server!\n");


                    string sendingMessage = username.Text;
                    Byte[] buffer = Encoding.Default.GetBytes(sendingMessage);
                    //message.AppendText("Message Sent: " + sendingMessage + "\n");
                    clientSocket.Send(buffer);

                    Thread receiveThread = new Thread(Receive);
                    receiveThread.Start();

                }
                catch
                {
                    messageBox.AppendText("Could not connect to the server!\n");
                }
            }
            else
            {
                messageBox.AppendText("Check the port\n");
            }
        }
        private void Receive()
        {
            while (connected)
            {
                try
                {
                    Byte[] buffer = new Byte[1024];
                    clientSocket.Receive(buffer);

                    string incomingMessage = Encoding.Default.GetString(buffer);
                    incomingMessage = incomingMessage.Substring(0, incomingMessage.IndexOf('\0'));

                    if (incomingMessage.Contains("Final"))
                    {
                        messageBox.AppendText("The game is finished: " + incomingMessage + "\n");
                        connected = false;
                        terminating = true;
                    }
                    else if (incomingMessage.Length > 1)
                    {
                        messageBox.AppendText(incomingMessage + "\n");
                        answerBox.Enabled = true;
                        send.Enabled = true;
                    }
                }
                catch
                {
                    if (!terminating)
                    {
                        messageBox.AppendText("The server has disconnected\n");
                        connect.Enabled = true;
                    }

                    clientSocket.Close();
                    connected = false;
                }

            }
        }

        private void send_Click(object sender, EventArgs e)
        {
            send.Enabled = false;
            answerBox.Enabled = false;
            string sendingMessage = answerBox.Text;
            Byte[] buffer = Encoding.Default.GetBytes(sendingMessage);
            messageBox.AppendText("Answer Sent: " + sendingMessage + "\n");
            clientSocket.Send(buffer); 
        }
        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            connected = false;
            terminating = true;
            Environment.Exit(0);
        }
    }
}