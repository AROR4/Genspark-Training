using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Services
{
    public interface IMemberService
    {
        Member AddMember(MemberDTO member);
        List<Member> GetAllMembers();
        Member GetMemberById(int id);
    }
}