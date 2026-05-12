using Wordle.Enums;

namespace Wordle.Models
{
    public class GameModel
    {
        public int PlayerId { get; set; }

        public Difficulty SelectedDifficulty { get; set; }

        public string SecretWord { get; set; }

        public int AttemptsTaken { get; set; } = 0;

        public HashSet<string> Guesses { get; set; } =
            new HashSet<string>();

        public bool IsWon { get; set; } = false;

        public int Score { get; set; } = 0;

        public int AttemptsRemaining => 6 - AttemptsTaken;

        public string feedback =String.Empty;

        public GameModel(
            int playerId,
            Difficulty difficulty,
            string secretWord)
        {
            PlayerId = playerId;

            SelectedDifficulty = difficulty;

            SecretWord = secretWord;
        }
    }
}