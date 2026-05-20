using LibraryManagementSystem.Models;
namespace LibraryManagementSystem.Repositories
{
    public interface IBookRepository<K,T> where T:class
    {

        T AddBook(T book);
        List<T> GetAllBooks();
        T? GetBookById(K id);
        List<T> SearchBooks(string title);
 
    }
}
