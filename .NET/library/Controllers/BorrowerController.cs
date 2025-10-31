using Microsoft.AspNetCore.Mvc;
using OneBeyondApi.DataAccess;
using OneBeyondApi.Model;

/*
Add an "On Loan" end point with functionality to get/query the details of all borrowers with active loans and the titles of books they have on loan.
Extend the "On Loan" end point to allow books on loan to be returned.

If books are returned after their loan end date then a fine should be raised against the borrower 
(data model for fines and relationships with borrowers are left to the candidate to define)

Add functionality to allow a borrower to reserve a particular title that is currently on loan
(also consider the case of multiple borrowers all wanting to borrow the same book). 
The borrower should also be able to query via the API to find out when the book will be available for them.
*/

namespace OneBeyondApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BorrowerController : ControllerBase
    {
        private readonly ILogger<BorrowerController> _logger;
        private readonly IBorrowerRepository _borrowerRepository;

        public BorrowerController(ILogger<BorrowerController> logger, IBorrowerRepository borrowerRepository)
        {
            _logger = logger;
            _borrowerRepository = borrowerRepository;
        }

        [HttpGet]
        [Route("GetBorrowers")]
        public IList<Borrower> Get()
        {
            return _borrowerRepository.GetBorrowers();
        }

        [HttpPost]
        [Route("AddBorrower")]
        public Guid Post(Borrower borrower)
        {
            return _borrowerRepository.AddBorrower(borrower);
        }

        [HttpPost]
        [Route("OnLoan")]
        public List<BorrowerData> OnLoan()
        {
            return _borrowerRepository.GetBorrowersWithLoan();
        }

        [HttpPost]
        [Route("RequestLoan")]
        public IActionResult RequestLoan(Borrower borrower, Book book)
        {
            var res = _borrowerRepository.RequestLoan(borrower, book);
            return Ok(res);
        }

        [HttpPost]
        [Route("CompleteLoan")]
        public IActionResult CompleteLoan(Borrower borrower, Guid bookId)
        {
            var res = _borrowerRepository.CompleteLoan(borrower, bookId);
            return Ok(res);
        }


        [HttpPost]
        [Route("Reserve")]
        public IActionResult Reserve(Borrower borrower, Book book)
        {
            var res = _borrowerRepository.Reserve(borrower, book);
            return Ok(res);
        }
    }
}