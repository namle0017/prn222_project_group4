using FapWeb.Models.Dtos.AttendanceDtos;
using FapWeb.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using FapWeb.Infrastructure;

namespace FapWeb.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var classes = await _attendanceService.GetAttendanceClassesAsync(userId.Value, GetCurrentRoleName());
            return View(classes);
        }

        [HttpGet]
        public async Task<IActionResult> Take(Guid classId, Guid? scheduleId, DateTime? attendanceDate)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = await _attendanceService.GetAttendanceTakeViewAsync(classId, scheduleId, attendanceDate, userId.Value, GetCurrentRoleName());
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Take(AttendanceSaveRequestDto request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var saved = await _attendanceService.SaveAttendanceAsync(request, userId.Value, GetCurrentRoleName());
            TempData[saved ? "SuccessMessage" : "ErrorMessage"] = saved
                ? "Attendance saved successfully."
                : request.ScheduleId.HasValue
                    ? "Unable to save attendance. Please verify the selected schedule and enrolled students."
                    : "No schedule exists for this class and date. Please create a schedule first.";

            return RedirectToAction(nameof(Take), new
            {
                classId = request.ClassId,
                scheduleId = request.ScheduleId,
                attendanceDate = request.AttendanceDate.ToString("yyyy-MM-dd")
            });
        }

        [HttpGet]
        public async Task<IActionResult> History(Guid? studentId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var history = await _attendanceService.GetAttendanceHistoryAsync(userId.Value, GetCurrentRoleName(), studentId);
            return View(history);
        }

        private Guid? GetCurrentUserId()
        {
            var userIdStr = HttpContext.Session.GetString(AppSessionKeys.UserId);

            if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return null;
            }

            return userId;
        }

        private string? GetCurrentRoleName()
        {
            return HttpContext.Session.GetString(AppSessionKeys.RoleName);
        }
    }
}
