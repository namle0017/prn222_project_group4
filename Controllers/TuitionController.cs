using FapWeb.Models.Dtos.TuitionDtos;
using FapWeb.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace FapWeb.Controllers
{
    public class TuitionController : Controller
    {
        private const string UserIdSessionKey = "UserId";
        private const string RoleNameSessionKey = "RoleName";

        private readonly ITuitionService _tuitionService;

        public TuitionController(ITuitionService tuitionService)
        {
            _tuitionService = tuitionService;
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
        public async Task<IActionResult> CreateFee()
        {
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

            var baseCallbackUrl = $"{Request.Scheme}://{Request.Host}";
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
