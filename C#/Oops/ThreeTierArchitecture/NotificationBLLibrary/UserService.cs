using NotificationModelLibrary;
using NotificationDALLibrary;
using NotificationBLLibrary.Interfaces;

namespace NotificationBLLibrary
{
    public class UserService : IUserService
    {
      
        UserRepository userRepository=new UserRepository();
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
            var newuser=userRepository.Create(user);
            return newuser;
        }

        public User? DeleteUser(int id)
        {
            return userRepository.Delete(id);
        }

        public void Delete()
        {
            Console.Write("Enter User ID to delete: ");

            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid ID");
                return;
            }

            var user = DeleteUser(id);

            if (user == null)
            {
                Console.WriteLine("User not found");
                return;
            }
            Console.WriteLine(user);
            Console.WriteLine("This user deleted successfully");
        }

        public  List<User> GetAllUsers()
        {
            return userRepository.GetAll();
        }

        public void GetUser()
        {
            Console.Write("Enter User ID ");
             if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid ID");
                return ;
            }

            var user=userRepository.GetById(id);
            if (user != null)
            {
                Console.WriteLine(user);
            }
            else
            {
                Console.WriteLine("User not found");
            }
        }


   

        public void Update()
        {
            Console.Write("Enter User ID: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid ID");
                return;
            }

            var user = userRepository.GetById(id);

            if (user == null)
            {
                Console.WriteLine("User not found");
                return;
            }

            Console.WriteLine("What do you want to update?");
            Console.WriteLine("1. Email");
            Console.WriteLine("2. Phone");
            Console.WriteLine("3. Both");

            if (!int.TryParse(Console.ReadLine(), out int choice))
            {
                Console.WriteLine("Please enter a valid number");
                return;
            }

            string? email = null;
            string? phone = null;

            switch (choice)
            {
                case 1:
                    email = PromptForValidEmail();
                    break;

                case 2:
                    phone = PromptForValidPhone();
                    break;

                case 3:
                    email = PromptForValidEmail();
                    phone = PromptForValidPhone();
                    break;

                default:
                    Console.WriteLine("Invalid choice");
                    return;
            }

            UpdateUserFields(id, email, phone);
        }

        public void UpdateUserFields(int id, string? email = null, string? phone = null)
        {
            var user = userRepository.GetById(id);

            if (user == null)
            {
                Console.WriteLine("User not found");
                return;
            }

            if (!string.IsNullOrWhiteSpace(email))
                user.Email = email;

            if (!string.IsNullOrWhiteSpace(phone))
                user.PhoneNumber = phone;

            userRepository.Update(id,user);   

            Console.WriteLine("User updated successfully");
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

        private string PromptForValidEmail()
        {
            string email;
            do
            {
                Console.Write("Enter new email: ");
                email = Console.ReadLine() ?? string.Empty;
                if (!IsValidEmail(email))
                {
                    Console.WriteLine("Invalid email format.");
                }
            } while (!IsValidEmail(email));

            return email;
        }

        private string PromptForValidPhone()
        {
            string phone;
            do
            {
                Console.Write("Enter new phone: ");
                phone = Console.ReadLine() ?? string.Empty;
                if (!IsValidPhone(phone))
                {
                    Console.WriteLine("Invalid phone number. Must be 10 digits.");
                }
            } while (!IsValidPhone(phone));

            return phone;
        }
    }
}
