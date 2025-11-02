using OneBeyondApi.Model;

namespace OneBeyondApi.DataAccess
{
    public enum OperationResult
    {
        Success,
        BookNotFound,
        BookAlreadyOnLoan,
        BookNotAvailable,
        BookAlreadyReserved,
        BookAvailable
    }

    public class BorrowerRepository : IBorrowerRepository
    {
        private readonly LibraryContext _context;

        public BorrowerRepository(LibraryContext context)
        {
            _context = context;
        }

        public List<Borrower> GetBorrowers()
        {
            var list = _context.Borrowers
                .ToList();
            return list;
        }

        // todo - add paging
        public List<BorrowerData> GetBorrowersWithLoan()
        {
            var borrowersWithLoans = _context.Catalogue
                .Where(c => c.OnLoanTo != null) // only loaned books
                .GroupBy(c => c.OnLoanTo)
                .Select(g => new BorrowerData
                {
                    Borrower = g.Key!,
                    BooksOnLoan = g.Select(c => c.Book).ToList()
                })
                .ToList();

            return borrowersWithLoans;
        }

        public Guid AddBorrower(Borrower borrower)
        {
            _context.Borrowers.Add(borrower);
            _context.SaveChanges();
            return borrower.Id;
        }

        public int RequestLoan(Borrower borrower, Book book)
        {
            var bookInStock = _context.Catalogue.FirstOrDefault(p => p.Book.Id == book.Id);
            if (bookInStock == null)
            {
                return (int)OperationResult.BookNotFound;
            }

            if (bookInStock.OnLoanTo == null)
            {
                bookInStock.OnLoanTo = borrower;
                bookInStock.LoanEndDate = DateTime.UtcNow.AddDays(14); // make it configurable
            }
            else
            {
                return (int)OperationResult.BookAlreadyOnLoan;
            }

            _context.SaveChanges();

            return (int)OperationResult.Success;
        }

        public int CompleteLoan(Borrower borrower, Guid bookId)
        {
            // check if book is available
            var bookInStock = _context.Catalogue.FirstOrDefault(p => p.Book.Id == bookId);
            if (bookInStock != null)
            {
                // check if the book is returned after allowed date
                if (bookInStock.LoanEndDate < DateTime.UtcNow)
                {
                    var daysOverdue = (DateTime.UtcNow - bookInStock.LoanEndDate.Value).Days;
                    var fineAmount = daysOverdue * 1.0m; // todo - make fine amount configurable

                    _context.Fines.Add(new Fine { 
                        Amount = fineAmount,
                        BorrowerId = borrower.Id,
                        Reason = $"Book was returned {daysOverdue} days later"
                    });
                }

                bookInStock.OnLoanTo = null;
                bookInStock.LoanEndDate = null;
            }

            _context.SaveChanges();

            return (int)OperationResult.Success;
        }

        public int Reserve(Borrower borrower, Book book)
        {
            var bookInStock = _context.Catalogue.FirstOrDefault(p => p.Book.Id == book.Id);
            if (bookInStock == null)
            {
                return (int)OperationResult.BookNotFound;
            }

            if (bookInStock.OnLoanTo == null)
            {
                return (int)OperationResult.BookAvailable;
            }

            bool isReserved = _context.Reservations
                .Any(r => r.BorrowerId == borrower.Id && r.BookId == book.Id);

            if (isReserved)
                return (int)OperationResult.BookAlreadyReserved;

            _context.Reservations.Add(new Reservation
            {
                BorrowerId = borrower.Id,
                BookId = book.Id,
                CreatedAt = DateTime.UtcNow
            });

            _context.SaveChanges();

            return (int)OperationResult.Success;
        }
    }
}
