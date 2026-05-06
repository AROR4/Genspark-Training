
using Wordle.Enums;

namespace Wordle.Models
{
    public class GameModel
    {
        
        public Difficulty SelectedDifficulty { get; set; }
        public string SecretWord { get; set; }
        public int AttemptsTaken { get; set; } = 0;
        public HashSet<string> Guesses { get; set; } = new HashSet<string>();
        public bool IsWon { get; set; } = false;
        public int Score { get; set; } = 0;
        public int AttemptsRemaining => 6 - AttemptsTaken;

        public GameModel(Difficulty difficulty,string secretWord)
        {
            SelectedDifficulty=difficulty;
            AttemptsTaken=0;
            SecretWord=secretWord;
        }
    }
}