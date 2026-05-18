using System.Collections.Generic;

namespace LibraryManagementSystem.Models;

public class MembershipType
{
    public int MembershipTypeId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int MaxBorrowLimit { get; set; }

    public int MaxBorrowDays { get; set; }

    public ICollection<Member> Members { get; set; } = new List<Member>();

    public override string ToString()
    {
        return $"Id: {MembershipTypeId} | Name: {Name} | " +
            $"Borrow Limit: {MaxBorrowLimit} | " +
            $"Borrow Days: {MaxBorrowDays}";
    }
}
