using FapWeb.Infrastructure;
using Microsoft.AspNetCore.SignalR;

namespace FapWeb.Hubs
{
    // Realtime cho luồng duyệt "phí khác": teacher tạo -> đẩy tới nhóm ADMIN;
    // admin duyệt/từ chối -> đẩy tới nhóm của user tạo (theo UserId).
    public class ApprovalHub : Hub
    {
        public const string AdminGroup = "ADMIN";

        public static string UserGroup(Guid userId) => $"user:{userId:N}";

        public override async Task OnConnectedAsync()
        {
            var session = Context.GetHttpContext()?.Session;
            var roleName = session?.GetString(AppSessionKeys.RoleName);
            var userIdStr = session?.GetString(AppSessionKeys.UserId);

            if (AppRoles.IsAdmin(roleName))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, AdminGroup);
            }

            if (!string.IsNullOrWhiteSpace(userIdStr) && Guid.TryParse(userIdStr, out var userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));
            }

            await base.OnConnectedAsync();
        }
    }
}
