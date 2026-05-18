using System;
using System.Collections.Generic;

namespace LibraryManagementSystem.Models;

public class BookCopy
{
    public int BookCopyId { get; set; }

    public int BookId { get; set; }

    public string SerialNumber { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public bool IsAvailable { get; set; } = true;
    
    public int DamagePercentage { get; set; } = 0;

    public Book Book { get; set; } = null!;

    public ICollection<Borrowing> Borrowings { get; set; } = new List<Borrowing>();

    public override string ToString()
    {
        string bookTitle =
            Book?.Title ?? BookId.ToString();

        return $"Id: {BookCopyId} | Book: {bookTitle} | " +
            $"Serial Number: {SerialNumber} | Status: {Status} | " +
            $"Available: {IsAvailable} | " +
            $"Damage: {DamagePercentage}%";
    }
}
