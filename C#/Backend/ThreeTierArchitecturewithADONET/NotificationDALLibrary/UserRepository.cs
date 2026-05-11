using NotificationModelLibrary;
using Npgsql;
using NotificationDALLibrary.Interfaces;

namespace NotificationDALLibrary{
public class UserRepository : IRepository<int,User>
{
    DbConnection dbConnection=new DbConnection();

    public  User Create(User user)
    {
        string insertCmd =
        $"INSERT INTO users(name, email, phonenumber) VALUES('{user.Name}', '{user.Email}', '{user.PhoneNumber}') RETURNING userid";
       
        NpgsqlCommand command =
        new NpgsqlCommand(insertCmd, dbConnection.connection);

        try
        {
            dbConnection.connection.Open();

            int generatedid = Convert.ToInt32(command.ExecuteScalar());

            Console.WriteLine($"User created successfully with ID :{generatedid}");
            user.id=generatedid;
            return user;
            
        }
        catch(PostgresException pe)
        {
        if(pe.SqlState == "23505")
        {
            if(pe.ConstraintName == "users_email_key")
            {
                throw new Exception("Email already exists.");
            }

            if(pe.ConstraintName == "users_phonenumber_key")
            {
                throw new Exception("Phone number already exists.");
            }
        }

        throw new Exception(pe.Message);
        }
        finally
        {
            dbConnection.connection.Close();
        }
    }

    public List<User> GetAll()
    {
        List<User> users = new List<User>();

        string query = "SELECT * FROM users";

        NpgsqlCommand command =
        new NpgsqlCommand(query, dbConnection.connection);

        try
        {
            dbConnection.connection.Open();

            NpgsqlDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                User user = new User(
                    Convert.ToInt32(reader["userid"]),
                    reader["name"].ToString() ?? "",
                    reader["email"].ToString() ?? "",
                    reader["phonenumber"].ToString() ?? ""
                );

                users.Add(user);
            }

            return users;
        }
        catch (PostgresException pe)
        {
        throw new Exception("Error while fetching users from database.", pe);
        }
        finally
        {
            dbConnection.connection.Close();
        }
    }

    public  User? GetById(int id)
    {
        string query =
        $"SELECT * FROM users WHERE userid ={id}";

        NpgsqlCommand command=new NpgsqlCommand(query,dbConnection.connection);

        try{
            dbConnection.connection.Open();
            NpgsqlDataReader reader =
            command.ExecuteReader();

                if (reader.Read())
                {
                    User user = new User(
                    Convert.ToInt32(reader["userid"]),
                    reader["name"].ToString() ?? "",
                    reader["email"].ToString() ?? "",
                    reader["phonenumber"].ToString() ?? ""
                    );
                    return user;
                }
                return null;

        }
        catch (PostgresException pe)
        {
            throw new Exception($"Error while fetching user with id {id}.", pe);
        }

        finally
        {
            dbConnection.connection.Close();
        }

        
    }

    public User? Update(int id, User updatedUser)
    {
        string query =
            $"UPDATE users SET name = '{updatedUser.Name}',email = '{updatedUser.Email}',phonenumber = '{updatedUser.PhoneNumber}' WHERE userid = {updatedUser.id}";

        NpgsqlCommand command =
            new NpgsqlCommand(query, dbConnection.connection);


        try
        {
            dbConnection.connection.Open();

            int result = command.ExecuteNonQuery();

            if (result > 0)
            {
                return updatedUser;
            }

            return null;
        }
        catch (PostgresException pe)
        {
            if(pe.ConstraintName == "users_email_key")
            {
                throw new Exception(" Email already exists.");
            }
            
            if(pe.ConstraintName == "users_phonenumber_key")
            {
                throw new Exception(" Phone number already exists.");
            }
            return null;
        }
        finally
        {
            dbConnection.connection?.Close();
        }
    }

    public User? Delete(int id)
    {
        User? user = GetById(id);

        if (user == null)
        {
            return null;
        }

        string query =
            $"DELETE FROM users WHERE userid = {id}";

        NpgsqlCommand command =
            new NpgsqlCommand(query, dbConnection.connection);

        try
        {
            dbConnection.connection.Open();

            int result = command.ExecuteNonQuery();

            if (result > 0)
            {
                return user;
            }

            return null;
        }
        catch (PostgresException pe)
        {
            Console.WriteLine(pe.Message);
            return null;
        }
        finally
        {
            dbConnection.connection.Close();
        }
    }
      

       
    }

   
}
