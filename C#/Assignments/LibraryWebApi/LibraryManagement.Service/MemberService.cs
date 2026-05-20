using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories;

namespace LibraryManagementSystem.Services
{
    public class MemberService : IMemberService
    {
        private readonly IMemberRepository<int,Member> _memberRepository;

        public MemberService(IMemberRepository<int,Member> memberRepository)
        {
            _memberRepository = memberRepository;
        }

        public Member AddMember(MemberDTO member)
        {
            try
            {
                if(string.IsNullOrEmpty(member.FullName) || string.IsNullOrEmpty(member.Email) || member.PhoneNumber.Count()!=10)
                {
                    throw new ArgumentException("Member must have a name, email, and a 10-digit phone number.");
                }
                Member memberToAdd = new Member
                {
                    FullName = member.FullName,
                    Email = member.Email,
                    PhoneNumber = member.PhoneNumber
                };
                return _memberRepository.AddMember(memberToAdd);
            }
            catch(Exception ex)
            {
                throw new Exception($"Error adding member: {ex.Message}");
            }
        }

        public List<Member> GetAllMembers()
        {
            try
            {
                return _memberRepository.GetAllMembers();
            }
            catch(Exception ex)
            {
                throw new Exception($"Error retrieving members: {ex.Message}");
            }
        }

        public Member GetMemberById(int id)
        {
            try
            {
                var member = _memberRepository.GetMemberById(id);
                return member;
            }
            catch(Exception ex)
            {
                throw new Exception($"Error retrieving member: {ex.Message}");
            }
        }
    }
}