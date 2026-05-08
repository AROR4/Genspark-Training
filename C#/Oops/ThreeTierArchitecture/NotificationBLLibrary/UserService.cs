using NotificationModelLibrary;
using NotificationDALLibrary;
using NotificationBLLibrary.Interfaces;
using NotificationBLLibrary.Validators;

namespace NotificationBLLibrary
{
    public class UserService : IUserService
    {
        UserRepository userRepository = new UserRepository();

        public User AddUser(string name, string email, string phone)
        {
            //final check before creating user
            UserValidator.ValidateName(name);
            UserValidator.ValidateEmail(email);
            UserValidator.ValidatePhone(phone);

            User user = new User(name, email, phone);
            var newuser = userRepository.Create(user);
            return newuser;
        }

        public User? DeleteUser(int id)
        {
            return userRepository.Delete(id);
        }

        public List<User> GetAllUsers()
        {
            return userRepository.GetAll();
        }

        public User? GetUser(int id)
        {
            return userRepository.GetById(id);
        }

        public User? UpdateUser(int id, string? email = null, string? phone = null)
        {
            var user = userRepository.GetById(id);

            if (user == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                //final check
                UserValidator.ValidateEmail(email, user.Email);
                user.Email = email;
            }

            if (!string.IsNullOrWhiteSpace(phone))
            {
                //final check
                UserValidator.ValidatePhone(phone, user.PhoneNumber);
                user.PhoneNumber = phone;
            }

            return userRepository.Update(id, user);
        }

        
    }
}
