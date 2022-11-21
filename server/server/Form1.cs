using Microsoft.VisualBasic.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace server
{
    public partial class Form1 : Form
    {
        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        List<Socket> clientSockets = new List<Socket>();
        Dictionary<string, double> map = new Dictionary<string, double>();
        bool terminating = false;
        bool listening = false;
        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();
        }
        

        private void start_Click(object sender, EventArgs e)
        {
            int serverPort;

            if (Int32.TryParse(port.Text, out serverPort))
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, serverPort);
                serverSocket.Bind(endPoint);
                serverSocket.Listen(3);

                listening = true;
                start.Enabled = false;
                messageServer.Enabled = true;

                Thread acceptThread = new Thread(Accept);
                acceptThread.Start();

                messageServer.AppendText("Started listening on port: " + serverPort + "\n");

            }
            else
            {
                messageServer.AppendText("Please check port number \n");
            }
        }

        private bool ValidUser(string name)
        {
            if (clientSockets.Count >= 2 || map.ContainsKey(name))
            {
                return false;
            }
            return true;
        }
        private void Accept()
        {
            while (listening)
            {
                try
                {
                    Socket newClient = serverSocket.Accept();
                    
                    clientSockets.Add(newClient);
                    messageServer.AppendText("A client is trying to connect.\n");
                    //map["deneme"] = 0;
                    Thread receiveThread = new Thread(() => Receive(newClient)); // updated
                    receiveThread.Start();
                    
                }
                catch
                {
                    if (terminating)
                    {
                        listening = false;
                    }
                    else
                    {
                        messageServer.AppendText("The socket stopped working.\n");
                    }

                }
            }
        }

        private void Receive(Socket thisClient) // updated
        {
            bool connected = true;

            while (connected && !terminating)
            {
                try
                {
                    Byte[] buffer = new Byte[64];
                    thisClient.Receive(buffer);

                    string incomingMessage = Encoding.Default.GetString(buffer);
                    incomingMessage = incomingMessage.Substring(0, incomingMessage.IndexOf("\0"));
                    messageServer.AppendText("Client: " + incomingMessage + "\n");
                }
                catch
                {
                    if (!terminating)
                    {
                        messageServer.AppendText("A client has disconnected\n");
                    }
                    thisClient.Close();
                    clientSockets.Remove(thisClient);
                    connected = false;
                }
            }
        }
        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            listening = false;
            terminating = true;
            Environment.Exit(0);
        }
    }
}