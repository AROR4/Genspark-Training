namespace LibraryManagementSystem.Exceptions;

public class FineLimitExceededException : Exception
{
    public FineLimitExceededException(string message)
        : base(message)
    {
    }
}