using Microsoft.VisualBasic.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Policy;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.AxHost;

namespace client
{
    public partial class Form1 : Form
    {
        bool terminating = false;
        bool connected = false;
        Socket clientSocket;

        int numberOfQuestions;
        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();
        }


        // Connect to a server
        private void connect_Click(object sender, EventArgs e)
        {
            
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            string IP = ip.Text;

            int portNum;
            if (Int32.TryParse(port.Text, out portNum))
            {
                try
                {
                    disconnect.Enabled = true;
                    clientSocket.Connect(IP, portNum);
                    connect.Enabled = false;
                    messageBox.Enabled = true;
                    connected = true;
                    

                    // send username
                    string sendingMessage = username.Text;
                    Byte[] buffer = Encoding.Default.GetBytes(sendingMessage);
                    clientSocket.Send(buffer);

                    // start a new thread to receive messages
                    Thread receiveThread = new Thread(Receive);
                    receiveThread.Start();

                }
                catch
                {
                    messageBox.AppendText("Could not connect to the server!\n");
                    disconnect.Enabled = false;
                }
            }
            else
            {
                messageBox.AppendText("Check the port\n");
            }
        }

        // Receive messages from server
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


                    if (incomingMessage.Contains("only one player")) // get question numbers
                    {
                        messageBox.AppendText("Only you are left in the game, you are the winner! ");
                        send.Enabled = false;
                          
                    } else if (incomingMessage.Contains("connected to the server")){ // connection is established

                        messageBox.Clear();
                        messageBox.AppendText("Connected to the Server!\n");
                        //send.Enabled = true;

                    } else if (incomingMessage.Contains("not valid username")) { // not a valid user

                        messageBox.AppendText("Already, used user name!\n");
                        clientSocket.Close();
                        connected = false;
                        terminating = true;
                        connect.Enabled = true;
                        disconnect.Enabled = false;
                        send.Enabled = false;

                    } else if (incomingMessage.Contains("game started")) // if server is full already
                    {
                        messageBox.AppendText("The game has already started, wait for the next round \n");
                        send.Enabled = false;
                    }
                    else if (incomingMessage.Contains("Final")) // if the game succesfully completed
                    {
                        messageBox.AppendText("The game is finished. " + incomingMessage + "\n");
                        //clientSocket.Close();
                        //connected = false;
                        //terminating = true;
                        //connect.Enabled = true;
                        //disconnect.Enabled = false;
                        send.Enabled = false;
                    }
                    else if (incomingMessage.Contains("disconnect")) // if game interrupted
                    {
                        messageBox.AppendText("The game is terminated. A client or server may drop.\n");
                        clientSocket.Close();
                        connected = false;
                        terminating = true;
                        connect.Enabled = true;
                        disconnect.Enabled = false;
                        send.Enabled = false;
                        disconnect.Enabled = false;
                    }
                    else if (incomingMessage.Contains("Question")) // any other messages
                    {
                        messageBox.AppendText(incomingMessage + "\n");
                        answerBox.Enabled = true;
                        send.Enabled = true;
                    }else if (incomingMessage.Contains("Could not parse the answer"))
                    {   
                        messageBox.AppendText(incomingMessage + "\n");
                        send.Enabled = true;
                        answerBox.Enabled= true;
                    }
                    else
                    {
                        messageBox.AppendText(incomingMessage + "\n");
                    }
                }
                catch // if something goes wrong
                {
                    if (!terminating)
                    {
                        messageBox.AppendText("The server has disconnected\n");
                        connect.Enabled = true;
                    }
                    disconnect.Enabled = false;
                    clientSocket.Close();
                    connected = false;
                }

            }
        }

        // Send answers to the server
        private void send_Click(object sender, EventArgs e)
        {
            send.Enabled = false;
            answerBox.Enabled = false;
            string sendingMessage = answerBox.Text;
            Byte[] buffer = Encoding.Default.GetBytes(sendingMessage);
            messageBox.AppendText("Answer Sent: " + sendingMessage + "\n");
            clientSocket.Send(buffer); 
        }

        // Close application
        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            connected = false;
            terminating = true;
            Environment.Exit(0);
        }

        // Disconnect from the server
        private void disconnect_Click(object sender, EventArgs e)
        {
            connect.Enabled = true;
            disconnect.Enabled = false;
            messageBox.AppendText("Disconnected from the Server!\n");
            clientSocket.Close();
            connected = false;
            terminating = true;
            send.Enabled=false;
        }
    }
}