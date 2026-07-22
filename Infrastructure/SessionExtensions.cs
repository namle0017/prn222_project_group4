namespace FapWeb.Infrastructure
{
    /// <summary>
    /// Gom việc đọc thông tin đăng nhập từ Session về một chỗ,
    /// thay cho các hàm GetCurrentUserId/GetCurrentRoleName lặp lại trong từng controller.
    /// </summary>
    public static class SessionExtensions
    {
        public static Guid? GetUserId(this ISession session)
        {
            var userIdText = session.GetString(AppSessionKeys.UserId);

            if (string.IsNullOrWhiteSpace(userIdText) || !Guid.TryParse(userIdText, out var userId))
            {
                return null;
            }

            return userId;
        }

        public static string? GetRoleName(this ISession session)
        {
            return session.GetString(AppSessionKeys.RoleName);
        }

        public static bool IsLoggedIn(this ISession session)
        {
            return session.GetUserId().HasValue;
        }
    }
}
