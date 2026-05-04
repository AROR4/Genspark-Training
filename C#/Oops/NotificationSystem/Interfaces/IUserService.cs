using NotificationSystem.Models;

namespace NotificationSystem
{
    internal interface IUserService
    {
        public User AddUser();

        public List<User> GetAllUsers();

        
    }
}