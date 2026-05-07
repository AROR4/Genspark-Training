using NotificationSystem.Interfaces;
using NotificationSystem.Models;
using NotificationSystem.Services;

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
            while (Choice != 7)
            {
                Console.WriteLine("Please Select any of the Services");
                Console.WriteLine("1. Add User");
                Console.WriteLine("2. Send Email Notification to User");
                Console.WriteLine("3. Send Sms Notification to User");
                Console.WriteLine("4. Notification History");
                Console.WriteLine("5. Exit");
                while(!int.TryParse(Console.ReadLine(), out Choice) && Choice>0 && Choice < 8)
                {
                    Console.WriteLine("Invalid Option Selected. Please try again");
                }

                if (Choice == 1)
                {
                    var user=userService.AddUser();
                    Console.WriteLine("User Added Successfully");
                    Console.WriteLine(user);

                }
                if(Choice == 2)
                {
                    List<User> users=userService.GetAllUsers();
                    if (users.Count == 0)
                    {
                        Console.WriteLine("No user added yet. Please add user first ");
                        continue;

                    }
                    Console.WriteLine("Select User");
                    for(int i = 0; i < users.Count; i++)
                    {
                        Console.Write(i+1 + ". ");
                        Console.WriteLine(users[i]);
                    }
                    int userchoice;
                    while(!int.TryParse(Console.ReadLine(), out userchoice) && userchoice>0 && userchoice < users.Count+1)
                    {
                    Console.WriteLine("Invalid Option Selected. Please try again");
                    }
                    Console.WriteLine("Enter message to sent");
                    String message=Console.ReadLine()??"";
                    notificationService.SendNotification(new EmailNotification(),message,users[userchoice-1]);
                }

                if(Choice == 3)
                {
                    List<User> users=userService.GetAllUsers();
                    if (users.Count == 0)
                    {
                        Console.WriteLine("No user added yet. Please add user first ");
                        continue;

                    }
                    Console.WriteLine("Select User");
                    for(int i = 0; i < users.Count; i++)
                    {
                        Console.Write(i+1 + ". ");
                        Console.WriteLine(users[i]);
                    }
                    int userchoice;
                    while(!int.TryParse(Console.ReadLine(), out userchoice) && userchoice>0 && userchoice < users.Count+1)
                    {
                    Console.WriteLine("Invalid Option Selected. Please try again");
                    }
                    Console.WriteLine("Enter message to sent");
                    String message=Console.ReadLine()??"";
                    notificationService.SendNotification(new SmsNotification(),message,users[userchoice-1]);
                }
                if(Choice == 4)
                {
                    notificationService.PrintHistory();
                }
                if(Choice == 5)
                {
                    Console.WriteLine("Thanks for using the Service");
                    Console.WriteLine("---------------------------");
                    break;
                }

                
            }
        }

        static void Main(string[] args)
        {   
            
            new Program().NotificationSender();
        }
    }
}
