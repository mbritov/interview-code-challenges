using Microsoft.AspNetCore.Mvc;
using OneBeyondApi.DataAccess;
using OneBeyondApi.Model;

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
        public IActionResult RequestLoan([FromBody] LoanRequest request)
        {
            var res = _borrowerRepository.RequestLoan(request.Borrower, request.Book);
            return Ok();
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
        public IActionResult Reserve([FromBody] ReserveRequest request)
        {
            var res = _borrowerRepository.Reserve(request.Borrower, request.Book);
            return Ok(res);
        }
    }
}
