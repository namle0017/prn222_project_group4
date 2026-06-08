using FapWeb.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace FapWeb.Controllers
{
    public class NotificationController : Controller
    {
        private const string UserIdSessionKey = "UserId";
        private const string RoleNameSessionKey = "RoleName";

        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var notifications = await _notificationService.GetReceivedNotificationsAsync(userId.Value);
            return View(notifications);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var notification = await _notificationService.GetNotificationDetailsAsync(id, userId.Value);
            if (notification == null)
            {
                return NotFound();
            }

            await _notificationService.MarkAsReadAsync(id, userId.Value);
            notification.IsRead = true;

            return View(notification);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var marked = await _notificationService.MarkAsReadAsync(id, userId.Value);
            if (!marked)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> UnreadCount()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { count = 0 });
            }

            var count = await _notificationService.CountUnreadAsync(userId.Value);
            return Json(new { count });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTuitionReminder(Guid studentId, decimal? amount, DateTime? dueDate)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            await _notificationService.CreateTuitionReminderNotificationAsync(userId.Value, studentId, amount, dueDate);
            return RedirectToAction(nameof(Index));
        }

        private Guid? GetCurrentUserId()
        {
            var userIdStr = HttpContext.Session.GetString(UserIdSessionKey);

            if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out Guid userId))
            {
                return null;
            }

            _ = HttpContext.Session.GetString(RoleNameSessionKey);
            return userId;
        }
    }
}
