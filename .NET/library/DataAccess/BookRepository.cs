using OneBeyondApi.Model;

namespace OneBeyondApi.DataAccess
{
    public class BookRepository : IBookRepository
    {
        private readonly LibraryContext _context;

        public BookRepository(LibraryContext context)
        {
            _context = context;
        }

        public List<Book> GetBooks()
        {
            var list = _context.Books.ToList();
            return list;
        }

        public Guid AddBook(Book book)
        {
            _context.Books.Add(book);
            _context.SaveChanges();
            return book.Id;
        }
    }
}
