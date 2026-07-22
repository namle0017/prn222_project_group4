using FapWeb.Models.Dtos.AttendanceDtos;
using FapWeb.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using FapWeb.Infrastructure;

namespace FapWeb.Controllers
{
    /// <summary>
    /// Bộ điều khiển Quản lý Điểm danh và Tra cứu Lịch sử Chuyên cần (Attendance Controller).
    /// </summary>
    /// <remarks>
    /// Đảm nhận điều hướng các trang hiển thị danh sách lớp học điểm danh, form tích chọn trạng thái có mặt/vắng mặt,
    /// xuất dữ liệu ra file Excel và hiển thị giao diện báo cáo chuyên cần.
    /// </remarks>
    [RequireRole]
    public class AttendanceController : Controller
    {
        private readonly IAttendanceService _attendanceService;

        /// <summary>
        /// Khởi tạo AttendanceController với IAttendanceService được tiêm qua Dependency Injection.
        /// </summary>
        /// <param name="attendanceService">Dịch vụ quản lý điểm danh.</param>
        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        /// <summary>
        /// Action hiển thị trang danh sách các lớp học cần điểm danh (Index View).
        /// </summary>
        /// <returns>Trả về View danh sách lớp học hoặc chuyển hướng về trang Đăng nhập nếu chưa xác thực.</returns>
        [HttpGet]
        [RequireRole(AppRoles.Admin, AppRoles.Teacher)]
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
        [RequireRole(AppRoles.Admin, AppRoles.Teacher)]
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
        [RequireRole(AppRoles.Admin, AppRoles.Teacher)]
        public async Task<IActionResult> Take(AttendanceSaveRequestDto request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var saved = await _attendanceService.SaveAttendanceAsync(request, userId.Value, GetCurrentRoleName());
            TempData[saved ? "SuccessMessage" : "ErrorMessage"] = saved
                ? "Lưu điểm danh thành công."
                : request.ScheduleId.HasValue
                    ? "Không thể lưu điểm danh. Vui lòng kiểm tra lại buổi học và danh sách học sinh của lớp."
                    : "Lớp này chưa có buổi học nào trong ngày đã chọn. Vui lòng tạo lịch học trước.";

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
                builder.AppendLine(CsvHelper.Row(
                    item.AttendanceDate.ToString("dd/MM/yyyy"),
                    item.StudentName,
                    item.ClassName,
                    status,
                    teacher));
            }

            return File(CsvHelper.ToFileBytes(builder), "text/csv", $"LichSuDiemDanh_{DateTime.Now:yyyyMMdd}.csv");
        }

        [HttpGet]
        [RequireRole(AppRoles.Admin, AppRoles.Teacher)]
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
                builder.AppendLine(CsvHelper.Row((i + 1).ToString(), student.StudentName, status));
            }

            return File(CsvHelper.ToFileBytes(builder), "text/csv", $"DiemDanhCaHoc_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
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
