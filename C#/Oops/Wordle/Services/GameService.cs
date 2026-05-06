using Wordle.Enums;
using Wordle.Models;
using Wordle.Exceptions;
using Wordle.Services;
using Wordle.interfaces;

namespace Wordle.Services
{
    public class GameService : IGameService
    {
        public GameModel GameModel ;

        public void Start(Difficulty difficulty)
        {
            WordProvider wordProvider = new WordProvider();
            string secretWord = wordProvider.GetWord(difficulty);
            GameModel = new GameModel ( difficulty,secretWord);

            while (GameModel.AttemptsTaken < 6)
            {
                Console.WriteLine($"\n--- Attempt {GameModel.AttemptsTaken + 1} of 6 ---");
                string guess = InputGuess();
                
                string result = FeedbackGenerator.GetAttemptFeedback(GameModel.SecretWord, guess.ToUpper());
                GameModel.Guesses.Add(guess.ToUpper());
                
                DisplayFeedback(guess, result);

                if (result == "GGGGG")
                {
                    GameModel.IsWon = true;
                    GameModel.Score = FeedbackGenerator.CalculateScore(GameModel.AttemptsTaken + 1, true);
                    Console.WriteLine($"\n{FeedbackGenerator.GetFinalFeedback(GameModel.AttemptsTaken + 1)}");
                    return;
                }
                GameModel.AttemptsTaken++;
            }
            
            GameModel.IsWon = false;
            GameModel.Score = 0;
            Console.WriteLine("\nGame Over! You ran out of attempts.");
        }

        public string InputGuess()
        {
            while (true)
            {
                Console.Write("Enter a 5-letter word: ");
                string input = Console.ReadLine()?.ToUpper() ?? "";

                try
                {
                    GuessValidator.ValidateWord(input, GameModel.Guesses);
                    return input;
                }
                catch (InvalidGuessException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.ResetColor();
                }
            }
        }

        private void DisplayFeedback(string guess, string feedback)
        {
            for (int i = 0; i < guess.Length; i++)
            {
                if (feedback[i] == 'G')
                {
                    Console.ForegroundColor=ConsoleColor.Green;
                    Console.Write($"{guess[i]} ");
                }
                else if(feedback[i] == 'Y')
                {
                    Console.ForegroundColor=ConsoleColor.Yellow;
                    Console.Write($"{guess[i]} ");
                }
                else
                {
                    Console.ForegroundColor=ConsoleColor.Gray;
                    Console.Write($"{guess[i]} ");
                }
            }
            Console.ResetColor();
            Console.WriteLine();
            for (int i = 0; i < guess.Length; i++)
            {
                Console.Write($"{feedback[i]} ");
            }
            Console.WriteLine();
        }

        public GameModel GetGameState()
        {
            return GameModel;
        }
    }



}
