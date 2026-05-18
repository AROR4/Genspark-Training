namespace LibraryManagementSystem.Models;

public class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public int? MemberId { get; set; }

    public Member? Member { get; set; }

    public override string ToString()
    {
        string member =
            Member?.FullName ??
            MemberId?.ToString() ??
            "N/A";

        return $"Id: {UserId} | Username: {Username} | " +
            $"Role: {Role} | Member: {member}";
    }
}
