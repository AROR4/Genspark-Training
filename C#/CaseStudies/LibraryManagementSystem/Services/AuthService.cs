using LibraryManagementSystem.Interfaces;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories;

namespace LibraryManagementSystem.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IMemberRepository _memberRepository;

    public AuthService()
    {
        _userRepository = new UserRepository();
        _memberRepository = new MemberRepository();
    }

    public void Register(User user)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(user.Username))
            {
                throw new Exception("Username cannot be empty");
            }


            if (string.IsNullOrWhiteSpace(user.Password))
            {
                throw new Exception("Password cannot be empty");
            }


            if (string.IsNullOrWhiteSpace(user.Role))
            {
                throw new Exception("Role cannot be empty");
            }

            string role = user.Role.Trim();

            if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                user.Role = "Admin";
                user.MemberId = null;
            }
            else if (role.Equals("User", StringComparison.OrdinalIgnoreCase))
            {
                user.Role = "User";

                if (!user.MemberId.HasValue)
                {
                    throw new Exception("Member Id is required for user role");
                }

                Member? member =
                    _memberRepository.GetMemberById(
                        user.MemberId.Value);

                if (member == null)
                {
                    throw new Exception("Member not found");
                }

                if (!member.IsActive)
                {
                    throw new Exception("Member is inactive");
                }

                User? existingMemberUser =
                    _userRepository.GetUserByMemberId(
                        user.MemberId.Value);

                if (existingMemberUser != null)
                {
                    throw new Exception("Member already has a user account");
                }
            }
            else
            {
                throw new Exception("Role must be Admin or User");
            }

            User? existingUser =
                _userRepository.GetUserByUsername(user.Username);

            if (existingUser != null)
            {
                throw new Exception("Username already exists");
            }

            _userRepository.AddUser(user);
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while registering user: "
                + ex.Message);
        }
    }

    public User? Login(
        string username,
        string password)
    {
        try
        {
            User? user =_userRepository.GetUserByUsername(username.Trim());

            if (user == null)
            {
                throw new Exception("Invalid username");
            }

            if (user.Password != password)
            {
                throw new Exception("Invalid password");
            }

            if (user.Role.ToLower().Equals("user"))
            {
                if (!user.Member!.IsActive)
                {
                    throw new Exception("Member is Not Active. Contact Admin !!");
                }
            }
            return user;
        }
        catch (Exception ex)
        {
            throw new Exception("Login failed: " + ex.Message);
        }
    }
}
