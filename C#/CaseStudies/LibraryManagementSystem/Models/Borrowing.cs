using System;

namespace LibraryManagementSystem.Models;

public class Borrowing
{
    public int BorrowingId { get; set; }

    public int MemberId { get; set; }

    public int BookCopyId { get; set; }

    public DateTime BorrowDate { get; set; } = DateTime.Now;

    public DateTime DueDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public int OldDamagePercentage { get; set; }

    public int? NewDamagePercentage { get; set; }

    public decimal FineAmount { get; set; }

    public BookCopy BookCopy { get; set; } = null!;

    public Fine? Fine { get; set; }

    public Member Member { get; set; } = null!;

    public override string ToString()
    {
        string memberName =
            Member?.FullName ?? MemberId.ToString();

        string bookTitle =
            BookCopy?.Book?.Title ??
            BookCopyId.ToString();

        string serialNumber =
            BookCopy?.SerialNumber ?? "N/A";

        string newDamagePercentage =
            NewDamagePercentage?.ToString() ?? "N/A";

        return $"Id: {BorrowingId} \n Member: {memberName} \n " +
            $"Book: {bookTitle} | Copy Serial: {serialNumber} \n " +
            $"Old Damage: {OldDamagePercentage}% | " +
            $"New Damage: {newDamagePercentage}% | " +
            $"Borrowed On: {BorrowDate:d} \n " +
            $"Due On: {DueDate:d} \n " +
            $"Returned On: {ReturnDate?.ToString("d") ?? "Not returned"} \n " +
            $"Status: {Status} \n Fine: {FineAmount:C}";
    }
}
