using LibraryManagementSystem.Interfaces;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories;

namespace LibraryManagementSystem.Services;

public class MemberService : IMemberService
{
    private readonly IMemberRepository _memberRepository;
    private readonly IUserRepository _userRepository;

    public MemberService()
    {
        _memberRepository = new MemberRepository();
        _userRepository = new UserRepository();
    }

    public void AddMember(Member member)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(
                member.FullName))
            {
                throw new Exception(
                    "Member name cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(
                member.Email))
            {
                throw new Exception(
                    "Email cannot be empty");
            }

            member.Email = member.Email.Trim();

            if (string.IsNullOrWhiteSpace(
                member.PhoneNumber))
            {
                throw new Exception(
                    "Phone number cannot be empty");
            }

            member.PhoneNumber = member.PhoneNumber.Trim();

            User? existingUser =
                _userRepository
                .GetUserByUsername(member.Email);

            if (existingUser != null)
            {
                throw new Exception(
                    "Email is already used by a login account");
            }

            Member? existingEmail =
                _memberRepository
                .GetMemberByEmail(member.Email);

            if (existingEmail != null)
            {
                throw new Exception(
                    "Email already exists");
            }

            Member? existingPhone =
                _memberRepository
                .GetMemberByPhone(
                    member.PhoneNumber);

            if (existingPhone != null)
            {
                throw new Exception(
                    "Phone number already exists");
            }

            _memberRepository.AddMember(member);
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
            return _memberRepository
                .GetAllMembers();
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
            return _memberRepository
                .GetInactiveMembers();
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while fetching inactive members: "
                + ex.Message);
        }
    }

    public List<MembershipType> GetAllMembershipTypes()
    {
        try
        {
            return _memberRepository
                .GetAllMembershipTypes();
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while fetching membership types: "
                + ex.Message);
        }
    }

    public Member? GetMemberById(int memberId)
    {
        try
        {
            return _memberRepository
                .GetMemberById(memberId);
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while fetching member: "
                + ex.Message);
        }
    }

    public List<Member>
        SearchMembersByEmail(
            string email)
    {
        return _memberRepository
            .SearchMembersByEmail(
                email);
    }

    public List<Member>
        SearchMembersByPhoneNumber(
            string phoneNumber)
    {
        return _memberRepository
            .SearchMembersByPhoneNumber(
                phoneNumber);
    }

    public void DeactivateMember(int memberId)
    {
        try
        {
            Member? member =
                _memberRepository
                .GetMemberById(memberId);

            if (member == null)
            {
                throw new Exception(
                    "Member not found");
            }

            _memberRepository
                .DeactivateMember(memberId);
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while deactivating member: "
                + ex.Message);
        }
    }

    public void ActivateMember(int memberId)
    {
        try
        {
            Member? member =
                _memberRepository
                .GetMemberById(memberId);

            if (member == null)
            {
                throw new Exception(
                    "Member not found");
            }

            if (member.IsActive)
            {
                throw new Exception(
                    "Member is already active");
            }

            member.IsActive = true;

            _memberRepository
                .UpdateMember(member);
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Error while activating member: "
                + ex.Message);
        }
    }

    public void UpdateMemberMembership(
    int memberId,
    int membershipTypeId)
    {
        Member? member =
            _memberRepository
            .GetMemberById(memberId);

        if (member == null)
        {
            throw new Exception(
                "Member not found");
        }

       List<MembershipType> membershipTypes =
        _memberRepository
        .GetAllMembershipTypes();

        MembershipType? membershipType =
        membershipTypes.FirstOrDefault(
        mt => mt.MembershipTypeId ==
              membershipTypeId);

        if (membershipType == null)
        {
            throw new Exception(
                "Membership type not found");
        }

        if (member.MembershipTypeId ==
            membershipTypeId)
        {
            throw new Exception(
                "Member already has this membership type");
        }

        member.MembershipTypeId =
            membershipTypeId;

        _memberRepository
            .UpdateMember(member);
    }
}
