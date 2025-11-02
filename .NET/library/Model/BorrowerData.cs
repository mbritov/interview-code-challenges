namespace OneBeyondApi.Model
{
    public class BorrowerData
    {
        public Borrower Borrower { get; set; }
        public List<Book> BooksOnLoan { get; set; }
    }
}