using Microsoft.VisualBasic.Logging;
using System;
using System.CodeDom.Compiler;
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

struct Client
{
    
    public Socket socket;
    public string name;
    public bool playing;
   
    public double score;
    public int answer;

    public Client(Socket s, string n, bool p, double sc, int ans = 0)
    {
        socket = s;
        name = n;
        score = sc;
        playing = p;
        answer = ans;
     
    }
};

namespace server
{
    public partial class Form1 : Form
    {
        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        //Dictionary to store the answers of the clients.
        Dictionary<string, Client> clients = new Dictionary<string, Client>();
        List<string> leftplayers = new List<string>();

        bool terminating = false;
        bool listening = false;


        int numberOfQuestions;

        // Index of current question
        int currentQuestion = 0;

        Mutex global_mutex = new Mutex();

        int activeUser = 0;
        int allUsers = 0;

        bool isgameon = false;
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

        private void startGame_Click(object sender, EventArgs e)
        {
            if (Int32.TryParse(questionNumber.Text, out numberOfQuestions))
            {
                messageServer.Clear();
                startGame.Enabled = false;
                isgameon = true;
                leftplayers = new List<string>();
                global_mutex.WaitOne();
                foreach (var client in clients)
                {
                    Client tmp = clients[client.Key];
                    tmp.playing = true;
                    tmp.score = 0;
                    tmp.answer = 0;
                    clients[client.Key] = tmp;
                    Interlocked.Add(ref activeUser, 1);
                }
                global_mutex.ReleaseMutex();

                Thread questionThread = new Thread(QuestionAndCheck);
                questionThread.Start();
            }
            else
            {
                messageServer.AppendText("Please  number of questions!! \n");
            }
        }

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
        private void Receive(Socket thisClient) // updated
        {
            bool isName = true;
            bool connected = true;
            string username = "";
            while (connected && !terminating)
            {
                try
                {
                    Byte[] buffer = new Byte[64];
                    thisClient.Receive(buffer);

                    string incomingMessage = Encoding.Default.GetString(buffer);
                    incomingMessage = incomingMessage.Substring(0, incomingMessage.IndexOf('\0'));
                    if (isName)
                    {
                        if (ValidUser(incomingMessage))
                        {
                            isName = false;
                            username = incomingMessage;
                            sendMessage(thisClient, "connected to the server");
                            messageServer.AppendText("Client " + incomingMessage + " is connected!\n");
                            global_mutex.WaitOne();
                            clients[username] = new Client(thisClient, username, false, 0);
                            global_mutex.ReleaseMutex();
                            Interlocked.Add(ref allUsers, 1);
                            if ((allUsers-activeUser) >= 2 && activeUser == 0)
                            {
                                startGame.Enabled = true;
                            }

                            if (isgameon)
                            {
                                sendMessage(thisClient, "game started");
                            }
                        }           
                        else
                        {
                            // username is not valid 
                            sendMessage(thisClient, "not valid username\n");
                            thisClient.Close();
                            connected = false;
                            isName = false;
                        }

                    }
                    else
                    {                 
                         if (clients[username].playing){
                            
                            int answerNum;

                            if (Int32.TryParse(incomingMessage, out answerNum)){
                                Client tmp = clients[username];
                                tmp.answer = answerNum;
                                global_mutex.WaitOne();
                                clients[username] = tmp;
                                global_mutex.ReleaseMutex();
                                messageServer.AppendText(username + ": " + answerNum + "\n");
                                
                            }
                            else
                            {
                                sendMessage(clients[username].socket, "Could not parse the answer!\n");
                            }
                        }
                    }
                }
                catch
                {

                    bool clientPlaying = clients[username].playing;
                    leftplayers.Add(username);
                    global_mutex.WaitOne();
                    clients.Remove(username);
                    global_mutex.ReleaseMutex();
                    Interlocked.Add(ref allUsers, -1);
                    if (!terminating)
                    {
                        broadCast("Player " + username + " has left the game.");
                        if (clientPlaying)
                        {
                            Interlocked.Add(ref activeUser, -1);
                            messageServer.AppendText(username + " has disconnected from game \n");
                        }
                        else
                        {
                            messageServer.AppendText("A client has disconnected\n");
                        }
                    }
                    if (!isgameon && allUsers < 2)
                    {
                        startGame.Enabled = false;
                    }
                    
                    thisClient.Close();
                    connected = false;
                }
            }
        }
        private void QuestionAndCheck()
        {
            bool questionAsked = false; // currently there is not any question asked.
            while (listening && currentQuestion < numberOfQuestions && activeUser >= 2)
            {
                if (IsAllClientsAnswered())
                {
                    // Ask a question 
                    if (!questionAsked)
                    {
                        string newQuestion = questions.askQuestion(currentQuestion) + "\n";
                        messageServer.AppendText("Question-"+ (currentQuestion+1) +": " + newQuestion + "\n");
                        broadCast("Question-" + (currentQuestion+1) + ": " + newQuestion);
                        resetField();

                        questionAsked = true;

                        
                   
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
            if (activeUser < 2)
            {
                messageServer.AppendText("One of the clients has disconnected. There are less players than two!\nGame Ends!\n");
                global_mutex.WaitOne();
                foreach (Client client in clients.Values)
                {
                    if(client.playing) {
                        messageServer.AppendText("Winner is: " + client.name + " " + client.score + "\n");
                        sendMessage(client.socket , "only one player");
                        sendScores();
                    }
                }
                global_mutex.ReleaseMutex();
            }
            global_mutex.WaitOne();
            foreach (var client in clients)
            {
                Client tmp = clients[client.Key];
                tmp.playing = false;
                tmp.score = 0;
                tmp.answer = Int32.MinValue;
                clients[client.Key] = tmp;
            }
            global_mutex.ReleaseMutex();
            activeUser = 0;
            currentQuestion = 0;
            isgameon = false;

            start.Enabled = false;
            startGame.Enabled = (allUsers-activeUser) >= 2 ? true : false;
        }

        private void resetField()
        {
            global_mutex.WaitOne();
            foreach (var client in clients)
            {
                if (client.Value.playing)
                {
                    Client tmp = clients[client.Key];
                    tmp.answer = Int32.MinValue;
                    clients[client.Key] = tmp;
                }    
            }
            global_mutex.ReleaseMutex();
        }
        private bool IsAllClientsAnswered()
        {
            bool flag = true;
            global_mutex.WaitOne();
            foreach (Client client in clients.Values)
            {
                if (client.answer == (double)Int32.MinValue)
                {
                    flag = false;
                    break;
                }
            }
            global_mutex.ReleaseMutex();
            return flag;
        }
        private void checkAnswers()
        {
            int closestAnswer = int.MaxValue;
            List<string> winnerList = new List<string>();
            global_mutex.WaitOne();
            foreach (var client in clients)
            {
                if (client.Value.playing)
                {
                    int curr = questions.checkAnswer(currentQuestion, client.Value.answer);
                    if (curr < closestAnswer)
                    {
                        winnerList = new List<string> { client.Key };
                        closestAnswer = curr;
                    }
                    else
                    {
                        if (curr == closestAnswer)
                        {
                            winnerList.Add(client.Key);
                        }
                    }
                }
            }
            global_mutex.ReleaseMutex();
            foreach (string winner in winnerList)
            {
                double score = clients[winner].score + 1.0 / winnerList.Count;
                Client tmp = new Client(clients[winner].socket, clients[winner].name, clients[winner].playing, score);
                global_mutex.WaitOne();
                clients[winner] = tmp;
                global_mutex.ReleaseMutex();
            }
        }
        private void broadCast(string message)
        {
            Byte[] buffer = Encoding.Default.GetBytes(message);
            global_mutex.WaitOne();
            foreach (Client client in clients.Values)
            {

                if (client.playing)
                {
                    try
                    {
                        client.socket.Send(buffer);
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
            global_mutex.ReleaseMutex();
        }
        private void broadCastWaitingIncluded(string message)
        {
            Byte[] buffer = Encoding.Default.GetBytes(message);
            global_mutex.WaitOne();
            foreach (Client client in clients.Values)
            {
                try
                {
                    client.socket.Send(buffer);
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
            global_mutex.ReleaseMutex();
        }
        private void sendMessage(Socket socket ,string message)
        {
            Byte[] buffer = Encoding.Default.GetBytes(message);
            socket.Send(buffer);
        }
        private void sendScores()
        {       
            string scores = numberOfQuestions - 1 != currentQuestion  ? "Scores: \n" : "Final Scores: ";
            global_mutex.WaitOne();
            var ordered = clients.Where(x=> x.Value.playing == true).OrderBy(x => -1 * x.Value.score).ToDictionary(x => x.Key, x => x.Value.score   );
            global_mutex.ReleaseMutex();
            foreach (string player in leftplayers)
            {
                 ordered[player] = 0;
               
            }
            foreach (var item in ordered)
            {      
               scores += item.Key + ": " + item.Value + " ";      
            }
            scores += "\n";
            messageServer.AppendText(scores);
            if ((numberOfQuestions - 1 == currentQuestion)) // if game is ended send the winner
            {
                global_mutex.WaitOne();
                var maxValuePair = clients.Aggregate((x, y) => x.Value.score > y.Value.score ? x : y);
                bool isDraw = clients.Count(kv => kv.Value.score == maxValuePair.Value.score) > 1;
                global_mutex.ReleaseMutex();
                scores += !isDraw ? "Winner: " + maxValuePair.Key + " !!!\n" : "Draw!!\n";
                messageServer.AppendText(!isDraw ? "Winner: " + maxValuePair.Key + " !!!\n" : "Draw!!\n");
            }
            
            broadCast(scores);
        }
        private bool ValidUser(string name)
        {
            global_mutex.WaitOne();
            if (clients.ContainsKey(name))
            {
                global_mutex.ReleaseMutex();
                return false;
            }
            global_mutex.ReleaseMutex();
            return true;
        }
        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            listening = false;
            terminating = true;
            broadCastWaitingIncluded("disconnect");
            Environment.Exit(0);
        }
    }
}

