using System.Collections.Generic;

namespace LibraryManagementSystem.Models;

public class Category
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ICollection<Book> Books { get; set; } = new List<Book>();

    public override string ToString()
    {
        return $"Id: {CategoryId} | Name: {CategoryName} | " +
            $"Description: {Description ?? "N/A"}";
    }
}
