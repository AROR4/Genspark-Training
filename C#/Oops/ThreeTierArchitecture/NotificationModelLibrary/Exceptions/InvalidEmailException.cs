namespace NotificationModelLibrary.Exceptions
{
    public class InvalidEmailException : Exception
    {
        private string _message;
        public InvalidEmailException()
        {
            _message="Invalid email provided.";
        }

        public InvalidEmailException(string message)
        {
            _message="Invalid email provided. " +message;
        }

        public override string Message => _message;
    }
}