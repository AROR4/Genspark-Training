using NotificationModelLibrary;

namespace NotificationBLLibrary.Interfaces
{
    public interface IUserService
    {
        public User AddUser();

        public void GetUser();
        
        public void Update();

        public List<User> GetAllUsers();

        public void Delete();

    }
}