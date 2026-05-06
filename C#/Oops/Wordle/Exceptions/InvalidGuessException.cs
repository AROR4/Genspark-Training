namespace Wordle.Exceptions
{
    public class InvalidGuessException : Exception
    {
        private string _message;
        public InvalidGuessException()
        {
            _message = "You have entered wrong format of Input. Please enter only letters .!!!";
        }
        public InvalidGuessException(string message)
        {
            _message =" Please enter again." + message;
        }

        public override string Message => _message;
    }
}