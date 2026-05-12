using Wordle.Enums;
using Wordle.interfaces;
using Wordle.Repositories;

namespace Wordle.Services
{
    internal class WordProvider
    {
        private readonly IWordRepository repository = new WordRepository();

        public string GetWord(Difficulty difficulty)
        {
            return repository.GetWord(difficulty);
        }
    }
}