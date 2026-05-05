using NotificationSystem.Models;

namespace NotificationSystem
{
    internal interface IUserService
    {
        public User AddUser();

        public void GetUser();
        
        public void Update();

        public List<User> GetAllUsers();

        public void Delete();

    }
}