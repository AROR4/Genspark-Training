using Wordle.Enums;
using Wordle.Models;
using Wordle.Exceptions;
using Wordle.Services;
using Wordle.interfaces;

namespace Wordle.Services
{
    public class GameService : IGameService
    {
        private GameModel? _gameModel;

        public void Start(Difficulty difficulty)
        {
            WordProvider wordProvider = new WordProvider();
            string secretWord = wordProvider.GetWord(difficulty);
            _gameModel = new GameModel(difficulty, secretWord);
            GameModel gameModel = GetCurrentGame();

            while (gameModel.AttemptsTaken < 6)
            {
                Console.WriteLine($"\n--- Attempt {gameModel.AttemptsTaken + 1} of 6 ---");
                string guess = InputGuess();
                
                string result = FeedbackGenerator.GetAttemptFeedback(gameModel.SecretWord, guess.ToUpper());
                gameModel.Guesses.Add(guess.ToUpper());
                
                DisplayFeedback(guess, result);

                if (result == "GGGGG")
                {
                    gameModel.IsWon = true;
                    gameModel.Score = FeedbackGenerator.CalculateScore(gameModel.AttemptsTaken + 1, true);
                    Console.WriteLine($"\n{FeedbackGenerator.GetFinalFeedback(gameModel.AttemptsTaken + 1)}");
                    return;
                }
                gameModel.AttemptsTaken++;
            }
            
            gameModel.IsWon = false;
            gameModel.Score = 0;
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
                    GuessValidator.ValidateWord(input, GetCurrentGame().Guesses);
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
            return GetCurrentGame();
        }

        private GameModel GetCurrentGame()
        {
            return _gameModel ?? throw new InvalidOperationException("Game has not been started.");
        }
    }



}
