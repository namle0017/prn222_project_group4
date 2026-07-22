using FapWeb.Infrastructure;
using FapWeb.Models.Dtos.PaginatedDtos;
using FapWeb.Models.Dtos.TransactionHistoryDtos;
using FapWeb.Models.Dtos.TransactionHistoryDtos.Childs;
using FapWeb.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace FapWeb.Controllers
{
    public class TransactionController : Controller
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(ITransactionService transactionService, ILogger<TransactionController> logger)
        {
            _transactionService = transactionService;
            _logger = logger;
        }

        // GET: /Transaction/Index?pageNumber=1&pageSize=10
        // Xem toan bo giao dich cua he thong: chi ADMIN.
        [HttpGet]
        [RequireRole(AppRoles.Admin)]
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            var request = new TransactionHistoryRequestDto
            {
                QueryUser = new UserQueryRequestDto(),
                Paginated = new PaginatedDto
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize
                }
            };

            var transactions = await _transactionService.GetTransactionAsync(request);

            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;

            return View(transactions);
        }

        // GET: /Transaction/MyHistory?pageNumber=1&pageSize=10
        // Giao dich cua chinh nguoi dang dang nhap: moi vai tro deu xem duoc.
        [HttpGet]
        [RequireRole]
        public async Task<IActionResult> MyHistory(int pageNumber = 1, int pageSize = 10)
        {
            var userId = HttpContext.Session.GetUserId()!.Value;

            var request = new TransactionHistoryRequestDto
            {
                QueryUser = new UserQueryRequestDto { UserId = userId },
                Paginated = new PaginatedDto
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize
                }
            };

            var transactions = await _transactionService.GetTransactionAsync(request);

            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;

            return View(transactions);
        }

        // GET: /Transaction/Search?searchTerm=...&pageNumber=1&pageSize=10
        // Tim kiem tren toan bo giao dich (theo ten hoc sinh/phu huynh): chi ADMIN.
        [HttpGet]
        [RequireRole(AppRoles.Admin)]
        public async Task<IActionResult> Search(string? searchTerm, int pageNumber = 1, int pageSize = 10)
        {
            var request = new TransactionHistoryRequestDto
            {
                QueryUser = new UserQueryRequestDto { SearchTerm = searchTerm },
                Paginated = new PaginatedDto
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize
                }
            };

            var transactions = await _transactionService.GetTransactionAsync(request);

            ViewBag.SearchTerm = searchTerm;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;

            return View("Index", transactions);
        }
    }
}
