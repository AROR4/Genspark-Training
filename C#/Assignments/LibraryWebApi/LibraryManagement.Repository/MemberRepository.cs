using LibraryManagementSystem.Models;
using LibraryManagementSystem.Contexts;

namespace LibraryManagementSystem.Repositories
{
    public class MemberRepository : IMemberRepository<int, Member>
    {
        private readonly LibraryDbContext _context;
        public MemberRepository(LibraryDbContext context)
        {
            _context = context;
        }
        public Member AddMember(Member member)
        {
            try{
             
                _context.Members.Add(member);
                _context.SaveChanges();

                return member;

            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public List<Member> GetAllMembers()
        {
            try{
                return _context.Members.ToList();
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
        }

        public Member GetMemberById(int id)
        {
            try{
                return _context.Members.FirstOrDefault(m => m.MemberId == id)!;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
