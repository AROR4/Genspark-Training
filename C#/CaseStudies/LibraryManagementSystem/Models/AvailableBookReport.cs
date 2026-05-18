namespace LibraryManagementSystem.Models;

public class AvailableBooksReport
{
    public string CategoryName { get; set; }
        = string.Empty;

    public int BookId { get; set; }

    public string Title { get; set; }
        = string.Empty;

    public string Author { get; set; }
        = string.Empty;

    public long AvailableCopies { get; set; }

    public override string ToString()
    {
        return $"Category: {CategoryName} | Book Id: {BookId} | " +
            $"Title: {Title} | Author: {Author} | " +
            $"Available Copies: {AvailableCopies}";
    }
}
