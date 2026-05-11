using Npgsql;

namespace NotificationDALLibrary
{
    public class DbConnection
    {
        internal NpgsqlConnection connection =
            new NpgsqlConnection(
                Environment.GetEnvironmentVariable(
            "DB_CONNECTION_STRING")
            );

        public void TestConnection()
        {
            try
            {
                connection.Open();

                Console.WriteLine(
                    "Database connected successfully."
                );

                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "Connection failed: " + ex.Message
                );
            }
        }
    }
}
