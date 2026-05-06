using Wordle.Services;
using Wordle.Enums;
using Wordle.interfaces;

class Program
{
    static void Main(string[] args)
    {
        bool playAgain = true;
        while (playAgain)
        {
            Console.Clear();
            Console.WriteLine("Welcome to Wordle C#!");
            Console.WriteLine("Choose Difficulty: 1. Easy, 2. Medium , 3. Hard");
            
            
            var choice=Console.ReadLine() ;
            Difficulty diff;

            if (choice == "2")
            {
                diff = Difficulty.Medium;
            }
            else if (choice == "3")
            {
                diff = Difficulty.Hard;
            }
            else
            {
                diff = Difficulty.Easy;
            }
                        
            IGameService game = new GameService();
            game.Start(diff);

            Console.WriteLine($"Final Score: {game.GetGameState().Score}");
            if(!game.GetGameState().IsWon)
                Console.WriteLine($"The correct word was : {game.GetGameState().SecretWord}");
            Console.Write("\nWould you like to play again? (y/n): ");
            playAgain = Console.ReadLine()?.ToLower() == "y";
        }
    }
}