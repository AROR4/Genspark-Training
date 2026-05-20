
namespace LibraryManagementSystem.Repositories
{
    public interface IMemberRepository<K,T> where T:class
    {

        T AddMember(T Member);
        List<T> GetAllMembers();
        T GetMemberById(K id);
 
    }
}