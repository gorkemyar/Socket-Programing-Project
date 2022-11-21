struct Player {
    public string playerName;
    public double score;

    public Player(string name) {
        playerName = name;
        score = 0.0;
    }
}

class Score {
    Player player1;
    Player player2;

    public Score(string player1Name, string player2Name) {
        player1 = new Player(player1Name);
        player2 = new Player(player2Name);
    }

    public void changeScore(bool player1answer, bool player2answer) {
        if (player1answer && player2answer) {
            player1.score += 0.5;
            player2.score += 0.5;
            return;
        } else if (player1answer) {
            player1.score += 1;
        } else {
            player2.score += 1;
        }
    }
}