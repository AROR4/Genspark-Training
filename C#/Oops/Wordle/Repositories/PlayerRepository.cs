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

                string query = @"
            SELECT p.Id,
                   p.Username,
                   COALESCE(SUM(s.Score), 0) AS TotalScore
            FROM Players p
            LEFT JOIN Scores s
            ON p.id = s.playerid
            WHERE p.Username = @username
            AND p.Password = @password
            GROUP BY p.Id, p.Username";

                NpgsqlCommand command =
                    new NpgsqlCommand(query, dbConnection.connection);

                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", password);

                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    return new Player
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Username = reader["Username"].ToString() ?? "",
                        TotalScore = Convert.ToInt32(reader["TotalScore"])
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