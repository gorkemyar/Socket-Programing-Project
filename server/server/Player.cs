using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace server
{
    internal class Player
    {
        public Socket socket;
        public string name;
        public bool playing;
        public int score;

        Player(Socket s, string n, bool p, int sc)
        {
            socket = s;
            name = n;
            score = sc;
            playing = p;
        }
        
    }
}
