using Npgsql;
using Wordle.Enums;
using Wordle.interfaces;

namespace Wordle.Repositories
{
    internal class WordRepository : IWordRepository
    {
        private readonly DbConnection dbConnection =
            new DbConnection();

        public string GetWord(
            Difficulty difficulty)
        {
            try
            {
                dbConnection.connection.Open();

                string query =
                    @"SELECT Word
                    FROM Words
                    WHERE Difficulty=@difficulty
                    ORDER BY RANDOM()
                    LIMIT 1";

                NpgsqlCommand command =
                    new NpgsqlCommand(
                        query,
                        dbConnection.connection);

                command.Parameters.AddWithValue(
                    "@difficulty",
                    difficulty.ToString());

                string word=command.ExecuteScalar()?.ToString()??"";
                if (word== null || word.Length==0)
                {
                    throw new Exception("Error fetching word ");
                }
                return word;

            }
            catch(Exception ex)
            {
                throw new Exception(
                    "Error fetching word : " +
                    ex.Message);
            }
            finally
            {
                dbConnection.connection.Close();
            }
        }
    }
}