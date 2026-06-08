using FapWeb.Models.Dtos.NotificationDtos;

namespace FapWeb.Services.IServices
{
    public interface INotificationService
    {
        Task<List<NotificationResponseDto>> GetReceivedNotificationsAsync(Guid receiverId);

        Task<NotificationResponseDto?> GetNotificationDetailsAsync(Guid notificationId, Guid receiverId);

        Task<int> CountUnreadAsync(Guid receiverId);

        Task<bool> MarkAsReadAsync(Guid notificationId, Guid receiverId);

        Task CreateAbsentAttendanceNotificationAsync(Guid teacherId, Guid studentId, DateTime attendanceDate);

        Task CreateTuitionReminderNotificationAsync(Guid teacherId, Guid studentId, decimal? amount, DateTime? dueDate);
    }
}
