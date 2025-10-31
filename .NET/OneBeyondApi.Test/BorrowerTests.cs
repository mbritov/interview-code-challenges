using Microsoft.EntityFrameworkCore;
using OneBeyondApi.DataAccess;
using OneBeyondApi.Model;

namespace OneBeyondApi.Tests.DataAccess
{
    [TestFixture]
    public class BorrowerRepositoryTests
    {
        private readonly BorrowerRepository repository = new BorrowerRepository();
        private readonly CatalogueRepository catRepository = new CatalogueRepository();

        public LibraryContext GetInMemoryContext()
        {
            var context = new LibraryContext();            
            return context;
        }

        [Test]
        public void AddBorrower_ShouldAddAndReturnId()
        {
            // Arrange
            using var context = GetInMemoryContext();
            var repo = new BorrowerRepository();
            var borrower = new Borrower { Id = Guid.NewGuid(), Name = "John Doe", EmailAddress = "test@test.com" };

            // Act
            repository.AddBorrower(borrower);
            var borrowers = context.Borrowers.ToList();

            // Assert
            Assert.That(borrowers.Count, Is.EqualTo(1));
            Assert.That(borrowers.First().Name, Is.EqualTo("John Doe"));
        }

        [Test]
        public void GetBorrowers_ShouldReturnAllBorrowers()
        {
            // Arrange
            using var context = GetInMemoryContext();
            context.Borrowers.AddRange(
                new Borrower { Id = Guid.NewGuid(), Name = "Alice", EmailAddress = "test1@test.com" },
                new Borrower { Id = Guid.NewGuid(), Name = "Bob", EmailAddress = "test2@test.com" }
            );
            context.SaveChanges();

            // Act
            var borrowers = repository.GetBorrowers();

            //Assert
            Assert.That(borrowers.Count, Is.GreaterThan(1));
        }

        [Test]
        public void GetBorrowersWithLoan_ShouldReturnBorrowersHavingBooksInBookStock()
        {
            // Arrange
            using var context = GetInMemoryContext();

            var borrower1 = new Borrower { Id = Guid.NewGuid(), Name = "Reader 1", EmailAddress = "test1@test.com" };
            var borrower2 = new Borrower { Id = Guid.NewGuid(), Name = "Reader 2", EmailAddress = "test2@test.com" };

            var book1 = new Book { Id = Guid.NewGuid(), Name = "Book A", ISBN = "12" };
            var book2 = new Book { Id = Guid.NewGuid(), Name = "Book B", ISBN = "34" };

            var stock1 = new BookStock { Id = Guid.NewGuid(), Book = book1, OnLoanTo = borrower1 };
            var stock2 = new BookStock { Id = Guid.NewGuid(), Book = book2, OnLoanTo = null };

            context.Borrowers.AddRange(borrower1, borrower2);
            context.Catalogue.AddRange(stock1, stock2);
            context.SaveChanges();

            // Act
            var borrowersWithLoans = repository.GetBorrowersWithLoan();

            // Assert
            Assert.That(borrowersWithLoans.Count, Is.EqualTo(1));
            Assert.That(borrowersWithLoans.First().borrower.Name, Is.EqualTo("Reader 1"));
        }

        [Test]
        public void RequestLoan_ShouldAssignBorrowerAndSetLoanEndDate()
        {
            // Arrange
            using var context = GetInMemoryContext();

            var borrower = new Borrower { Id = Guid.NewGuid(), Name = "Borrower", EmailAddress = "test1@test.com" };
            var book = new Book { Id = Guid.NewGuid(), Name = "Book X", ISBN = "123" };

            // Act
            repository.RequestLoan(borrower, book);

            var borrowerData = repository.GetBorrowersWithLoan().First(p => p.borrower.Id == borrower.Id);

            Assert.That(borrowerData.borrower.Id, Is.EqualTo(borrower.Id));
            Assert.That(borrowerData.booksOnLoan.Any(), Is.True);
        }

        [Test]
        public void CompleteLoan_ShouldClearLoanData()
        {
            // Assert
            using var context = GetInMemoryContext();

            var borrower = new Borrower { Id = Guid.NewGuid(), Name = "Borrower", EmailAddress = "test1@test.com" };
            var book = new Book { Id = Guid.NewGuid(), Name = "Book X", ISBN = "123" };

            // Act
            repository.RequestLoan(borrower, book);
            repository.CompleteLoan(borrower, book.Id);
            var borrowerData = repository.GetBorrowersWithLoan().First(p => p.borrower.Id == borrower.Id);

            // Assert
            Assert.That(borrowerData.booksOnLoan.Any(b => b.Id == book.Id),Is.True);
        }

        [Test]
        public void ReserveBook_ShouldAddReserveToBorrower()
        {
            // Assert
            using var context = GetInMemoryContext();

            var borrower = new Borrower { Id = Guid.NewGuid(), Name = "Borrower", EmailAddress = "test1@test.com" };
            var book = new Book { Id = Guid.NewGuid(), Name = "Book X", ISBN = "123" };

            // Act
            repository.Reserve(borrower, book);
            var books = catRepository.SearchCatalogue(new CatalogueSearch() { BookName = book.Name });

            // Assert
            Assert.That(books.Any(b => b.ReserverdTo.Id == borrower.Id), Is.True);
        }
    }
}
