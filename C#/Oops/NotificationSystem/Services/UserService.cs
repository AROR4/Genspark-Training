using NotificationSystem.Models;

namespace NotificationSystem
{
    internal class UserService : IUserService
    {
        List<User> users = new List<User>();

        public User AddUser()
        {
            Console.WriteLine("Enter the details for adding the user");

            string name;
            do
            {
                Console.WriteLine("Enter the full name:");
                name = Console.ReadLine() ?? "";
                if (string.IsNullOrWhiteSpace(name))
                {
                    Console.WriteLine("Name cannot be empty.");
                }
            } while (string.IsNullOrWhiteSpace(name));

            string email;
            do
            {
                Console.WriteLine("Enter the email of user:");
                email = Console.ReadLine() ?? "";
                if (!IsValidEmail(email))
                {
                    Console.WriteLine("Invalid email format.");
                }
            } while (!IsValidEmail(email));

            string phone;
            do
            {
                Console.WriteLine("Enter the phone number of user:");
                phone = Console.ReadLine() ?? "";
                if (!IsValidPhone(phone))
                {
                    Console.WriteLine("Invalid phone number. Must be 10 digits.");
                }
            } while (!IsValidPhone(phone));

            User user = new User(name, email, phone);
            users.Add(user);

            return user;
        }


        public  List<User> GetAllUsers()
        {
            return users;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhone(string phone)
        {
            return phone.Length == 10 && phone.All(char.IsDigit);
        }

    }
}