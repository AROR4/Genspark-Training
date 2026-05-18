using System.Collections.Generic;

namespace LibraryManagementSystem.Models;

public class Book
{
    public int BookId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public string ISBN { get; set; } = string.Empty;

    public int? PublishedYear { get; set; }

    public int CategoryId { get; set; }

    public string? Description { get; set; }

    public bool IsAvailable { get; set; } = true;

    public ICollection<BookCopy> BookCopies { get; set; } = new List<BookCopy>();
    public decimal Price { get; set; }

    public Category Category { get; set; } = null!;

    public override string ToString()
    {
        string category =
            Category?.CategoryName ?? CategoryId.ToString();

        int totalCopies =
            BookCopies?.Count ?? 0;

        int availableCopies =
            BookCopies?.Count(copy => copy.IsAvailable) ?? 0;

        return $"Id: {BookId} | Title: {Title} \n " +
            $"Author: {Author} | ISBN: {ISBN} \n " +
            $"Published Year: {PublishedYear?.ToString() ?? "N/A"} \n " +
            $"Category: {category} | Price: {Price:C} \n " +
            $"Available: {IsAvailable} \n " +
            $"Copies: {availableCopies}/{totalCopies} \n ";
    }
}
