
using NotificationModelLibrary.Exceptions;
using System.Text.RegularExpressions;

namespace NotificationBLLibrary.Validators
{
    public static class UserValidator
    {
public static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name cannot be empty.");
            }
        }

        public static void ValidateEmail(string email, string previous = "")
        {
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            if (string.IsNullOrWhiteSpace(email) || !Regex.IsMatch(email, pattern))
            {
                throw new InvalidEmailException("Invalid email format");
            }

            if (email == previous)
            {
                throw new InvalidEmailException("Please enter email different from old email");
            }
        }

        public static void ValidatePhone(string phone, string previous = "")
        {
            if (string.IsNullOrWhiteSpace(phone) || phone.Length != 10 || !phone.All(char.IsDigit))
            {
                throw new InvalidPhoneException("Phone must be exactly 10 digits.");
            }
            if (phone == previous)
            {
                throw new InvalidPhoneException("Please add number different from old phone number");
            }
        }
    }
}