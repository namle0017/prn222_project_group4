using FapWeb.Infrastructure;
using FapWeb.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace FapWeb.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var roleName = GetCurrentRoleName();

            if (userId == null || string.IsNullOrWhiteSpace(roleName))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (AppRoles.IsAdmin(roleName))
            {
                return View("Admin", await _dashboardService.GetAdminDashboardAsync());
            }

            if (AppRoles.IsTeacher(roleName))
            {
                return View("Teacher", await _dashboardService.GetTeacherDashboardAsync(userId.Value));
            }

            if (AppRoles.IsParent(roleName))
            {
                return View("Parent", await _dashboardService.GetParentDashboardAsync(userId.Value));
            }

            if (AppRoles.IsStudent(roleName))
            {
                return View("Student", await _dashboardService.GetStudentDashboardAsync(userId.Value));
            }

            return View();
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
