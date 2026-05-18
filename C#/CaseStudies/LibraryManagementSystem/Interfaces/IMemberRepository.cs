using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Interfaces;

public interface IMemberRepository
{
   void AddMember(Member member);

    List<Member> GetAllMembers();

    List<Member> GetInactiveMembers();

    List<MembershipType> GetAllMembershipTypes();

    Member? GetMemberById(int memberId);

    Member? GetMemberByEmail(string email);

    Member? GetMemberByPhone(string phoneNumber);

    void UpdateMember(Member member);

    void DeactivateMember(int memberId);

    List<Member> SearchMembersByEmail(
    string email);

    List<Member> SearchMembersByPhoneNumber(
    string phoneNumber);
    
}
