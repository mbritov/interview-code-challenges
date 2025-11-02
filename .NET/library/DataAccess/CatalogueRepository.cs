using Microsoft.EntityFrameworkCore;
using OneBeyondApi.Model;

namespace OneBeyondApi.DataAccess
{
    public class CatalogueRepository : ICatalogueRepository
    {
        private readonly LibraryContext _context;

        public CatalogueRepository(LibraryContext context)
        {
            _context = context;
        }
        public List<BookStock> GetCatalogue()
        {
            var list = _context.Catalogue
                .Include(x => x.Book)
                .ThenInclude(x => x.Author)
                .Include(x => x.OnLoanTo)
                .ToList();
            return list;
        }

        public List<BookStock> SearchCatalogue(CatalogueSearch search)
        {
            var list = _context.Catalogue
                .Include(x => x.Book)
                .ThenInclude(x => x.Author)
                .Include(x => x.OnLoanTo)
                .AsQueryable();

            if (search != null)
            {
                if (!string.IsNullOrEmpty(search.Author)) {
                    list = list.Where(x => x.Book.Author.Name.Contains(search.Author));
                }
                if (!string.IsNullOrEmpty(search.BookName)) {
                    list = list.Where(x => x.Book.Name.Contains(search.BookName));
                }
            }
                    
            return list.ToList();
        }

        public List<Reservation> SearchReservations(Guid bookId, Guid borrowerId)
        {
            var list = _context.Reservations
                .AsQueryable();

            list = list.Where(x => x.BorrowerId == borrowerId && x.BookId == bookId);
  
            return list.ToList();
        }
    }
}
