using NotificationModelLibrary;

namespace NotificationBLLibrary.Interfaces
{
    public interface IUserService
    {
        User AddUser(string name, string email, string phone);

        User? GetUser(int id);

        User? UpdateUser(int id, string? email = null, string? phone = null);

        List<User> GetAllUsers();

        User? DeleteUser(int id);

        
    }
}
