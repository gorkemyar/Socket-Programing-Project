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
using static System.Formats.Asn1.AsnWriter;

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
        int currentQuestion = 0;

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
            int numberOfQuestions;


            if (Int32.TryParse(port.Text, out serverPort) && Int32.TryParse(questionNumber.Text, out numberOfQuestions))
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, serverPort);
                serverSocket.Bind(endPoint);
                serverSocket.Listen(3);

                listening = true;
                start.Enabled = false;
                messageServer.Enabled = true;

                Thread acceptThread = new Thread(Accept);
                acceptThread.Start();
                Thread questionThread = new Thread(() => { QuestionAndCheck(numberOfQuestions); });
                questionThread.Start();

                messageServer.AppendText("Started listening on port: " + serverPort + "\n");

            }
            else
            {
                messageServer.AppendText("Please check port number and number of questions!! \n");
            }
        }

        private void QuestionAndCheck(int numberOfQuestions)
        {
            bool questionAsked = false;
            while (listening & currentQuestion < numberOfQuestions)
            {
                if (map.Count == 2)
                {
                    if (answerCount == 2)
                    {
                        
                        // yeni soru yolla  
                        if (!questionAsked)
                        {
                            string newQuestion = questions.askQuestion(currentQuestion) + "\n";
                            messageServer.AppendText(newQuestion);
                            broadCast(newQuestion);
                            questionAsked = true;

                            // answer count = 0
                            answerCount = 0;
                        }
                        else
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

                            string scores = numberOfQuestions - 1 != currentQuestion ? "Scores: \n" : "Final Scores: ";
                            var ordered = map.OrderBy(x => -1 * x.Value).ToDictionary(x => x.Key, x => x.Value);
                            foreach (var item in ordered)
                            {
                                scores += item.Key + ": " + item.Value + " ";
                            }
                            scores += "\n";
                            messageServer.AppendText(scores);
                            broadCast(scores);
                            
                            questionAsked = false;
                            currentQuestion += 1;
                        }
                        // scorela ama ilk sorudan önce scorlama
                    }
                                      
                }
            }
            foreach(var client in clientSockets)
            {
                client.Close();
            }
            Console.WriteLine("Deneme");
            clientSockets.Clear();
            map.Clear();
            answers.Clear();
            currentQuestion = 0;
            answerCount = 0;
            
        }



        
        private void Accept()
        {
            while (listening)
            {
                try
                {
                    Socket newClient = serverSocket.Accept();
                    
                    clientSockets.Add(newClient);
                    //messageServer.AppendText("A client is trying to connected.\n");
                   
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
                        messageServer.AppendText("Client " + incomingMessage + " is connected!\n");
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

        private void broadCast(string message)
        {
            Byte[] buffer = Encoding.Default.GetBytes(message);
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
        }
    }
}

