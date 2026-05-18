using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Interfaces;

public interface IAuthService
{
    void Register(User user);

    User? Login(string username, string password);
}