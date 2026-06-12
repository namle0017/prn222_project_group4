using FapWeb.Infrastructure;
using FapWeb.Models.Dtos.UserManagementDtos;
using FapWeb.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace FapWeb.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserManagementService _userManagementService;

        public UserController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? searchTerm, string? roleFilter)
        {
            var accessCheck = EnsureAdmin();
            if (accessCheck != null)
            {
                return accessCheck;
            }

            var users = await _userManagementService.GetUsersAsync(searchTerm, roleFilter);

            ViewBag.SearchTerm = searchTerm;
            ViewBag.RoleFilter = roleFilter;

            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var accessCheck = EnsureAdmin();
            if (accessCheck != null)
            {
                return accessCheck;
            }

            return View(await _userManagementService.GetCreateModelAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserFormDto request)
        {
            var accessCheck = EnsureAdmin();
            if (accessCheck != null)
            {
                return accessCheck;
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                ModelState.AddModelError(nameof(request.Password), "Mật khẩu là bắt buộc khi tạo tài khoản mới.");
            }

            if (!ModelState.IsValid)
            {
                request.RoleOptions = (await _userManagementService.GetCreateModelAsync()).RoleOptions;
                return View(request);
            }

            var (success, error) = await _userManagementService.CreateAsync(request);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, error ?? "Không thể tạo tài khoản.");
                request.RoleOptions = (await _userManagementService.GetCreateModelAsync()).RoleOptions;
                return View(request);
            }

            TempData["SuccessMessage"] = "Tạo tài khoản thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var accessCheck = EnsureAdmin();
            if (accessCheck != null)
            {
                return accessCheck;
            }

            var model = await _userManagementService.GetEditModelAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserFormDto request)
        {
            var accessCheck = EnsureAdmin();
            if (accessCheck != null)
            {
                return accessCheck;
            }

            if (!ModelState.IsValid)
            {
                request.RoleOptions = (await _userManagementService.GetCreateModelAsync()).RoleOptions;
                return View(request);
            }

            var (success, error) = await _userManagementService.UpdateAsync(request);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, error ?? "Không thể cập nhật tài khoản.");
                request.RoleOptions = (await _userManagementService.GetCreateModelAsync()).RoleOptions;
                return View(request);
            }

            TempData["SuccessMessage"] = "Cập nhật tài khoản thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Guardians()
        {
            var accessCheck = EnsureAdmin();
            if (accessCheck != null)
            {
                return accessCheck;
            }

            ViewBag.LinkForm = await _userManagementService.GetGuardianLinkModelAsync();
            var links = await _userManagementService.GetGuardianLinksAsync();
            return View(links);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkGuardian(GuardianLinkFormDto request)
        {
            var accessCheck = EnsureAdmin();
            if (accessCheck != null)
            {
                return accessCheck;
            }

            var (success, error) = await _userManagementService.CreateGuardianLinkAsync(request);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
                ? "Liên kết phụ huynh – học sinh thành công."
                : error ?? "Không thể tạo liên kết.";

            return RedirectToAction(nameof(Guardians));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveGuardianLink(Guid linkId)
        {
            var accessCheck = EnsureAdmin();
            if (accessCheck != null)
            {
                return accessCheck;
            }

            var removed = await _userManagementService.RemoveGuardianLinkAsync(linkId);
            TempData[removed ? "SuccessMessage" : "ErrorMessage"] = removed
                ? "Đã xóa liên kết."
                : "Không thể xóa liên kết.";

            return RedirectToAction(nameof(Guardians));
        }

        private IActionResult? EnsureAdmin()
        {
            var userIdStr = HttpContext.Session.GetString(AppSessionKeys.UserId);
            if (string.IsNullOrWhiteSpace(userIdStr))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!AppRoles.IsAdmin(HttpContext.Session.GetString(AppSessionKeys.RoleName)))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return null;
        }
    }
}
