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
        int answerCount = 0;
        int currentQuestion = -1;
        Dictionary<string, int> answers = new Dictionary<string, int>(); 


        Questions questions = new Questions("questions.txt");
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
                Thread questionThread = new Thread(QuestionAndCheck);
                questionThread.Start();

                messageServer.AppendText("Started listening on port: " + serverPort + "\n");

            }
            else
            {
                messageServer.AppendText("Please check port number \n");
            }
        }

        private void QuestionAndCheck()
        {
            
            while (listening)
            {
                if (map.Count == 2)
                {
                    if (answerCount == 2)
                    {
                        // scorela ama ilk sorudan önce scorlama
                        if(currentQuestion >= 0)
                        {
                            int closestAnswer = int.MaxValue;
                            List<string> winnerList = new List<string>();
                            foreach (var item in answers)
                            {
                                int curr = questions.checkAnswer(currentQuestion, item.Value);
                                if (curr < closestAnswer)
                                {
                                    winnerList = new List<string> { item.Key };
                                    closestAnswer = curr;
                                }
                                else
                                {
                                    if (curr == closestAnswer)
                                    {
                                        winnerList.Add(item.Key);
                                    }
                                }

                            }
                            foreach (var winner in winnerList)
                            {
                                map[winner] += 1.0 / winnerList.Count;
                            }


                            // scoreları yazdır

                            messageServer.AppendText("Scores: \n");
                            foreach (var item in map)
                            {
                                messageServer.AppendText(item.Key + ": " + item.Value + "\n");
                            }
                        }


                        
                        // current question arttır
                        currentQuestion += 1;
                        // yeni soru yolla  
                        Byte[] buffer = Encoding.Default.GetBytes(questions.askQuestion(currentQuestion));
                        foreach (Socket client in clientSockets)
                        {
                            try
                            {
                                client.Send(buffer);
                            }
                            catch
                            {
                                messageServer.AppendText("There is a problem! Check the connection...\n");
                                terminating = true;
                                messageServer.Enabled = false;
                                port.Enabled = true;
                                start.Enabled = true;
                                serverSocket.Close();
                            }

                        }
                        

                        // answer count 0 la
                        answerCount = 0;
                       

                    }
                   
                    
                }
            }
        }


        
        private void Accept()
        {
            while (listening)
            {
                try
                {
                    Socket newClient = serverSocket.Accept();
                    
                    clientSockets.Add(newClient);
                    messageServer.AppendText("A client is trying to connected.\n");
                   
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

        private bool ValidUser(string name)
        {
            if (map.Count >= 2 || map.ContainsKey(name))
            {
                return false;
            }
            return true;
        }
        private void Receive(Socket thisClient) // updated
        {
            bool connected = true;
            bool isPending = true;
            string username = "";
            while (connected && !terminating)
            {
                try
                {
                    Byte[] buffer = new Byte[64];
                    thisClient.Receive(buffer);

                    string incomingMessage = Encoding.Default.GetString(buffer);
                    incomingMessage = incomingMessage.Substring(0, incomingMessage.IndexOf('\0'));

                    if (isPending && ValidUser(incomingMessage))
                    {
                        isPending = false;
                        username = incomingMessage;
                        map[incomingMessage] = 0;
                        messageServer.AppendText("Client: " + incomingMessage + "\n");
                        if (map.Count == 2)
                        {
                            answerCount = 2; 
                        }

                    }
                    else if (isPending)
                    {
                        if (!terminating)
                        {
                            messageServer.AppendText("A client has disconnected\n");
                        }
                        thisClient.Close();
                        clientSockets.Remove(thisClient);
                        connected = false;
                        isPending = false;
                    }
                    else
                    {
                        //messageServer.AppendText(username+": "+ incomingMessage +"\n");
                        int answerNum;
                        if (Int32.TryParse(incomingMessage, out answerNum)){
                            answers[username] = answerNum;
                            Interlocked.Add(ref answerCount, 1);

                        }
                        else
                        {
                            Byte[] errorBuffer = Encoding.Default.GetBytes("Could not parse the answer!\n");
                            thisClient.Send(errorBuffer);
                        }
                        
                    }
                    
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