using LibraryManagementSystem.Models;
using LibraryManagementSystem.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LibraryManagementSystem.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository<int, Book> _bookRepository;

        public BookService(IBookRepository<int, Book> bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public Book AddBook(BookDTO book)
        {
            try
            {
                if(string.IsNullOrEmpty(book.Title) || string.IsNullOrEmpty(book.Author) || string.IsNullOrEmpty(book.ISBN))
                {
                    throw new ArgumentException("Book must have a title, author, and ISBN.");
                }
                if(book.PublicationYear <= 0 )
                {
                    throw new ArgumentException("Book must have valid publication year.");
                }
                if(book.AvailableCopies < 0)
                {
                    throw new ArgumentException("Available copies cannot be negative.");
                }
                Book booktoadd=new Book
                {
                    Title=book.Title,
                    Author=book.Author,
                    ISBN=book.ISBN,
                    PublicationYear=book.PublicationYear,
                    AvailableCopies=book.AvailableCopies
                };
                var addedBook = _bookRepository.AddBook(booktoadd);
                return addedBook;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding book: {ex.Message}");
            }
        }

        public List<Book> GetAllBooks()
        {
            try
            {
                var books = _bookRepository.GetAllBooks();
                return books;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving books: {ex.Message}");
            }
        }

        public Book GetBookById(int id)
        {
            try
            {
                var book = _bookRepository.GetBookById(id);
                return book;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving book: {ex.Message}");
            }
        }

        public List<Book> SearchBooks(string title)
        {
            try
            {
                var books = _bookRepository.SearchBooks(title);
                return books;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error searching books: {ex.Message}");
            }
        }
    }
}