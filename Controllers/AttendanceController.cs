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

        [HttpGet]
        public async Task<IActionResult> ExportHistory(Guid? studentId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var history = await _attendanceService.GetAttendanceHistoryAsync(userId.Value, GetCurrentRoleName(), studentId);
            
            var builder = new System.Text.StringBuilder();
            builder.AppendLine("Ngày,Học sinh,Lớp học,Trạng thái,Giáo viên");
            
            foreach (var item in history)
            {
                var status = item.StatusName.Equals("ABSENT", StringComparison.OrdinalIgnoreCase) ? "Vắng mặt" : "Có mặt";
                var teacher = string.IsNullOrWhiteSpace(item.TeacherName) ? "Không có" : item.TeacherName;
                builder.AppendLine($"{item.AttendanceDate:dd/MM/yyyy},{item.StudentName},{item.ClassName},{status},{teacher}");
            }
            
            return File(System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(builder.ToString())).ToArray(), "text/csv", $"LichSuDiemDanh_{DateTime.Now:yyyyMMdd}.csv");
        }

        [HttpGet]
        public async Task<IActionResult> ExportSession(Guid classId, Guid? scheduleId, DateTime? attendanceDate)
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

            var builder = new System.Text.StringBuilder();
            builder.AppendLine("STT,Học sinh,Trạng thái");
            
            for (int i = 0; i < model.Students.Count; i++)
            {
                var student = model.Students[i];
                var status = student.StatusName.Equals("ABSENT", StringComparison.OrdinalIgnoreCase) ? "Vắng mặt" : "Có mặt";
                builder.AppendLine($"{i + 1},{student.StudentName},{status}");
            }
            
            return File(System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(builder.ToString())).ToArray(), "text/csv", $"DiemDanhCaHoc_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
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
