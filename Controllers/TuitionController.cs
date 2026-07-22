using FapWeb.Infrastructure;
using FapWeb.Models.Dtos.TuitionDtos;
using FapWeb.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace FapWeb.Controllers
{
    public class TuitionController : Controller
    {
        private const string UserIdSessionKey = "UserId";
        private const string RoleNameSessionKey = "RoleName";

        private readonly ITuitionService _tuitionService;
        private readonly IConfiguration _configuration;

        public TuitionController(ITuitionService tuitionService, IConfiguration configuration)
        {
            _tuitionService = tuitionService;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var data = await _tuitionService.GetTuitionStatusesAsync(userId.Value, GetCurrentRoleName());
            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> ExportTuition()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var data = await _tuitionService.GetTuitionStatusesAsync(userId.Value, GetCurrentRoleName());
            
            var builder = new System.Text.StringBuilder();
            builder.AppendLine("Học sinh,Lớp học,Học phí yêu cầu,Đã nộp,Còn lại,Trạng thái,Hạn nộp");
            
            foreach (var item in data)
            {
                var dueDate = item.DueDate.HasValue ? item.DueDate.Value.ToString("dd/MM/yyyy") : "Không có";
                var status = item.StatusName switch
                {
                    "PAID" => "Đã nộp",
                    "PARTIAL" => "Nộp một phần",
                    "UNPAID" => "Chưa nộp",
                    _ => item.StatusName
                };
                builder.AppendLine($"{item.StudentName},{item.ClassName},{item.RequiredFee},{item.PaidAmount},{item.RemainingAmount},{status},{dueDate}");
            }
            
            return File(System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(builder.ToString())).ToArray(), "text/csv", $"QuanLyHocPhi_{DateTime.Now:yyyyMMdd}.csv");
        }

        [HttpGet]
        public async Task<IActionResult> CreateFee()
        {
            var accessCheck = EnsureAdmin();
            if (accessCheck != null)
            {
                return accessCheck;
            }

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = await _tuitionService.GetCreateFeeModelAsync(userId.Value, GetCurrentRoleName());
            if (model == null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFee(TuitionFeeCreateDto request)
        {
            var accessCheck = EnsureAdmin();
            if (accessCheck != null)
            {
                return accessCheck;
            }

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                var refreshed = await _tuitionService.GetCreateFeeModelAsync(userId.Value, GetCurrentRoleName());
                request.ClassOptions = refreshed?.ClassOptions ?? new();
                return View(request);
            }

            var (created, skipped, error) = await _tuitionService.GenerateClassFeesAsync(request, userId.Value, GetCurrentRoleName());

            if (error != null)
            {
                TempData["ErrorMessage"] = error;
            }
            else
            {
                TempData["SuccessMessage"] = skipped > 0
                    ? $"Đã tạo {created} khoản học phí. Bỏ qua {skipped} học sinh đã có học phí tháng này."
                    : $"Đã tạo {created} khoản học phí cho cả lớp.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> CreatePayment(Guid tuitionFeeId)
        {
            var accessCheck = EnsureAdmin();
            if (accessCheck != null)
            {
                return accessCheck;
            }

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = await _tuitionService.GetPaymentCreateAsync(tuitionFeeId, userId.Value, GetCurrentRoleName());
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePayment(PaymentCreateDto request)
        {
            var accessCheck = EnsureAdmin();
            if (accessCheck != null)
            {
                return accessCheck;
            }

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var saved = await _tuitionService.RecordPaymentAsync(request, userId.Value, GetCurrentRoleName());
            TempData[saved ? "SuccessMessage" : "ErrorMessage"] = saved
                ? "Payment recorded successfully."
                : "Unable to record payment.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> History(Guid? tuitionFeeId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var history = await _tuitionService.GetPaymentHistoryAsync(userId.Value, GetCurrentRoleName(), tuitionFeeId);
            return View(history);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayOnline(Guid tuitionFeeId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var baseCallbackUrl = _configuration["BaseCallbackUrl"] ?? $"{Request.Scheme}://{Request.Host}";
            var checkoutForm = await _tuitionService.CreateOnlinePaymentAsync(tuitionFeeId, userId.Value, GetCurrentRoleName(), baseCallbackUrl);

            if (checkoutForm == null)
            {
                TempData["ErrorMessage"] = "Không thể tạo thanh toán online cho khoản học phí này.";
                return RedirectToAction(nameof(Index));
            }

            return View(checkoutForm);
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallback(string invoice, string result)
        {
            var statusName = result?.ToLowerInvariant() switch
            {
                "success" => "SUCCESS",
                "cancel" => "CANCELLED",
                _ => "FAILED"
            };

            var isSuccess = await _tuitionService.FinalizeOnlinePaymentAsync(invoice, statusName);

            if (isSuccess)
            {
                TempData["SuccessMessage"] = "Thanh toán học phí qua SePay thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = statusName == "CANCELLED"
                    ? "Giao dịch đã bị hủy."
                    : "Thanh toán không thành công hoặc giao dịch không hợp lệ.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendReminder(Guid tuitionFeeId)
        {
            var accessCheck = EnsureAdmin();
            if (accessCheck != null)
            {
                return accessCheck;
            }

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var sent = await _tuitionService.SendTuitionReminderAsync(tuitionFeeId, userId.Value, GetCurrentRoleName());
            TempData[sent ? "SuccessMessage" : "ErrorMessage"] = sent
                ? "Tuition reminder sent successfully."
                : "Unable to send tuition reminder.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> CreateOtherFee()
        {
            var accessCheck = EnsureStaff();
            if (accessCheck != null)
            {
                return accessCheck;
            }

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = await _tuitionService.GetCreateOtherFeeModelAsync(userId.Value, GetCurrentRoleName());
            if (model == null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOtherFee(OtherFeeCreateDto request)
        {
            var accessCheck = EnsureStaff();
            if (accessCheck != null)
            {
                return accessCheck;
            }

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                var refreshed = await _tuitionService.GetCreateOtherFeeModelAsync(userId.Value, GetCurrentRoleName());
                request.ClassOptions = refreshed?.ClassOptions ?? new();
                return View(request);
            }

            var (created, error) = await _tuitionService.CreateOtherFeeAsync(request, userId.Value, GetCurrentRoleName());
            if (error != null)
            {
                TempData["ErrorMessage"] = error;
            }
            else
            {
                TempData["SuccessMessage"] = AppRoles.IsAdmin(GetCurrentRoleName())
                    ? $"Đã tạo {created} khoản phí khác cho cả lớp."
                    : $"Đã tạo {created} khoản phí khác. Đang chờ Admin duyệt.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Approvals()
        {
            var accessCheck = EnsureAdmin();
            if (accessCheck != null)
            {
                return accessCheck;
            }

            var pending = await _tuitionService.GetPendingApprovalsAsync(GetCurrentRoleName());
            return View(pending);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveFee(Guid tuitionFeeId)
        {
            var accessCheck = EnsureAdmin();
            if (accessCheck != null)
            {
                return accessCheck;
            }

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var ok = await _tuitionService.ApproveFeeAsync(tuitionFeeId, userId.Value, GetCurrentRoleName());
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Đã duyệt khoản phí." : "Không thể duyệt khoản phí.";
            return RedirectToAction(nameof(Approvals));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectFee(Guid tuitionFeeId)
        {
            var accessCheck = EnsureAdmin();
            if (accessCheck != null)
            {
                return accessCheck;
            }

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var ok = await _tuitionService.RejectFeeAsync(tuitionFeeId, userId.Value, GetCurrentRoleName());
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "Đã từ chối khoản phí." : "Không thể từ chối khoản phí.";
            return RedirectToAction(nameof(Approvals));
        }

        [HttpGet]
        public async Task<IActionResult> PendingCount()
        {
            var count = await _tuitionService.CountPendingApprovalsAsync(GetCurrentRoleName());
            return Json(new { count });
        }

        private IActionResult? EnsureAdmin()
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(UserIdSessionKey)))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!AppRoles.IsAdmin(GetCurrentRoleName()))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return null;
        }

        private IActionResult? EnsureStaff()
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(UserIdSessionKey)))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!AppRoles.IsStaff(GetCurrentRoleName()))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return null;
        }

        private Guid? GetCurrentUserId()
        {
            var userIdStr = HttpContext.Session.GetString(UserIdSessionKey);

            if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return null;
            }

            return userId;
        }

        private string? GetCurrentRoleName()
        {
            return HttpContext.Session.GetString(RoleNameSessionKey);
        }
    }
}
