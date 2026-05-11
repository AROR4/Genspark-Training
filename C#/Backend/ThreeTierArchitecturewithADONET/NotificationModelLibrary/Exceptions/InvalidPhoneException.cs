namespace NotificationModelLibrary.Exceptions
{
    public class InvalidPhoneException : Exception
    {
        private string _message;
        public InvalidPhoneException()
        {
            _message="Invalid phone number provided. ";
        }

        public InvalidPhoneException(string message)
        {
            _message="Invalid phone number provided. " + message;
        }
        public override string Message => _message;
    }
}