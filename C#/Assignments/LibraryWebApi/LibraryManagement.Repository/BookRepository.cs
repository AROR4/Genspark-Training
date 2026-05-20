using LibraryManagementSystem.Models;
using LibraryManagementSystem.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Repositories
{
    public class BookRepository : IBookRepository<int, Book>
    {
        private readonly LibraryDbContext _context;
        public BookRepository(LibraryDbContext context)
        {
            _context = context;
        }
        public Book AddBook(Book book)
        {
            try{
                
                _context.Books.Add(book);
                _context.SaveChanges();

                return book;

            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public List<Book> GetAllBooks()
        {
            try{
                return _context.Books.ToList();
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
        }

        public Book? GetBookById(int id)
        {
            try{
                return _context.Books.FirstOrDefault(b => b.BookId == id);
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public List<Book> SearchBooks(string title)
        {
            try
            {
                var searchTitle = title.Trim();

                return _context.Books
                        .Where(b => EF.Functions.ILike(b.Title, $"%{searchTitle}%"))
                        .ToList();
            
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
