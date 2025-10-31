using OneBeyondApi.Model;
using System.Net;
namespace OneBeyondApi.DataAccess
{
    public interface IBorrowerRepository
    {
        public List<Borrower> GetBorrowers();

        public List<BorrowerData> GetBorrowersWithLoan();

        public Guid AddBorrower(Borrower borrower);

        public int RequestLoan(Borrower borrower, Book book);

        public int CompleteLoan(Borrower borrower, Guid bookId);

        public int Reserve(Borrower borrower, Book book);
    }
}
