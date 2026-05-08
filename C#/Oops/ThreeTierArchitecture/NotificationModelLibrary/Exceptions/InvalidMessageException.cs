namespace NotificationModelLibrary.Exceptions
{
    public class InvalidMessageException : Exception
    {
        private string _message;
        public InvalidMessageException()
        {
            _message="Invalid message provided.";
        }
        public InvalidMessageException(string message)
        {
            _message = "Invalid message provided: " + message;
        }
        public override string Message => _message;
    }
}