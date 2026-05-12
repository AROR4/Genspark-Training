using Wordle.Models;

namespace Wordle.interfaces
{
    internal interface IPlayerRepository
    {
        bool UserExists(string username);

        int Register(string username, string password);

        Player? Login(string username, string password);
    }
}