using LibraryManagementSystem.Interfaces;
using LibraryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Repositories;

public class UserRepository : IUserRepository
{
    private readonly LibraryDbContext _context;

    public UserRepository(LibraryDbContext context)
    {
        _context = context;
    }

    public void AddUser(User user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();
    }

    public User? GetUserByUsername(string username)
    {
        return _context.Users
            .Include(u => u.Member)
            .FirstOrDefault(
                u => u.Username.ToLower() ==
                     username.Trim().ToLower());
    }

    public User? GetUserByMemberId(int memberId)
    {
        return _context.Users
            .FirstOrDefault(
                u => u.MemberId == memberId);
    }
}
