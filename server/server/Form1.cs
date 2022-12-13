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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace server
{
    public partial class Form1 : Form
    {
        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        List<Socket> inGameSockets = new List<Socket>();
        Dictionary<Socket, string > allSockets = new Dictionary<Socket, string>();
        Dictionary<string, double> inGameMap = new Dictionary<string, double>();



        bool terminating = false;
        bool listening = false;
        bool inGame = false;

        int playerCount = 0;
        int numberOfQuestions;

        // Count of how many user answered a question.
        int answerCount = 0;

        // Index of current question
        int currentQuestion = 0;

        //Dictionary to store the answers of the clients.
        Dictionary<string, int?> answers = new Dictionary<string, int?>(); 

        //Question class that reads questions from a txt file and ask question.
        Questions questions = new Questions("questions.txt");
        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();
        }
        
        // Starting the server.
        private void start_Click(object sender, EventArgs e)
        {
            int serverPort;
            

            messageServer.Clear();
            

            if (Int32.TryParse(port.Text, out serverPort) && Int32.TryParse(questionNumber.Text, out numberOfQuestions))
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, serverPort);
                
                serverSocket.Bind(endPoint);                
                serverSocket.Listen(3);

                listening = true;
                start.Enabled = false;
                messageServer.Enabled = true;

                messageServer.AppendText("Started listening on port: " + serverPort + "\n");

                Thread acceptThread = new Thread(Accept);
                acceptThread.Start();

                Thread userManagerThread = new Thread(UserManager);
                userManagerThread.Start();
                
            }
            else
            {
                messageServer.AppendText("Please check port number and number of questions!! \n");
            }
        }

        // Ask Questions and Validate the answers.
        private void QuestionAndCheck()
        {
            bool questionAsked = false; // currently there is not any question asked.
            while (listening && currentQuestion < numberOfQuestions && inGame)
            {
                if (answerCount == playerCount)
                {    
                    // Ask a question 
                    if (!questionAsked)
                    {
                        string newQuestion = questions.askQuestion(currentQuestion) + "\n";
                        messageServer.AppendText(newQuestion+ "\n");
                        broadCast(newQuestion);
                        questionAsked = true;

                        // no body answered questions yet.
                        answerCount = 0;
                    } // all clients answered questions.
                    else
                    {
                        //Check Answers
                        checkAnswers();

                        //Send and Show Scores
                        sendScores();

                        //ask a new question
                        questionAsked = false;
                        currentQuestion += 1;
                    }
                    
                }
                                       
            }

            // Game is either finished or Interrupted
            
            foreach(var user in inGameMap)
            {
                messageServer.AppendText("Winner is: " + user.Key + " " + user.Value + "\n");

            }
            currentQuestion = 0;
            answerCount = 0;
            inGame= false;
        }

        //Accept clients to the server
        private void Accept()
        {
            while (listening)
            {
                try
                {
                    Socket newClient = serverSocket.Accept();
                    allSockets[newClient] = "";
                    Thread receiveThread = new Thread(() => Receive(newClient)); // updated
                    receiveThread.Start(); 
                    //messageServer.AppendText("A client is trying to connect.\n");
                   
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

        // Check whether user is valid or not
        private bool ValidUser(string name)
        {
            if (allSockets.ContainsValue(name))
            {
                return false;
            }
            return true;
        }

        private void UserManager()
        {
            while(listening)
            {
                if (inGame)
                {
                    startGame.Enabled = false;
                    if(inGameSockets.Count < 2)
                    {
                        inGame = false;
                        messageServer.AppendText("One of the clients has disconnected. There are less players than two!\nGame Ends!\n");
                        broadCast("winner winner chicken dinner");

                    }

                }
                else
                {
                    if(allSockets.Count >= 2  && !inGame)
                    {
                        startGame.Enabled = true;
                    }
                    else
                    {
                        startGame.Enabled =false;  
                    }
                }
            }
        }

        //Receive messages from client
        private void Receive(Socket thisClient) // updated
        {
            bool connected = true;
            bool isNameReceived = false;
            string username = "";
            while (connected && !terminating)
            {
                try
                {
                    // incoming message is username or answer
                    Byte[] buffer = new Byte[64];
                    thisClient.Receive(buffer);

                    string incomingMessage = Encoding.Default.GetString(buffer);
                    incomingMessage = incomingMessage.Substring(0, incomingMessage.IndexOf('\0'));
                    if (!isNameReceived)
                    {
                        if (ValidUser(incomingMessage))
                        {
                            allSockets[thisClient] = incomingMessage; //name
                            username = incomingMessage;
                            isNameReceived = true;
                            if (inGame)
                            {
                                sendMessage(thisClient, "connected and wait");
                                messageServer.AppendText("Client " + incomingMessage + " is connected! It will wait until next game\n");
                            }
                            else
                            {
                                sendMessage(thisClient, "connected to the server");
                                messageServer.AppendText("Client " + incomingMessage + " is connected!\n");
                            }

                        }
                        else
                        {
                            sendMessage(thisClient, "not valid username\n");
                            // close the connection!
                            thisClient.Close();
                            allSockets.Remove(thisClient);
                            connected = false;
                        }
                    }
                    else
                    {
                        // game is started receive answers,
                        // User is valid and game is started. Get answers from the clients.
                        int answerNum;
                        if (Int32.TryParse(incomingMessage, out answerNum))
                        {
                            answers[username] = answerNum;
                            messageServer.AppendText(username + ": " + answerNum + "\n");
                            // lock for answer count, basically it is a mutex
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
                    // if something goes wrong.
                    if (!terminating)
                    {
                        messageServer.AppendText("A client has disconnected\n");
                    }
                    thisClient.Close();
                    // If the socket was in game remove it from the inGameMap and inGameSockets
                    
                    if (inGameMap.ContainsKey(username))
                    {
                        inGameMap.Remove(username);
                        playerCount--;
                    }
                    if (inGameSockets.Contains(thisClient))
                    {
                        inGameSockets.Remove(thisClient);
                    }
                    if (answers.ContainsKey(username))
                    {
                        if (answers[username] != null)
                        {
                            answerCount--;
                        }
                        answers.Remove(username);
                    }
                    allSockets.Remove(thisClient);
                    
                    connected = false;
                }
            }
        }

        //Check Answers of the users
        private void checkAnswers()
        {
            int closestAnswer = int.MaxValue;
            List<string> winnerList = new List<string>();
            foreach (var item in answers)
            {

                int tempAnswer = (int)(item.Value != null ? item.Value! : 0);
                int curr = questions.checkAnswer(currentQuestion, tempAnswer);

                
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
                inGameMap[winner] += 1.0 / winnerList.Count;
            }
        }

        //Broadcast a message to all clients
        private void broadCast(string message)
        {
            Byte[] buffer = Encoding.Default.GetBytes(message);
            foreach (Socket client in inGameSockets)
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

        private void broadCastIncludeWaitings(string message)
        {
            Byte[] buffer = Encoding.Default.GetBytes(message);
            foreach (Socket client in allSockets.Keys)
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

        //Send Message to a spesific client
        private void sendMessage(Socket socket ,string message)
        {
            Byte[] buffer = Encoding.Default.GetBytes(message);
            socket.Send(buffer);
        }

        // Send & Show Scores
        private void sendScores()
        {
            
            string scores = numberOfQuestions - 1 != currentQuestion ? "Scores: \n" : "Final Scores: ";
            var ordered = inGameMap.OrderBy(x => -1 * x.Value).ToDictionary(x => x.Key, x => x.Value);
            foreach (var item in ordered)
            {
                scores += item.Key + ": " + item.Value + " ";
            }
            scores += "\n";
            if (numberOfQuestions - 1 == currentQuestion) // if game is ended send the winner
            {
                var maxValuePair = inGameMap.Aggregate((x, y) => x.Value > y.Value ? x : y);
                bool isDraw = inGameMap.Count(kv => kv.Value == maxValuePair.Value) > 1;
                scores += !isDraw ? "Winner: " + maxValuePair.Key + " !!!\n" : "Draw!!\n";
            }
            messageServer.AppendText(scores+ "\n");
            broadCast(scores);
        }

        //Close Application
        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            listening = false;
            terminating = true;
            broadCast("disconnect");
            Environment.Exit(0);
        }

        private void startGame_Click(object sender, EventArgs e)
        {
            startGame.Enabled = false;
            inGameSockets.Clear();
            inGameMap.Clear();
            foreach (var item in allSockets) 
            {
                inGameSockets.Add(item.Key);
                inGameMap[item.Value] = 0;
                sendMessage(item.Key, "game started");

            }


            playerCount = inGameMap.Count;
            answerCount = playerCount;
            messageServer.AppendText("New game has started!\n");
            Thread questionThread = new Thread(QuestionAndCheck);
            questionThread.Start();
            inGame = true;
            //eligiblityForQuestions = true;
        }
    }
}

