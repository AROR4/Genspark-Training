namespace LibraryManagementSystem.Models;

public class MostBorrowedBooksReport
{
    public int BookId { get; set; }

    public string Title { get; set; }
        = string.Empty;

    public string Author { get; set; }
        = string.Empty;

    public long BorrowCount { get; set; }

    public override string ToString()
    {
        return $"Book Id: {BookId} | Title: {Title} | " +
            $"Author: {Author} | Borrow Count: {BorrowCount}";
    }
}
