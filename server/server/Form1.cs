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
using System.Security.Policy;
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
        
        Dictionary<string, Socket> clientSockets = new Dictionary<string, Socket>();
        Dictionary<string, double> map = new Dictionary<string, double>();
        Dictionary<string, Socket> waitingMap = new Dictionary<string, Socket>();

        bool terminating = false;
        bool listening = false;

        bool isGameStarted = false;
        int startGameButtonClicked = 0;
        int playerCount = 0;

        int numberOfQuestions;

        //Flag that checks eligibility for question asking
        bool eligiblityForQuestions = false;

        // Count of how many user answered a question.
        int answerCount = 0;

        // Index of current question
        int currentQuestion = 0;

        //Dictionary to store the answers of the clients.
        Dictionary<string, int> answers = new Dictionary<string, int>(); 

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
                //serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                
                serverSocket.Bind(endPoint);
                
                serverSocket.Listen(3);

                listening = true;
                start.Enabled = false;
                messageServer.Enabled = true;

                messageServer.AppendText("Started listening on port: " + serverPort + "\n");

                Thread acceptThread = new Thread(Accept);
                acceptThread.Start();
                
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
            while (listening && currentQuestion < numberOfQuestions && eligiblityForQuestions)
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
            if(!eligiblityForQuestions)
            {
                messageServer.AppendText("One of the clients has disconnected. There are less players than two!\nGame Ends!\n");
                foreach(var user in map)
                {
                    messageServer.AppendText("Winner is: " + user.Key + " " + user.Value + "\n");

                }
                broadCast("winner winner chicken dinner");

            }

            //foreach(var client in clientSockets)
            //{
            //    client.Close();
            //}

            foreach(var user in clientSockets)
            {
                waitingMap[user.Key] = user.Value;
            }
            clientSockets.Clear();

            map.Clear();
            answers.Clear();
            currentQuestion = 0;
            answerCount = 0;
            
            isGameStarted = false;
            start.Enabled = false;
            startGame.Enabled = waitingMap.Count >= 2 ? true: false;
            listening = false;
            //serverSocket.Close();
            //serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        }

        //Accept clients to the server
        private void Accept()
        {
            while (listening)
            {
                try
                {
                    Socket newClient = serverSocket.Accept();    
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

        // Check whether user is valid or not
        private bool ValidUser(string name)
        {
            if (waitingMap.ContainsKey(name) ||clientSockets.ContainsKey(name))
            {
                return false;
            }
            return true;
        }


        /*
        private void ReceiveWaiting(Socket thisClient) // updated
        {
           
            bool connected = true;
            string username = "";
            int gameCount = startGameButtonClicked;
            bool isPending = true;

            while (connected && !terminating && gameCount == startGameButtonClicked)
            {

                try
                {
                    // incoming message is username check its validty
                    Byte[] buffer = new Byte[64];
                    thisClient.Receive(buffer);

                    string incomingMessage = Encoding.Default.GetString(buffer);
                    incomingMessage = incomingMessage.Substring(0, incomingMessage.IndexOf('\0'));

                    //isPending will true until there is exactly 2 users;
                    
                }
                catch
                {
                    // if something goes wrong.
                    if (!terminating)
                    {
                        messageServer.AppendText("A waiting client has disconnected\n");
                    }
                    thisClient.Close();
                    waitingMap.Remove(username);
                    if (waitingMap.Count() < 2 && !isGameStarted)
                    {
                        startGame.Enabled = false;
                    }
                    connected = false;

                }
            }
        }
        */


        //Receive messages from client
        private void Receive(Socket thisClient) // updated
        {
            bool isWaiting = true;
            bool isPending = true;
            bool connected = true;
            string username = "";
            int gameCount = startGameButtonClicked;
            while (connected && !terminating)
            {
                try
                {

                    
                    // incoming message is username check its validty
                    Byte[] buffer = new Byte[64];
                    thisClient.Receive(buffer);

                    string incomingMessage = Encoding.Default.GetString(buffer);
                    incomingMessage = incomingMessage.Substring(0, incomingMessage.IndexOf('\0'));

                    if (isPending && ValidUser(incomingMessage))
                    {
                        isPending = false;
                        username = incomingMessage;
                        sendMessage(thisClient, "connected to the server");
                        messageServer.AppendText("Client " + incomingMessage + " is connected!\n");

                        waitingMap[incomingMessage] = thisClient;
                        if (waitingMap.Count >= 2 && !isGameStarted)
                        {
                            startGame.Enabled = true;
                        }

                    }
                    else if (isPending)
                    {
                        // username is not valid or already enough users in the game. Send corresponding message
                        sendMessage(thisClient, "not valid username\n");
                        thisClient.Close();
                        connected = false;
                        isPending = false;
                    }


                    while (gameCount == startGameButtonClicked && isWaiting)
                    {
                        messageServer.AppendText("IS this working\n");
                        
                    }
                    if (isWaiting)
                    {
                        gameCount = startGameButtonClicked;
                        isWaiting = false;
                        waitingMap.Remove(username);
                        clientSockets[username] = thisClient;
                        map[username] = 0;
                    }
                    

                    int answerNum;
                    if (!isWaiting && Int32.TryParse(incomingMessage, out answerNum))
                    {
                        answers[username] = answerNum;
                        messageServer.AppendText(username+": "+ answerNum +"\n");
                        // lock for answer count, basically it is a mutex
                        Interlocked.Add(ref answerCount, 1);
                    }
                    else if (!isWaiting)
                    {
                        Byte[] errorBuffer = Encoding.Default.GetBytes("Could not parse the answer!\n");
                        thisClient.Send(errorBuffer);
                    }    
                }
                catch
                {
                    // if something goes wrong.
                    if (!terminating)
                    {
                        messageServer.AppendText("A playing client has disconnected\n");
                    }
                    thisClient.Close();
                    clientSockets.Remove(username);
                    waitingMap.Remove(username);
                    map.Remove(username);
                    answers.Remove(username);

                    playerCount = clientSockets.Count;
                    connected = false;
                    if (clientSockets.Count < 2)
                    {
                        eligiblityForQuestions = false;
                    }
 
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
        }

        //Broadcast a message to all clients
        private void broadCast(string message)
        {
            Byte[] buffer = Encoding.Default.GetBytes(message);
            foreach (var client in clientSockets)
            {
                try
                {
                    client.Value.Send(buffer);
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
            var ordered = map.OrderBy(x => -1 * x.Value).ToDictionary(x => x.Key, x => x.Value);
            foreach (var item in ordered)
            {
                scores += item.Key + ": " + item.Value + " ";
            }
            scores += "\n";
            if (numberOfQuestions - 1 == currentQuestion && map.Count > 0) // if game is ended send the winner
            {
                var maxValuePair = map.Aggregate((x, y) => x.Value > y.Value ? x : y);
                bool isDraw = map.Count(kv => kv.Value == maxValuePair.Value) > 1;
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

            playerCount = waitingMap.Count;
            answerCount = waitingMap.Count;

            startGame.Enabled = false;
            startGameButtonClicked++;
            isGameStarted = true;

            eligiblityForQuestions = true;

            Thread questionThread = new Thread(QuestionAndCheck);
            questionThread.Start();
        }

    }
}

