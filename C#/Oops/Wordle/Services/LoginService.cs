using Wordle.interfaces;
using Wordle.Models;
using Wordle.Repositories;

namespace Wordle.Services
{
    internal class LoginService
    {
        private readonly IPlayerRepository repository =
            new PlayerRepository();

        public Player Register(
            string username,
            string password)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new Exception(
                    "Username cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new Exception(
                    "Password cannot be empty");
            }

            if (repository.UserExists(username))
            {
                throw new Exception(
                    "Username already exists");
            }

            int id =
                repository.Register(username, password);

            return new Player
            {
                Id = id,
                Username = username
            };
        }

        public Player Login(
            string username,
            string password)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new Exception(
                    "Username cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new Exception(
                    "Password cannot be empty");
            }

            Player? player =
                repository.Login(username, password);

            if (player == null)
            {
                throw new Exception(
                    "Invalid username or password");
            }

            return player;
        }
    }
}
