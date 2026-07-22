using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FapWeb.Infrastructure
{
    /// <summary>
    /// Kiểm tra đăng nhập và vai trò trước khi vào action, thay cho việc
    /// đọc Session thủ công trong từng controller.
    ///
    /// Dùng: [RequireRole] để chỉ cần đăng nhập,
    ///       [RequireRole(AppRoles.Admin)] hoặc [RequireRole(AppRoles.Admin, AppRoles.Teacher)]
    ///       để giới hạn theo vai trò.
    ///
    /// Chưa đăng nhập  -> chuyển về trang Login.
    /// Sai vai trò     -> chuyển về Dashboard (giữ nguyên hành vi cũ của UserController).
    /// Request AJAX    -> trả mã 401/403 kèm JSON thay vì redirect.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class RequireRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _allowedRoles;

        public RequireRoleAttribute(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var session = context.HttpContext.Session;

            if (!session.IsLoggedIn())
            {
                context.Result = IsAjaxRequest(context.HttpContext.Request)
                    ? new JsonResult(new { answer = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại." })
                    {
                        StatusCode = StatusCodes.Status401Unauthorized
                    }
                    : new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            if (_allowedRoles.Length == 0)
            {
                return;
            }

            var roleName = session.GetRoleName();
            var isAllowed = _allowedRoles.Any(allowed => string.Equals(allowed, roleName, StringComparison.OrdinalIgnoreCase));

            if (!isAllowed)
            {
                context.Result = IsAjaxRequest(context.HttpContext.Request)
                    ? new JsonResult(new { answer = "Bạn không có quyền thực hiện thao tác này." })
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    }
                    : new RedirectToActionResult("Index", "Dashboard", null);
            }
        }

        private static bool IsAjaxRequest(HttpRequest request)
        {
            return string.Equals(request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
        }
    }
}
