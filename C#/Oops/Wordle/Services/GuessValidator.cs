using System.Collections.Generic;
using System.Linq;
using Wordle.Exceptions;

namespace Wordle.Services
{
    internal class GuessValidator
    {
        public static void ValidateWord(string word, HashSet<string> previousGuesses)
        {
            if (string.IsNullOrWhiteSpace(word))
                throw new InvalidGuessException("The word cannot be empty.");
            
            if (word.Length != 5)
                throw new InvalidGuessException("The word must be exactly 5 letters long.");

            if (word.Any(c => !char.IsLetter(c)))
                throw new InvalidGuessException("The word must only contain letters.");

            if (previousGuesses.Contains(word.ToUpper()))
                throw new InvalidGuessException("You have already guessed that word!");
        }
    }
}