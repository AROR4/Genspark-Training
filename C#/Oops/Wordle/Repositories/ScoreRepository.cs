using Npgsql;
using Wordle.interfaces;
using Wordle.Models;

namespace Wordle.Repositories
{
    internal class ScoreRepository : IScoreRepository
    {
        private readonly DbConnection dbConnection =
            new DbConnection();

        public void SaveScore(GameModel gameModel)
        {
            try
            {
                dbConnection.connection.Open();

                string query =
                    @"INSERT INTO Scores
                    (PlayerId, Score, AttemptsUsed, HiddenWord)
                    VALUES
                    (@playerId, @score, @attempts, @word)";

                NpgsqlCommand command =
                    new NpgsqlCommand(
                        query,
                        dbConnection.connection);

                command.Parameters.AddWithValue(
                    "@playerId",
                    gameModel.PlayerId);

                command.Parameters.AddWithValue(
                    "@score",
                    gameModel.Score);

                command.Parameters.AddWithValue(
                    "@attempts",
                    gameModel.AttemptsTaken);

                command.Parameters.AddWithValue(
                    "@word",
                    gameModel.SecretWord);

                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error saving score : " +
                    ex.Message);
            }
            finally
            {
                dbConnection.connection.Close();
            }
        }
    }
}