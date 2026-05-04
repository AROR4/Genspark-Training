using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankingApp.Models;

namespace BankingApp.Interfaces
{
    internal interface ICustomerInteract
    {
        public Account OpensAccount();
        public void PrintAccountDetails(string accountNumber);

        public void PrintAccountDetailsUsingPhone(string phoneNumber);

    }
}
