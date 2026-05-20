namespace LibraryManagementSystem.Models
{
    public class Book
    {
        public int BookId { get; set; }
        public string Title { get; set; }=String.Empty;
        public string Author { get; set; }=String.Empty;
        public string ISBN { get; set; }=String.Empty;
        public int PublicationYear { get; set; }
        public int AvailableCopies { get; set; }
    }
}