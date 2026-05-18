namespace LibraryManagementSystem.Exceptions;

public class BorrowLimitExceededException : Exception
{
    public BorrowLimitExceededException(string message)
        : base(message)
    {
    }
}