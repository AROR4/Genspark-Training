using Wordle.Enums;
using Wordle.Models;

namespace Wordle.interfaces
{
    public interface IGameService
    {
        void Start(Difficulty difficulty);
        GameModel GetGameState();
    }
}