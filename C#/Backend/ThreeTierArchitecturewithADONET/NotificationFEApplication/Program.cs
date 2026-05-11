
using NotificationBLLibrary.Interfaces;
using NotificationModelLibrary;
using NotificationBLLibrary;
using NotificationModelLibrary.Enums;
using NotificationModelLibrary.Exceptions;
using NotificationBLLibrary.Validators;



namespace NotificationSystem
{
    internal class Program
    {
        INotificationService notificationService;
        IUserService userService;

        public Program()
        {   
            notificationService=new NotificationService();
            userService=new UserService();
            
        }

        void NotificationSender()
        {
             Console.WriteLine("---------Welcome to Notification Service Simulation-------");
            int Choice=1;
            while (Choice!=3)
            {
                Console.WriteLine("Please Select any of the Services");
                Console.WriteLine("1. User Management");
                Console.WriteLine("2. Notification Services");
                Console.WriteLine("3. Exit");
                while(!int.TryParse(Console.ReadLine(), out Choice) || Choice <= 0 || Choice > 3)
                {
                    Console.WriteLine("Invalid Option Selected. Please try again");
                }

                switch (Choice)
                {
                    case 1:
                    {
                        int userchoice=1;
                        while(userchoice!=6)
                        {
                        Console.WriteLine("Please select any of the services");
                        Console.WriteLine("1. Add User");
                        Console.WriteLine("2. Get All Users");
                        Console.WriteLine("3. Get Specific User");
                        Console.WriteLine("4. Update Details of a User");
                        Console.WriteLine("5. Delete a User");
                        Console.WriteLine("6. Exit");
                        while(!int.TryParse(Console.ReadLine(), out userchoice) || userchoice <= 0 || userchoice > 6)
                                {
                                    Console.WriteLine("Invalid Option Selected. Please try again");
                                }
                        switch (userchoice)
                        {
                            case 1:

                                try{
                                string name = GetValidName();
                                string email = GetValidEmail();
                                string phone = GetValidPhone();
                                var user = userService.AddUser(name, email, phone);
                                Console.WriteLine("User Added Successfully");
                                Console.WriteLine(user);
                                }
                                catch(Exception e)
                                {
                                    Console.WriteLine("Error occured! User not created." + e.Message);
                                }
                                break;
                            case 2:

                                List<User> users = userService.GetAllUsers();

                                if (users.Count == 0)
                                {
                                    Console.WriteLine("No user added yet. Please add user first");
                                    break;
                                }

                                for (int i = 0; i < users.Count; i++)
                                {
                                    Console.WriteLine($"{i + 1}. {users[i]} \n");
                                }
                                break;
                                

                            case 3:
                                Console.Write("Enter User ID: ");
                                if (!int.TryParse(Console.ReadLine(), out int getUserId))
                                {
                                    Console.WriteLine("Invalid ID");
                                    break;
                                }
                                var selectedUser = userService.GetUser(getUserId);
                                if (selectedUser == null)
                                {
                                    Console.WriteLine("User not found");
                                    break;
                                }
                                Console.WriteLine(selectedUser);
                                break;
                                
                            case 4:
                                try{
                                Console.Write("Enter User ID: ");
                                if (!int.TryParse(Console.ReadLine(), out int updateUserId))
                                {
                                    Console.WriteLine("Invalid ID");
                                    break;
                                }
                                var userToUpdate = userService.GetUser(updateUserId);
                                if (userToUpdate == null)
                                {
                                    Console.WriteLine("User not found");
                                    break;
                                }
                                Console.WriteLine(userToUpdate);
                                Console.WriteLine("What do you want to update?");
                                Console.WriteLine("1. Email");
                                Console.WriteLine("2. Phone");
                                Console.WriteLine("3. Both");

                                int updateChoice;
                                while (!int.TryParse(Console.ReadLine(), out updateChoice)
                                    || updateChoice <= 0
                                    || updateChoice > 3)
                                {
                                    Console.WriteLine("Please enter a valid number");
                                }

                                string? updatedEmail = null;
                                string? updatedPhone = null;

                                switch (updateChoice)
                                {
                                    case 1:
                                        updatedEmail = GetValidEmail(userToUpdate.Email);
                                        break;
                                    case 2:
                                        updatedPhone = GetValidPhone(userToUpdate.PhoneNumber);
                                        break;
                                    case 3:
                                        updatedEmail = GetValidEmail(userToUpdate.Email);
                                        updatedPhone = GetValidPhone(userToUpdate.PhoneNumber);
                                        break;
                                }

                                    userService.UpdateUser(updateUserId, updatedEmail, updatedPhone);
                                    Console.WriteLine("User updated successfully");
                                }
                                catch(Exception e)
                                {
                                    Console.WriteLine("Error occured! User details not updated." + e.Message);
                                }
                                break;
                            case 5:
                                try{
                                Console.Write("Enter User ID to delete: ");
                                if (!int.TryParse(Console.ReadLine(), out int deleteUserId))
                                {
                                    Console.WriteLine("Invalid ID");
                                    break;
                                }
                                var deletedUser = userService.DeleteUser(deleteUserId);
                                if (deletedUser == null)
                                {
                                    Console.WriteLine("User not found");
                                    break;
                                }
                                Console.WriteLine(deletedUser);
                                Console.WriteLine("This user deleted successfully");
                                }
                                catch(Exception e)
                                {
                                    Console.WriteLine("Error occured! User  not deleted." + e.Message);
                                }
                                break;
                            case 6:
                                break;
                        }
                    }
                    break;
                }
                    case 2: 
                    int notifchoice=1;
                    while (notifchoice != 5)
                    {
                        Console.WriteLine("Please select any of the services");
                        Console.WriteLine("1. Send Notification to a user");
                        Console.WriteLine("2. Send Notification to All Users");
                        Console.WriteLine("3. See Notification History");
                        Console.WriteLine("4. See Notification History By User Id");
                        Console.WriteLine("5. Exit"); 
                        while(!int.TryParse(Console.ReadLine(), out notifchoice) || notifchoice <= 0|| notifchoice > 5)
                        {
                            Console.WriteLine("Invalid Option Selected. Please try again");
                        }  
                        switch (notifchoice)
                        {
                            case 1: 
                                try{
                                List<User> users = userService.GetAllUsers();

                                if (users.Count == 0)
                                {
                                    Console.WriteLine("No user added yet. Please add user first");
                                    break;
                                }

                                Console.WriteLine("Select User");
                                foreach (User listedUser in users)
                                {
                                    Console.WriteLine($"{listedUser} \n");
                                }

                                User? selectedUser = null;
                                int selectedUserId;
                                Console.Write("Enter User ID: ");
                                while (!int.TryParse(Console.ReadLine(), out selectedUserId)
                                    || (selectedUser = users.FirstOrDefault(user => user.id == selectedUserId)) == null)
                                {
                                    Console.Write("Invalid User ID. Please enter one of the listed IDs: ");
                                }

                                Console.WriteLine("Select Notification Type");
                                Console.WriteLine("1.Email");
                                Console.WriteLine("2.SMS");
                                int singletype;
                                Console.Write("Enter notification type: ");
                                while (!int.TryParse(Console.ReadLine(), out singletype)
                                    || singletype <= 0
                                    || singletype > 2)
                                {
                                    Console.Write("Invalid option. Enter 1 for Email or 2 for SMS: ");
                                }
                                string singleMessage ;
                                if(singletype==1){
                                    singleMessage = GetValidMessage(NotificationType.Email);
                                    notificationService.SendNotification(NotificationType.Email, selectedUser, singleMessage);
                                }
                                else
                                {
                                    singleMessage = GetValidMessage(NotificationType.Sms);
                                    notificationService.SendNotification(NotificationType.Sms, selectedUser, singleMessage);
                                }
                            }
                            catch(Exception e)
                            {
                                Console.WriteLine("Error occured! Notification sending failed." + e.Message);
                            }

                            break;
                            
                            case 2:
                                try{
                                List<User> storedusers = userService.GetAllUsers();

                                if (storedusers.Count == 0)
                                {
                                    Console.WriteLine("No user added yet. Please add user first");
                                    break;
                                }
                                Console.WriteLine("Select Notification Type");
                                Console.WriteLine("1.Email");
                                Console.WriteLine("2.SMS");
                                int alltype;
                                Console.Write("Enter notification type: ");
                                while (!int.TryParse(Console.ReadLine(), out alltype)
                                    || alltype <= 0
                                    || alltype > 2)
                                {
                                    Console.Write("Invalid option. Enter 1 for Email or 2 for SMS: ");
                                }
                                string allUsersMessage ;
                                if(alltype==1){
                                    allUsersMessage = GetValidMessage(NotificationType.Email);
                                    notificationService.SendToAllUsers(NotificationType.Email, storedusers, allUsersMessage);
                                }
                                else
                                {
                                    allUsersMessage = GetValidMessage(NotificationType.Sms);
                                    notificationService.SendToAllUsers(NotificationType.Sms, storedusers, allUsersMessage);
                                }
                                }
                                catch(Exception e)
                                {
                                    Console.WriteLine("Error occured! Notification sending failed." + e.Message);
                                }
                                break;
                            case 3:
                                List<Notification> notifications = notificationService.GetHistory();
                                if (notifications.Count == 0)
                                {
                                    Console.WriteLine("No notifications sent yet.");
                                    break;
                                }

                                Console.WriteLine("Notification History");
                                foreach (Notification notification in notifications)
                                {
                                    Console.WriteLine(notification);
                                }
                                break;
                            case 4 :
                                Console.Write("Enter User ID: ");
                                if (!int.TryParse(Console.ReadLine(), out int notificationUserId))
                                {
                                    Console.WriteLine("Invalid ID");
                                    break;
                                }

                                List<Notification> userNotifications = notificationService.GetHistoryByUserId(notificationUserId);
                                if (userNotifications.Count == 0)
                                {
                                    Console.WriteLine("No notifications found for this user.");
                                    break;
                                }

                                Console.WriteLine("Notification History For User");
                                foreach (Notification notification in userNotifications)
                                {
                                    Console.WriteLine(notification);
                                }
                                break;
                            case 5 :
                                break;
                            }


                    }
                    break;

                    case 3:
                    {
                        Console.WriteLine("Thanks for using the Service");
                        Console.WriteLine("---------------------------");
                        break;
                    }

                    default:
                    {
                        Console.WriteLine("Invalid choice. Try again.");
                        break;
                    }
                }

                
            }
        }

        // All actual validation rules are handled in the Business Layer.
        // Validation checks here are only used to improve user experience
        // by immediately re-prompting invalid fields instead of requiring
        // the user to re-enter all details again.
        // treating this as UI
        private string GetValidMessage(NotificationType notificationType)
        {
            string message;
            do
            {
                Console.WriteLine("Enter message to send");
                message = Console.ReadLine() ?? "";
                try
                {
                    NotificationValidator.ValidateMessage(message, notificationType);
                    return message;
                }
                catch (InvalidMessageException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.WriteLine("Please enter message again.");
                }
            } while (true);
        }

        private string GetValidName()
        {
            string name;
            do
            {
                Console.WriteLine("Enter the full name:");
                name = Console.ReadLine() ?? "";
                try
                {
                    UserValidator.ValidateName(name);
                    return name;
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            } while (true);
        }

        private string GetValidEmail(string previous = "")
        {
            string email;
            do
            {
                Console.Write("Enter email: ");
                email = Console.ReadLine() ?? "";
                try
                {
                    UserValidator.ValidateEmail(email, previous);
                    return email;
                }
                catch (InvalidEmailException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.WriteLine("Please enter email again.");
                }
            } while (true);
        }

        private string GetValidPhone(string previous = "")
        {
            string phone;
            do
            {
                Console.Write("Enter phone: ");
                phone = Console.ReadLine() ?? "";
                try
                {
                    UserValidator.ValidatePhone(phone, previous);
                    return phone;
                }
                catch (InvalidPhoneException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.WriteLine("Please enter 10-digit phone number again.");
                }
            } while (true);
        }

        static void Main(string[] args)
        {   
            DotNetEnv.Env.Load();
            new Program().NotificationSender();
        }
    }
}
