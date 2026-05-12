using Npgsql;
using Wordle.interfaces;
using Wordle.Models;

namespace Wordle.Repositories
{
    internal class PlayerRepository : IPlayerRepository
    {
        private readonly DbConnection dbConnection =
            new DbConnection();
        public bool UserExists(string username)
        {
            try
            {
                dbConnection.connection.Open();

                string query =
                    "SELECT COUNT(*) FROM Players WHERE Username=@username";

                NpgsqlCommand command =
                    new NpgsqlCommand(query, dbConnection.connection);

                command.Parameters.AddWithValue("@username", username);

                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error checking username: " + ex.Message);
            }
            finally
            {
                dbConnection.connection.Close();
            }
        }

        public int Register(string username, string password)
        {
            try
            {
                dbConnection.connection.Open();

                string query =
                    @"INSERT INTO Players(Username,Password)
                    VALUES(@username,@password)
                    RETURNING Id";

                NpgsqlCommand command =
                    new NpgsqlCommand(query, dbConnection.connection);

                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", password);

                return Convert.ToInt32(command.ExecuteScalar());
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Registration failed: " + ex.Message);
            }
            finally
            {
                dbConnection.connection.Close();
            }
        }

        public Player? Login(string username, string password)
        {
            try
            {
                dbConnection.connection.Open();

                string query =
                    @"SELECT * FROM Players
                    WHERE Username=@username
                    AND Password=@password";

                NpgsqlCommand command =
                    new NpgsqlCommand(query, dbConnection.connection);

                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", password);

                var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    return new Player
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Username = reader["Username"].ToString()??""
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Login failed: " + ex.Message);
            }
            finally
            {
                dbConnection.connection.Close();
            }
        }
    }
}