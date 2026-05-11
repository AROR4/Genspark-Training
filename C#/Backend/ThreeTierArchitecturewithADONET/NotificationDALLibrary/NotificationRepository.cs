

using NotificationDALLibrary.Interfaces;
using NotificationModelLibrary;
using NotificationModelLibrary.Enums;
using Npgsql;

namespace NotificationDALLibrary{
    public class NotificationRepository : INotificationRepository<Notification>
    {
        DbConnection dbConnection=new DbConnection();
        public Notification? Create(Notification notification)
        {
            string query=$"insert into notifications(message,notificationtype,recipient,sentdate,userid) values('{notification.Message}','{notification.NotificationType}','{notification.Recipient}','{notification.SentDateTime}','{notification.userId}')";
            NpgsqlCommand command=new NpgsqlCommand(query,dbConnection.connection);
            try
            {
                dbConnection.connection.Open();
                int result=command.ExecuteNonQuery();
                if (result > 0)
                {
                    return notification;
                }
                return null;
            }
            catch (PostgresException pe)
            {
                throw new Exception(pe.Message);
                
            }
            finally{
                dbConnection.connection?.Close();
            }
        }

        public List<Notification> GetAll()
    {
        List<Notification> notifications =
            new List<Notification>();

        string query =
            "SELECT * FROM notifications";

        NpgsqlCommand command =
            new NpgsqlCommand(query, dbConnection.connection);

        try
        {
            dbConnection.connection.Open();

            NpgsqlDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                Notification notification =
                    new Notification(
                        reader["message"].ToString() ?? "",
                        Enum.Parse<NotificationType>(
                            reader["notificationtype"].ToString() ?? ""
                        ),
                        reader["recipient"].ToString() ?? "",
                        Convert.ToDateTime(
                            reader["sentdate"]
                        ),
                        Convert.ToInt32(reader["userid"])
                    );

                notifications.Add(notification);
            }

            return notifications;
        }
        catch (PostgresException pe)
        {
            throw new Exception("Error while fetching notifications from database.", pe);
        }
        finally
        {
            dbConnection.connection.Close();
        }
    }
    public List<Notification> GetNotificationsByUserId(int userId)
    {
        List<Notification> notifications =
            new List<Notification>();

        string query =
            $@"SELECT n.message,
                      n.notificationtype,
                      n.recipient,
                      n.sentdate,
                      n.userid,
                      u.name,
                      u.email,
                      u.phonenumber
               FROM notifications n
               INNER JOIN users u ON n.userid = u.userid
               WHERE n.userid = {userId}";

        NpgsqlCommand command =
            new NpgsqlCommand(query, dbConnection.connection);


        try
        {
            dbConnection.connection.Open();

            NpgsqlDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                Notification notification =
                    new Notification(
                        reader["message"].ToString() ?? "",
                        Enum.Parse<NotificationType>(
                            reader["notificationtype"].ToString() ?? ""
                        ),
                        reader["recipient"].ToString() ?? "",
                        Convert.ToDateTime(reader["sentdate"]),
                        Convert.ToInt32(reader["userid"])
                    );
                notification.UserName = reader["name"].ToString() ?? "";
                notification.UserEmail = reader["email"].ToString() ?? "";
                notification.UserPhoneNumber = reader["phonenumber"].ToString() ?? "";

                notifications.Add(notification);
            }

            return notifications;
        }
        catch (PostgresException pe)
        {
            throw new Exception($"Error while fetching user with id .", pe);
        }

        finally
        {
            dbConnection.connection.Close();
        }
    }

        
    }
}
