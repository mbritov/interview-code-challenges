using OneBeyondApi.Model;
using System.Net;

namespace OneBeyondApi.DataAccess
{
    public class BorrowerRepository : IBorrowerRepository
    {
        public BorrowerRepository()
        {
        }

        public List<Borrower> GetBorrowers()
        {
            using var context = new LibraryContext();
            var list = context.Borrowers
                .ToList();
            return list;
        }

        // todo - add paging
        public List<BorrowerData> GetBorrowersWithLoan()
        {
            using var context = new LibraryContext();

            var borrowersWithLoans = context.Catalogue
                .Where(c => c.OnLoanTo != null) // only loaned books
                .GroupBy(c => c.OnLoanTo)
                .Select(g => new BorrowerData
                {
                    borrower = g.Key!,
                    booksOnLoan = g.Select(c => c.Book).ToList()
                })
                .ToList();

            return borrowersWithLoans;
        }

        public Guid AddBorrower(Borrower borrower)
        {
            using var context = new LibraryContext();
            context.Borrowers.Add(borrower);
            context.SaveChanges();
            return borrower.Id;
        }

        public int RequestLoan(Borrower borrower, Book book)
        {
            using var context = new LibraryContext();

            // check if book is available in stock
            var bookInStock = context.Catalogue.FirstOrDefault(p => p.Book.Id == book.Id);
            if (bookInStock == null)
            {
                // if not add new record to the stock
                context.Catalogue.Add(
                    new BookStock
                    {
                        Id = Guid.NewGuid(), OnLoanTo = borrower, Book = book, LoanEndDate = DateTime.UtcNow.AddDays(14) // make it configurable
                    });
            }
            else if (bookInStock.OnLoanTo == null)
            {
                bookInStock.OnLoanTo = borrower;
                bookInStock.LoanEndDate = DateTime.UtcNow.AddDays(14); // make it configurable
            }

            context.SaveChanges();
            return 0;
        }

        public int CompleteLoan(Borrower borrower, Guid bookId)
        {
            using var context = new LibraryContext();

            // check if book is available
            var bookInStock = context.Catalogue.FirstOrDefault(p => p.Book.Id == bookId);
            if (bookInStock != null)
            {
                // check if the book is returned after allowed date
                if (bookInStock.LoanEndDate < DateTime.UtcNow)
                {
                    // TODO apply penalty for overdue loan
                    borrower.fines.Add(new Fine { Id = Guid.NewGuid(), 
                        Date = DateTime.UtcNow, 
                        Amount = 1.0F  // todo - make fine amount configurable
                    });
                }

                bookInStock.OnLoanTo = null;
                bookInStock.LoanEndDate = null;
            }

            context.SaveChanges();
            return 0;
        }

        public int Reserve(Borrower borrower, Book book)
        {
            using var context = new LibraryContext();
            var bookInStock = context.Catalogue.FirstOrDefault(p => p.Book.Id == book.Id && p.OnLoanTo != null);
            if (bookInStock != null)
            {
                // reserve
                bookInStock.ReserverdTo = borrower;
                // todo - add mutliple reservedTo borrowers
            }
            else
            {
                // if not add new record to the stock
                context.Catalogue.Add(
                    new BookStock
                    {
                        Id = Guid.NewGuid(),
                        Book = book,
                        ReserverdTo = borrower
                    });
            }
            context.SaveChanges();

            return 0;
        }
    }
}
