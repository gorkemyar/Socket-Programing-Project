// See https://aka.ms/new-console-template for more information
Questions q = new Questions("questions.txt");
int qNumber = 10;
Console.WriteLine(q.askQuestion(qNumber));
int answer = 92;

if (q.checkAnswer(qNumber, answer)) {
    Console.WriteLine("correct");
} else {
    Console.WriteLine("incorrect");
}
