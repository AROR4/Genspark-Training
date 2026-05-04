using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Models
{
    internal class CurrentAccount : Account
    {
        public CurrentAccount()
        {
            AccountType = AccType.CurrentAccount;
            Balance = 0.0f;
        }
       
    }
}