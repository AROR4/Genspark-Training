using NotificationModelLibrary;

namespace NotificationDALLibrary{
public class UserRepository : Repository<int,User>
{
    public override User? GetById(int id)
    {
        try
        {
            return _notificationContext.Users
                .FirstOrDefault(u => u.Id == id);
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"Error fetching User with id {id}: {ex.Message}");
        }
    }

    public override User? Update(int id, User updatedUser)
    {
        try
        {
            var existingUser = GetById(id);

            if (existingUser == null)
            {
                return null;
            }

            existingUser.PhoneNumber = updatedUser.PhoneNumber;
            existingUser.Email = updatedUser.Email;

            _notificationContext.SaveChanges();

            return existingUser;
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"Error updating User with id {id}: {ex.Message}");
        }
    }

    public override User? Delete(int id)
    {
        try
        {
            var user = GetById(id);

            if (user == null)
            {
                return null;
            }

            _notificationContext.Users.Remove(user);

            _notificationContext.SaveChanges();

            return user;
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"Error deleting User with id {id}: {ex.InnerException?.Message}");
        }
    }
}

   
}
