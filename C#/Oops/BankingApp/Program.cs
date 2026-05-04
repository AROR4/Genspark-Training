using BankingApp.Interfaces;
using BankingApp.Services;

namespace BankingApp
{
    internal class Program
    {
        ICustomerInteract customerInteract;
        public Program()
        {
            customerInteract = new CustomerService();
        }
        void DoBanking()
        {
            Console.WriteLine("---------Welcome to Nxtgen Banking Systems---------");
            int Choice=1;
            while (Choice != 3)
            {
                Console.WriteLine("Please Select any of the Services");
                Console.WriteLine("1. Open a New Account");
                Console.WriteLine("2. View Account Details ");
                Console.WriteLine("3. Exit");
                while(!int.TryParse(Console.ReadLine(), out Choice) && Choice>0 && Choice < 4)
                {
                    Console.WriteLine("Invalid Option Selected. Please try again");
                }

                if (Choice == 1)
                {

                    var account = customerInteract.OpensAccount();
                    Console.WriteLine("Account Created Successfully . Here are Account Details");
                    Console.WriteLine(account);
                }
                if (Choice == 2)
                {
                    Console.WriteLine("Find Account using ....");
                    Console.WriteLine("1. Account Number \t 2. Phone Number");
                    int keychoice;
                    while(!int.TryParse(Console.ReadLine(),out keychoice) && keychoice>0 && keychoice<3)
                    {
                        Console.WriteLine("Invalid Entry. Please try Again");
                    }
                    if (keychoice == 1)
                    {
                        Console.WriteLine("Please enter the account number below ....");
                        string accNum = Console.ReadLine()??"";
                        customerInteract.PrintAccountDetails(accNum);
                    }
                    if (keychoice == 2)
                    {
                        Console.WriteLine("Please enter the Phone number below ....");
                        string Phone = Console.ReadLine()??"";
                        customerInteract.PrintAccountDetailsUsingPhone(Phone);
                    }
                }
                if (Choice == 3)
                {
                    break;
                }
            }   
        }
        
        static void Main(string[] args)
        {
            new Program().DoBanking();
        }
    }
}