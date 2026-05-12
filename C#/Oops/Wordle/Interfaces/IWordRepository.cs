using Wordle.Enums;

namespace Wordle.interfaces
{
    internal interface IWordRepository
    {
        string GetWord(Difficulty difficulty);
    }
}