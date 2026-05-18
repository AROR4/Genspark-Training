using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Interfaces;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Repositories;

public class MemberRepository : IMemberRepository
{
    private readonly LibraryDbContext _context;

    public MemberRepository()
    {
        _context = new LibraryDbContext();
    }

    public void AddMember(Member member)
    {
        try
        {
            _context.Members.Add(member);
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while adding member: "
                + ex.Message);
        }
    }

    public List<Member> GetAllMembers()
    {
        try
        {
            return _context.Members
                .Include(m => m.MembershipType)
                .Where(m => m.IsActive)
                .ToList();
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while fetching members: "
                + ex.Message);
        }
    }

    public List<Member> GetInactiveMembers()
    {
        try
        {
            return _context.Members
                .Include(m => m.MembershipType)
                .Where(m => !m.IsActive)
                .ToList();
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while fetching inactive members: "
                + ex.Message);
        }
    }

    public Member? GetMemberById(int memberId)
    {
        try
        {
            return _context.Members
                .Include(m => m.MembershipType)
                .FirstOrDefault(
                    m => m.MemberId == memberId);
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while fetching member: "
                + ex.Message);
        }
    }

    public Member? GetMemberByEmail(string email)
    {
        try
        {
            return _context.Members
                .Include(m => m.MembershipType)
                .FirstOrDefault(
                    m => m.Email.ToLower() ==
                         email.ToLower());
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while searching member by email: "
                + ex.Message);
        }
    }

    public Member? GetMemberByPhone(
        string phoneNumber)
    {
        try
        {
            return _context.Members
                .Include(m => m.MembershipType)
                .FirstOrDefault(
                    m => m.PhoneNumber == phoneNumber);
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while searching member by phone: "
                + ex.Message);
        }
    }

    public List<MembershipType>
    GetAllMembershipTypes()
    {
        try
        {
            return _context.MembershipTypes
                .ToList();
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while fetching membership types: "
                + ex.Message);
        }
    }
    public void UpdateMember(Member member)
    {
        try
        {
            _context.Members.Update(member);
            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while updating member: "
                + ex.Message);
        }
    }

    public void DeactivateMember(int memberId)
    {
        try
        {
            Member? member = _context.Members
                .FirstOrDefault(
                    m => m.MemberId == memberId);

            if (member == null)
            {
                throw new Exception(
                    "Member not found");
            }

            member.IsActive = false;

            _context.SaveChanges();
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while deactivating member: "
                + ex.Message);
        }
    }

    public List<Member>
    SearchMembersByEmail(
        string email)
    {
        return _context.Members
            .Include(m => m.MembershipType)
            .Where(m =>
                m.Email.ToLower()
                .Contains(email.ToLower()))
            .ToList();
    }

    public List<Member>
    SearchMembersByPhoneNumber(
        string phoneNumber)
    {
        return _context.Members
            .Include(m => m.MembershipType)
            .Where(m =>
                m.PhoneNumber.Contains(
                    phoneNumber))
            .ToList();
    }


}
