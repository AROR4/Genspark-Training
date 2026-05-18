using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Interfaces;

public interface IUserRepository
{
    void AddUser(User user);

    User? GetUserByUsername(string username);

    User? GetUserByMemberId(int memberId);
}
