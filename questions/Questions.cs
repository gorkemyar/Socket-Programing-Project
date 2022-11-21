class Questions
{
    private Dictionary<int, string> numberToQuestions;
    private Dictionary<string, int> questionsToAnswers;

    public Questions(string filepath) {
        string[] lines = File.ReadAllLines(filepath);  
        questionsToAnswers = new Dictionary<string, int>();
        numberToQuestions = new Dictionary<int, string>();

        int i = 0;

        while(i+1 < lines.Length) {
            
            string question = lines[i];
            int answer = Int32.Parse(lines[i+1]);
            questionsToAnswers.Add(question,answer);
            numberToQuestions.Add((i / 2), question);
            i += 2;
        }

    }

    private int getIndex(int questionNumber) {
        return (questionNumber % numberToQuestions.Count);
    }

    public string askQuestion(int questionNumber) {
        return numberToQuestions[getIndex(questionNumber)];
    }

    public bool checkAnswer(int questionNumber, int answer) {
        string question = numberToQuestions[getIndex(questionNumber)];
        return questionsToAnswers[question] == answer;
    }
}