
using NotificationBLLibrary.Interfaces;
using NotificationModelLibrary;
using NotificationBLLibrary;



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
            while (Choice!=9)
            {
                Console.WriteLine("Please Select any of the Services");
                Console.WriteLine("1. Add User");
                Console.WriteLine("2. Get All Users");
                Console.WriteLine("3. Get Specific User");
                Console.WriteLine("4. Update Details of a User");
                Console.WriteLine("5. Delete a User");
                Console.WriteLine("6. Send Email Notification to User");
                Console.WriteLine("7. Send Sms Notification to User");
                Console.WriteLine("8. Notification History");
                Console.WriteLine("9. Exit");
                while(!int.TryParse(Console.ReadLine(), out Choice) && Choice>0 && Choice < 8)
                {
                    Console.WriteLine("Invalid Option Selected. Please try again");
                }

                switch (Choice)
                {
                    case 1:
                    {
                        var user = userService.AddUser();
                        Console.WriteLine("User Added Successfully");
                        Console.WriteLine(user);
                        break;
                    }
                    case 2:
                    {
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
                    }
                    case 3:
                    {
                        userService.GetUser();
                        break;
                    }
                    case 4:
                    {
                        userService.Update();
                        break;
                    }
                    case 5:
                    {
                        userService.Delete();
                        break;
                    }
                    case 6:
                    {
                        List<User> users = userService.GetAllUsers();

                        if (users.Count == 0)
                        {
                            Console.WriteLine("No user added yet. Please add user first");
                            return;
                        }

                        Console.WriteLine("Select User");
                        for (int i = 0; i < users.Count; i++)
                        {
                            Console.WriteLine($"{i + 1}. {users[i]} \n");
                        }

                        int userchoice;
                        while (!int.TryParse(Console.ReadLine(), out userchoice)
                            || userchoice <= 0
                            || userchoice > users.Count)
                        {
                            Console.WriteLine("Invalid Option Selected. Please try again");
                        }

                        Console.WriteLine("Enter message to send");
                        string message = Console.ReadLine() ?? "";

                        notificationService.SendNotification(new EmailNotification(), message, users[userchoice - 1]);
                        break;
                    }

                    case 7:
                    {
                        List<User> users = userService.GetAllUsers();

                        if (users.Count == 0)
                        {
                            Console.WriteLine("No user added yet. Please add user first");
                            return;
                        }

                        Console.WriteLine("Select User");
                        for (int i = 0; i < users.Count; i++)
                        {
                            Console.WriteLine($"{i + 1}. {users[i]} \n");
                        }

                        int userchoice;
                        while (!int.TryParse(Console.ReadLine(), out userchoice)
                            || userchoice <= 0
                            || userchoice > users.Count)
                        {
                            Console.WriteLine("Invalid Option Selected. Please try again");
                        }

                        Console.WriteLine("Enter message to send");
                        string message = Console.ReadLine() ?? "";

                        notificationService.SendNotification(new SmsNotification(), message, users[userchoice - 1]);
                        break;
                    }

                    case 8:
                    {
                        notificationService.PrintHistory();
                        break;
                    }

                    case 9:
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

        static void Main(string[] args)
        {   
            
            new Program().NotificationSender();
        }
    }
}
