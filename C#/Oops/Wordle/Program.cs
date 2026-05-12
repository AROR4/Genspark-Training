using Wordle.Services;
using Wordle.Enums;
using Wordle.interfaces;
using Wordle.Models;

class Program
{

    static void Main(string[] args)
    {
        DotNetEnv.Env.Load();
        LoginService loginService = new LoginService();

        Console.Clear();
        Console.WriteLine("Welcome to Wordle C#!");
        Player? player;

        while (true)
        {
            Console.WriteLine("Choose Option");
            Console.WriteLine("\n1. Sign Up");
            Console.WriteLine("2. Login");

            Console.Write("Enter choice: ");


            try
            {
                if (!int.TryParse(
                    Console.ReadLine(),
                    out int authchoice))
                {
                    Console.WriteLine(
                        "Please enter 1 or 2.");
                    continue;
                }

                if (authchoice == 1)
                {
                    Console.Write("Enter Username: ");
                    string username = Console.ReadLine() ?? "";

                    Console.Write("Enter Password: ");
                    string password = Console.ReadLine() ?? "";

                    player =
                        loginService.Register(
                            username,
                            password);

                    Console.WriteLine(
                        "\nRegistration Successful!");

                    break;
                }
                else if (authchoice == 2)
                {
                    Console.Write("Enter Username: ");
                    string username =
                        Console.ReadLine() ?? "";

                    Console.Write("Enter Password: ");
                    string password =
                        Console.ReadLine() ?? "";

                    player =
                        loginService.Login(
                            username,
                            password);

                    Console.WriteLine(
                        "\nLogin Successful!");

                    break;
                }
                else
                {
                    Console.WriteLine("Invalid Choice. Try Again");

                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine(ex.Message);

                Console.ResetColor();
            }
        }

        Console.ForegroundColor=ConsoleColor.DarkGreen;
        Console.WriteLine("HI, " + player.Username + " Let's Play!!");
        Console.ResetColor();

        bool playAgain = true;
        while (playAgain)
        {
            Difficulty difficulty;
            while (true)
            {
                Console.WriteLine("\nChoose Difficulty");
                Console.WriteLine("1. Easy");
                Console.WriteLine("2. Medium");
                Console.WriteLine("3. Hard");

                Console.Write("Enter choice: ");

                if (!int.TryParse(
                    Console.ReadLine(),
                    out int choice))
                {
                    Console.WriteLine(
                        "Please enter 1, 2, or 3.");
                    continue;
                }

                if (choice == 1)
                {
                    difficulty = Difficulty.Easy;
                    break;
                }
                else if (choice == 2)
                {
                    difficulty = Difficulty.Medium;
                    break;
                }
                else if (choice == 3)
                {
                    difficulty = Difficulty.Hard;
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid choice. Please try again.");
                }

            }

            IGameService gameService =
                new GameService();

            gameService.Start(
                player.Id,
                difficulty);

            GameModel gameState =
                gameService.GetGameState();

            while (gameState.AttemptsTaken < 6)
            {
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"━━━━━━━━ Attempt {gameState.AttemptsTaken + 1}/6 ━━━━━━━━");
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("➤ Enter 5-letter word: ");
                Console.ResetColor();

                string input = Console.ReadLine()??"";
                string guess=input.ToUpper();

                try
                {
                    string feedback = gameService.SubmitGuess(guess);

                    DisplayFeedback(guess, feedback);

                    gameState = gameService.GetGameState();

                    if (gameState.IsWon)
                    {
                        Console.WriteLine();

                        Console.ForegroundColor = ConsoleColor.Green;


                        Console.WriteLine("      🎉 YOU WON! 🎉       ");


                        Console.ResetColor();

                        Console.WriteLine(
                            $"✅ Solved in {gameState.AttemptsTaken} attempts!");

                        break;
                    }

                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n{ex.Message}");
                    Console.ResetColor();
                }
            }
            gameService.SaveGame();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"🏆 FINAL SCORE : {gameState.Score}");
            Console.ResetColor();

            if (!gameState.IsWon)
            {
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("       💀 GAME OVER        ");
                Console.WriteLine();
                Console.ResetColor();

                Console.WriteLine(
                    $"🔍 Correct Word: {gameState.SecretWord}");
            }

            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("🔁 Would you like to play again? (y/n): ");
            Console.ResetColor();

            playAgain = Console.ReadLine()?.ToLower() == "y";

        }

        Console.WriteLine("✨ Thank You For Playing Wordle ✨");



    }

    public static void DisplayFeedback(string guess, string feedback)
    {
        Console.WriteLine();
        Console.Write("    ");
        for (int i = 0; i < guess.Length; i++)
        {
            if (feedback[i] == 'G')
            {
                Console.BackgroundColor = ConsoleColor.Green;
                Console.ForegroundColor = ConsoleColor.Black;
            }
            else if (feedback[i] == 'Y')
            {
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.ForegroundColor = ConsoleColor.Black;
            }
            else
            {
                Console.BackgroundColor = ConsoleColor.DarkGray;
                Console.ForegroundColor = ConsoleColor.White;
            }

            Console.Write($" {char.ToUpper(guess[i])} ");
            Console.ResetColor();
            Console.Write(" ");
        }

        Console.WriteLine();
        Console.WriteLine();
        Console.Write("    ");
        for (int i = 0; i < feedback.Length; i++)
        {
            if (feedback[i] == 'G')
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else if (feedback[i] == 'Y')
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            Console.Write($" {feedback[i]} ");
            Console.ResetColor();
            Console.Write(" ");
        }
        Console.WriteLine("\n");
    }

}
