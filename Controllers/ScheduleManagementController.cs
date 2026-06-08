using FapWeb.Infrastructure;
using FapWeb.Models.Dtos.ScheduleManagementDtos;
using FapWeb.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace FapWeb.Controllers
{
    public class ScheduleManagementController : Controller
    {
        private readonly IScheduleManagementService _scheduleManagementService;

        public ScheduleManagementController(IScheduleManagementService scheduleManagementService)
        {
            _scheduleManagementService = scheduleManagementService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(Guid? classId, DateOnly? dateFilter)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = await _scheduleManagementService.GetIndexAsync(classId, dateFilter, userId.Value, GetCurrentRoleName());
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create(Guid? classId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = await _scheduleManagementService.GetCreateModelAsync(classId, userId.Value, GetCurrentRoleName());
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ScheduleFormDto request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                var refreshed = await _scheduleManagementService.GetCreateModelAsync(request.ClassId, userId.Value, GetCurrentRoleName());
                request.ClassOptions = refreshed?.ClassOptions ?? new();
                return View(request);
            }

            var scheduleId = await _scheduleManagementService.CreateAsync(request, userId.Value, GetCurrentRoleName());
            TempData[scheduleId.HasValue ? "SuccessMessage" : "ErrorMessage"] = scheduleId.HasValue
                ? "Schedule created successfully."
                : "Unable to create schedule. Check duplicate date/time or class access.";

            return scheduleId.HasValue
                ? RedirectToAction(nameof(ClassSchedule), new { classId = request.ClassId })
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

            var model = await _scheduleManagementService.GetEditModelAsync(id, userId.Value, GetCurrentRoleName());
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ScheduleFormDto request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                if (request.Id.HasValue)
                {
                    var refreshed = await _scheduleManagementService.GetEditModelAsync(request.Id.Value, userId.Value, GetCurrentRoleName());
                    request.ClassOptions = refreshed?.ClassOptions ?? new();
                }

                return View(request);
            }

            var updated = await _scheduleManagementService.UpdateAsync(request, userId.Value, GetCurrentRoleName());
            TempData[updated ? "SuccessMessage" : "ErrorMessage"] = updated
                ? "Schedule updated successfully."
                : "Unable to update schedule.";

            return RedirectToAction(nameof(ClassSchedule), new { classId = request.ClassId });
        }

        [HttpGet]
        public async Task<IActionResult> ClassSchedule(Guid classId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = await _scheduleManagementService.GetClassScheduleAsync(classId, userId.Value, GetCurrentRoleName());
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
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
