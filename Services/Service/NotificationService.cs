using FapWeb.Models.Data;
using FapWeb.Models.Dtos.NotificationDtos;
using FapWeb.Models.Entities;
using FapWeb.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace FapWeb.Services.Service
{
    public class NotificationService : INotificationService
    {
        private readonly PostgresContext _context;

        public NotificationService(PostgresContext context)
        {
            _context = context;
        }

        public async Task<List<NotificationResponseDto>> GetReceivedNotificationsAsync(Guid receiverId)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Include(notification => notification.Sender)
                .Include(notification => notification.Receiver)
                .Where(notification => notification.ReceiverId == receiverId)
                .OrderByDescending(notification => notification.CreatedAt)
                .Select(notification => new NotificationResponseDto
                {
                    Id = notification.Id,
                    SenderId = notification.SenderId,
                    SenderName = notification.Sender != null ? notification.Sender.FullName : null,
                    ReceiverId = notification.ReceiverId,
                    ReceiverName = notification.Receiver != null ? notification.Receiver.FullName : null,
                    Title = notification.Title,
                    Content = notification.Content,
                    IsRead = notification.IsRead == true,
                    CreatedAt = notification.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<NotificationResponseDto?> GetNotificationDetailsAsync(Guid notificationId, Guid receiverId)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Include(notification => notification.Sender)
                .Include(notification => notification.Receiver)
                .Where(notification => notification.Id == notificationId && notification.ReceiverId == receiverId)
                .Select(notification => new NotificationResponseDto
                {
                    Id = notification.Id,
                    SenderId = notification.SenderId,
                    SenderName = notification.Sender != null ? notification.Sender.FullName : null,
                    ReceiverId = notification.ReceiverId,
                    ReceiverName = notification.Receiver != null ? notification.Receiver.FullName : null,
                    Title = notification.Title,
                    Content = notification.Content,
                    IsRead = notification.IsRead == true,
                    CreatedAt = notification.CreatedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<int> CountUnreadAsync(Guid receiverId)
        {
            return await _context.Notifications
                .CountAsync(notification => notification.ReceiverId == receiverId && notification.IsRead != true);
        }

        public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid receiverId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(notification => notification.Id == notificationId && notification.ReceiverId == receiverId);

            if (notification == null)
            {
                return false;
            }

            if (notification.IsRead == true)
            {
                return true;
            }

            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task CreateAbsentAttendanceNotificationAsync(Guid teacherId, Guid studentId, DateTime attendanceDate)
        {
            var guardians = await _context.StudentGuardians
                .AsNoTracking()
                .Include(studentGuardian => studentGuardian.Student)
                .Where(studentGuardian => studentGuardian.StudentId == studentId)
                .ToListAsync();

            if (guardians.Count == 0)
            {
                return;
            }

            var createdAt = DateTime.UtcNow;
            var attendanceDateText = attendanceDate.ToString("yyyy-MM-dd");

            var notifications = guardians.Select(studentGuardian =>
            {
                var studentName = studentGuardian.Student?.FullName;
                var studentDisplayName = string.IsNullOrWhiteSpace(studentName) ? "the student" : studentName;

                return new Notification
                {
                    SenderId = teacherId,
                    ReceiverId = studentGuardian.GuardianId,
                    Title = "Absence Notification",
                    Content = $"{studentDisplayName} was marked absent on {attendanceDateText}.",
                    IsRead = false,
                    CreatedAt = createdAt
                };
            }).ToList();

            await _context.Notifications.AddRangeAsync(notifications);
            await _context.SaveChangesAsync();
        }
    }
}
