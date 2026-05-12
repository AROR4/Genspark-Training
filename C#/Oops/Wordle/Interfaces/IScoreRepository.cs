using Wordle.Models;

namespace Wordle.interfaces
{
    internal interface IScoreRepository
    {
        void SaveScore(GameModel gameModel);
    }
}