using FapWeb.Models.Dtos.ChangePasswordDtos;
using FapWeb.Models.Dtos.LoginDtos;
using FapWeb.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace FapWeb.Controllers
{
    public class AuthController : Controller
    {
        private const string UserIdSessionKey = "UserId";
        private const string RoleNameSessionKey = "RoleName";
        private const string UserIdCookieKey = "userId";
        private const string RoleNameCookieKey = "roleName";

        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (!string.IsNullOrWhiteSpace(HttpContext.Session.GetString(UserIdSessionKey)))
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new LoginRequestDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var loginResult = _authService.LoginAsync(request);
            if (loginResult == null)
            {
                ModelState.AddModelError(string.Empty, "Số điện thoại hoặc mật khẩu không đúng.");
                return View(request);
            }

            HttpContext.Session.SetString(UserIdSessionKey, loginResult.UserId.ToString());
            HttpContext.Session.SetString(RoleNameSessionKey, loginResult.UserRole);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddHours(8),
                IsEssential = true
            };

            Response.Cookies.Append(UserIdCookieKey, loginResult.UserId.ToString(), cookieOptions);
            Response.Cookies.Append(RoleNameCookieKey, loginResult.UserRole, cookieOptions);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(UserIdSessionKey)))
            {
                return RedirectToAction(nameof(Login));
            }

            return View(new ChangePasswordRequestDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequestDto request)
        {
            var userIdStr = HttpContext.Session.GetString(UserIdSessionKey);
            if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return RedirectToAction(nameof(Login));
            }

            if (!ModelState.IsValid)
            {
                return View(request);
            }

            request.UserId = userId;
            var changed = await _authService.ChangePassword(request);
            if (!changed)
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu hiện tại không đúng.");
                return View(request);
            }

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công.";
            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove(UserIdSessionKey);
            HttpContext.Session.Remove(RoleNameSessionKey);

            Response.Cookies.Delete(UserIdCookieKey);
            Response.Cookies.Delete(RoleNameCookieKey);

            return RedirectToAction(nameof(Login));
        }
    }
}
