namespace LibraryManagementSystem.Exceptions;

public class DuplicateBorrowingException : Exception
{
    public DuplicateBorrowingException(string message)
        : base(message)
    {
    }
}