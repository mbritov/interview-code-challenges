using Microsoft.EntityFrameworkCore;
using OneBeyondApi.DataAccess;
using OneBeyondApi.Model;

namespace OneBeyondApi.Tests.DataAccess
{
    [TestFixture]
    public class BorrowerRepositoryTests
    {
        private readonly BorrowerRepository borrowerRepository;
        private readonly CatalogueRepository catRepository;
        private readonly LibraryContext context;

        public BorrowerRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<LibraryContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;

            context = new LibraryContext(options);
            borrowerRepository = new BorrowerRepository(context);
            catRepository = new CatalogueRepository(context);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            context.Dispose();
        }


        [Test]
        public void AddBorrower_ShouldAddAndReturnId()
        {
            // Arrange
            var borrower = new Borrower { Id = Guid.NewGuid(), Name = "John Doe", EmailAddress = "test@test.com" };

            // Act
            borrowerRepository.AddBorrower(borrower);
            var borrowers = context.Borrowers.ToList();

            // Assert
            Assert.That(borrowers.Count, Is.EqualTo(1));
            Assert.That(borrowers.First().Name, Is.EqualTo("John Doe"));
        }

        [Test]
        public void GetBorrowers_ShouldReturnAllBorrowers()
        {
            // Arrange
            context.Borrowers.AddRange(
                new Borrower { Id = Guid.NewGuid(), Name = "Alice", EmailAddress = "test1@test.com" },
                new Borrower { Id = Guid.NewGuid(), Name = "Bob", EmailAddress = "test2@test.com" }
            );
            context.SaveChanges();

            // Act
            var borrowers = borrowerRepository.GetBorrowers();

            //Assert
            Assert.That(borrowers.Count, Is.GreaterThan(1));
        }

        [Test]
        public void GetBorrowersWithLoan_ShouldReturnBorrowersHavingBooksInBookStock()
        {
            // Arrange
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
            var borrowersWithLoans = borrowerRepository.GetBorrowersWithLoan();

            // Assert
            Assert.That(borrowersWithLoans.Count, Is.EqualTo(1));
            Assert.That(borrowersWithLoans.First().Borrower.Name, Is.EqualTo("Reader 1"));
        }

        [Test]
        public void RequestLoan_ShouldAssignBorrowerAndSetLoanEndDate()
        {
            // Arrange
            var borrower = new Borrower { Id = Guid.NewGuid(), Name = "Borrower", EmailAddress = "test1@test.com" };
            var book = new Book { Id = Guid.NewGuid(), Name = "Book X", ISBN = "123" };
            var bookOnLoan = new BookStock
            {
                Book = book,
                OnLoanTo = null,
                LoanEndDate = null
            };
            context.Borrowers.Add(borrower);
            context.Books.Add(book);
            context.Catalogue.Add(bookOnLoan);
            context.SaveChanges();

            // Act
            borrowerRepository.RequestLoan(borrower, book);

            var borrowerData = borrowerRepository.GetBorrowersWithLoan().First(p => p.Borrower.Id == borrower.Id);

            // Assert
            Assert.That(borrowerData.Borrower.Id, Is.EqualTo(borrower.Id));
            Assert.That(borrowerData.BooksOnLoan.Any(), Is.True);
        }

        [Test]
        public void CompleteLoan_ShouldClearLoanData()
        {
            // Assert
            var borrower = new Borrower { Id = Guid.NewGuid(), Name = "Borrower", EmailAddress = "test1@test.com" };
            var book = new Book { Id = Guid.NewGuid(), Name = "Book X", ISBN = "123" };
            var bookOnLoan = new BookStock
            {
                Book = book,
                OnLoanTo = borrower,
                LoanEndDate = DateTime.Now.Date.AddDays(7)
            };
            context.Borrowers.Add(borrower);
            context.Books.Add(book);
            context.Catalogue.Add(bookOnLoan);
            context.SaveChanges();

            // Act
            borrowerRepository.CompleteLoan(borrower, book.Id);
            var borrowerData = borrowerRepository.GetBorrowersWithLoan().FirstOrDefault(p => p.Borrower.Id == borrower.Id);

            // Assert
            Assert.That(borrowerData, Is.Null);
        }

        [Test]
        public void ReserveBook_ShouldAddReserveToBorrower()
        {
            // Assert
            var borrower = new Borrower { Id = Guid.NewGuid(), Name = "Borrower", EmailAddress = "test1@test.com" };
            var book = new Book { Id = Guid.NewGuid(), Name = "Book X", ISBN = "123" };
            var bookOnLoan = new BookStock
            {
                Book = book,
                OnLoanTo = borrower,
                LoanEndDate = DateTime.Now.Date.AddDays(7)
            };
            context.Borrowers.Add(borrower);
            context.Books.Add(book);
            context.Catalogue.Add(bookOnLoan);
            context.SaveChanges();

            // Act
            borrowerRepository.Reserve(borrower, book);
            var books = catRepository.SearchReservations( book.Id, borrower.Id);

            // Assert
            Assert.That(books.Any(b => b.BorrowerId == borrower.Id), Is.True);
        }
    }
}
