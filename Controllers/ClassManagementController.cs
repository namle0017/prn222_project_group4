using FapWeb.Infrastructure;
using FapWeb.Models.Dtos.ClassManagementDtos;
using FapWeb.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace FapWeb.Controllers
{
    // Toan bo khu vuc quan ly lop chi danh cho ADMIN va TEACHER.
    // Menu trong _Layout da an voi cac vai tro khac, nhung truoc day go
    // thang URL van vao duoc nen can chan o day.
    [RequireRole(AppRoles.Admin, AppRoles.Teacher)]
    public class ClassManagementController : Controller
    {
        private readonly IClassManagementService _classManagementService;

        public ClassManagementController(IClassManagementService classManagementService)
        {
            _classManagementService = classManagementService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? searchTerm, Guid? teacherId, string? statusFilter)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = await _classManagementService.GetIndexAsync(searchTerm, teacherId, statusFilter, userId.Value, GetCurrentRoleName());
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!AppRoles.IsStaff(GetCurrentRoleName()))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var model = await _classManagementService.GetCreateModelAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClassFormDto request)
        {
            if (!AppRoles.IsStaff(GetCurrentRoleName()))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            if (!ModelState.IsValid)
            {
                request.TeacherOptions = (await _classManagementService.GetCreateModelAsync()).TeacherOptions;
                return View(request);
            }

            var classId = await _classManagementService.CreateAsync(request);
            TempData[classId.HasValue ? "SuccessMessage" : "ErrorMessage"] = classId.HasValue
                ? "Tạo lớp học thành công."
                : "Không thể tạo lớp học.";

            return classId.HasValue
                ? RedirectToAction(nameof(Details), new { id = classId.Value })
                : RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = await _classManagementService.GetEditModelAsync(id, userId.Value, GetCurrentRoleName());
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ClassFormDto request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                var refreshed = request.Id.HasValue
                    ? await _classManagementService.GetEditModelAsync(request.Id.Value, userId.Value, GetCurrentRoleName())
                    : null;

                request.TeacherOptions = refreshed?.TeacherOptions ?? new();
                return View(request);
            }

            var updated = await _classManagementService.UpdateAsync(request, userId.Value, GetCurrentRoleName());
            TempData[updated ? "SuccessMessage" : "ErrorMessage"] = updated
                ? "Cập nhật lớp học thành công."
                : "Không thể cập nhật lớp học.";

            return updated && request.Id.HasValue
                ? RedirectToAction(nameof(Details), new { id = request.Id.Value })
                : RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = await _classManagementService.GetDetailsAsync(id, userId.Value, GetCurrentRoleName());
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Students(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = await _classManagementService.GetStudentsAsync(id, userId.Value, GetCurrentRoleName());
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ExportStudents(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = await _classManagementService.GetStudentsAsync(id, userId.Value, GetCurrentRoleName());
            if (model == null)
            {
                return NotFound();
            }

            var builder = new System.Text.StringBuilder();
            builder.AppendLine("Học sinh,Người giám hộ,Ngày tham gia,Trạng thái");
            
            foreach (var student in model.Students)
            {
                var guardian = string.IsNullOrWhiteSpace(student.GuardianName) ? "Không có" : student.GuardianName;
                var enrolledAt = student.EnrolledAt.HasValue ? student.EnrolledAt.Value.ToString("dd/MM/yyyy HH:mm") : "Không có";
                var status = student.IsActive ? "Hoạt động" : "Không hoạt động";
                builder.AppendLine(CsvHelper.Row(student.StudentName, guardian, enrolledAt, status));
            }

            return File(CsvHelper.ToFileBytes(builder), "text/csv", $"DanhSachHocSinh_{DateTime.Now:yyyyMMdd}.csv");
        }

        [HttpGet]
        public async Task<IActionResult> AddStudent(Guid classId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = await _classManagementService.GetAddStudentModelAsync(classId, userId.Value, GetCurrentRoleName());
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudent(AddStudentToClassDto request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                var refreshed = await _classManagementService.GetAddStudentModelAsync(request.ClassId, userId.Value, GetCurrentRoleName());
                request.StudentOptions = refreshed?.StudentOptions ?? new();
                request.ClassName = refreshed?.ClassName ?? request.ClassName;
                return View(request);
            }

            var added = await _classManagementService.AddStudentAsync(request, userId.Value, GetCurrentRoleName());
            TempData[added ? "SuccessMessage" : "ErrorMessage"] = added
                ? "Thêm học sinh vào lớp thành công."
                : "Không thể thêm học sinh vào lớp.";

            return RedirectToAction(nameof(Students), new { id = request.ClassId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveStudent(Guid classId, Guid studentId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var removed = await _classManagementService.RemoveStudentAsync(classId, studentId, userId.Value, GetCurrentRoleName());
            TempData[removed ? "SuccessMessage" : "ErrorMessage"] = removed
                ? "Đã xóa học sinh khỏi lớp."
                : "Không thể xóa học sinh khỏi lớp.";

            return RedirectToAction(nameof(Students), new { id = classId });
        }

        private Guid? GetCurrentUserId()
        {
            var userIdStr = HttpContext.Session.GetString(AppSessionKeys.UserId);
            return string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId) ? null : userId;
        }

        private string? GetCurrentRoleName()
        {
            return HttpContext.Session.GetString(AppSessionKeys.RoleName);
        }
    }
}
