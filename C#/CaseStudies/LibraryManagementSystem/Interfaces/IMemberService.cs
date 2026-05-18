using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Interfaces;

public interface IMemberService
{
    void AddMember(Member member);

    List<Member> GetAllMembers();

    List<Member> GetInactiveMembers();

    List<MembershipType> GetAllMembershipTypes();

    Member? GetMemberById(int memberId);

    void DeactivateMember(int memberId);

    void ActivateMember(int memberId);

    List<Member> SearchMembersByEmail(
        string email);

    List<Member> SearchMembersByPhoneNumber(
        string phoneNumber);

    void UpdateMemberMembership(
    int memberId,
    int membershipTypeId);



}
