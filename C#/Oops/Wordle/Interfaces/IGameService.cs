using Wordle.Enums;
using Wordle.Models;

namespace Wordle.interfaces
{
    public interface IGameService
    {
        void Start(
            int playerId,
            Difficulty difficulty);

        string SubmitGuess(string guess);

        void SaveGame();
        GameModel GetGameState();
    }
}