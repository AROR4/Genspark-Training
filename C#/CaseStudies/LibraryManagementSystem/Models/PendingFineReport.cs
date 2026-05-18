namespace LibraryManagementSystem.Models;

public class PendingFineReport
{
    public int MemberId { get; set; }

    public string MemberName { get; set; }
        = string.Empty;

    public decimal PendingFine { get; set; }

    public override string ToString()
    {
        return $"Member Id: {MemberId} | " +
            $"Member: {MemberName} | " +
            $"Pending Fine: {PendingFine:C}";
    }
}
