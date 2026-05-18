using System.Collections.Generic;

namespace LibraryManagementSystem.Models;

public class Member
{
    public int MemberId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string? Address { get; set; }

    public int MembershipTypeId { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Borrowing> Borrowings { get; set; } = new List<Borrowing>();

    public MembershipType MembershipType { get; set; } = null!;
    
    public User? User { get; set; }

    public override string ToString()
    {
        string membershipType =
            MembershipType?.Name ?? MembershipTypeId.ToString();

        string loginUsername =
            User?.Username ?? "N/A";

        return $"Id: {MemberId} | Name: {FullName} \n " +
            $"Email: {Email} | Phone: {PhoneNumber} \n " +
            $"Address: {Address ?? "N/A"} \n  " +
            $"Membership: {membershipType} \n  " +
            $"Login Username: {loginUsername} \n " +
            $"Active: {IsActive}";
    }
}
