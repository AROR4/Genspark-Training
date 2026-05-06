namespace Wordle.Services
{
    internal class FeedbackGenerator
    {
        public static string GetAttemptFeedback(string word, string guess)
        {
            char[] result = new char[5];
            bool[] wordUsed = new bool[5];
            bool[] guessUsed = new bool[5]; 

            for (int i = 0; i < 5; i++)
            {
                if (guess[i] == word[i])
                {
                    result[i] = 'G';
                    wordUsed[i] = true;
                    guessUsed[i] = true;
                }
            }

            for (int i = 0; i < 5; i++)
            {
                if (guessUsed[i]) continue; 
                for (int j = 0; j < 5; j++)
                {
                    if (!wordUsed[j] && guess[i] == word[j])
                    {
                        result[i] = 'Y';
                        wordUsed[j] = true; 
                        guessUsed[i] = true;
                        break;
                    }
                }

                if (!guessUsed[i])
                {
                    result[i] = 'X';
                }
            }

            return new string(result);
        }

        public static string GetFinalFeedback(int attemptsTaken)
        {
            switch (attemptsTaken)
            {
                case 1: 
                return "Genius!";
                case 2: 
                return "Excellent!";
                case 3: 
                return "Great job!";
                case 4: 
                return "Good work!";
                case 5: 
                return "Nice try!";
                case 6: 
                return "That was close!";
                default:
                return "Oops!! Attempts are over!! Try Again. "; 
            }
      
        }

        public static int CalculateScore(int attemptsTaken, bool isWon)
        {
            if (!isWon) return 0;
            int baseScore = 600;
            int penalty = attemptsTaken * 100;
            return Math.Max(50, baseScore - penalty);
        }
    }
}