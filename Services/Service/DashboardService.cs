using FapWeb.Infrastructure;
using FapWeb.Models.Data;
using FapWeb.Models.Dtos.DashboardDtos;
using FapWeb.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace FapWeb.Services.Service
{
    public class DashboardService : IDashboardService
    {
        private readonly PostgresContext _context;

        public DashboardService(PostgresContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardDto> GetAdminDashboardAsync()
        {
            var totalExpected = await _context.TuitionFees.SumAsync(x => (decimal?)x.TotalAmount) ?? 0;
            var totalCollected = await _context.TuitionFees.SumAsync(x => x.PaidAmount) ?? 0;
            var attendanceRate = await GetOverallAttendanceRateAsync();

            return new AdminDashboardDto
            {
                TotalStudents = await CountUsersByRoleAsync(AppRoles.Student),
                TotalTeachers = await CountUsersByRoleAsync(AppRoles.Teacher),
                TotalParents = await CountUsersByRoleAsync(AppRoles.Parent),
                TotalClasses = await _context.Classes.CountAsync(),
                TotalActiveSchedules = await _context.Schedules.CountAsync(x => x.ScheduleDate >= DateOnly.FromDateTime(DateTime.Today)),
                TotalExpectedTuition = totalExpected,
                TotalCollectedTuition = totalCollected,
                TotalOutstandingTuition = totalExpected - totalCollected,
                StudentsWithUnpaidTuition = await _context.TuitionFees.CountAsync(x => x.TotalAmount - (x.PaidAmount ?? 0) > 0),
                OverallAttendanceRate = attendanceRate,
                LatestNotifications = await GetLatestNotificationsAsync()
            };
        }

        public async Task<TeacherDashboardDto> GetTeacherDashboardAsync(Guid currentUserId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            return new TeacherDashboardDto
            {
                MyClasses = await _context.Classes.CountAsync(x => x.TeacherId == currentUserId),
                StudentsInMyClasses = await _context.ClassStudents.CountAsync(x => x.Class.TeacherId == currentUserId && x.IsEnable != false),
                AbsentStudentsToday = await _context.AttendanceChecks.CountAsync(x => x.Schedule.Class.TeacherId == currentUserId && x.Schedule.ScheduleDate == today && x.Status.StatusName == "ABSENT"),
                ClassesNeedingAttendance = await _context.Schedules.CountAsync(x => x.Class.TeacherId == currentUserId && x.ScheduleDate == today && !x.AttendanceChecks.Any()),
                OutstandingTuitionInMyClasses = await _context.TuitionFees
                    .Where(x => x.Class != null && x.Class.TeacherId == currentUserId)
                    .SumAsync(x => (decimal?)(x.TotalAmount - (x.PaidAmount ?? 0))) ?? 0,
                TodaySchedules = await GetScheduleItemsAsync(x => x.Class.TeacherId == currentUserId && x.ScheduleDate == today),
                UpcomingSchedules = await GetScheduleItemsAsync(x => x.Class.TeacherId == currentUserId && x.ScheduleDate > today),
                LatestNotifications = await GetLatestNotificationsAsync(currentUserId),
                ClassSummaries = await _context.Classes
                    .Where(x => x.TeacherId == currentUserId)
                    .Select(x => new DashboardClassSummaryItemDto
                    {
                        ClassName = x.ClassName,
                        StudentCount = x.ClassStudents.Count(cs => cs.IsEnable != false),
                        NextScheduleDate = x.Schedules
                            .Where(s => s.ScheduleDate >= today)
                            .OrderBy(s => s.ScheduleDate)
                            .Select(s => (DateOnly?)s.ScheduleDate)
                            .FirstOrDefault()
                    })
                    .ToListAsync()
            };
        }

        public async Task<ParentDashboardDto> GetParentDashboardAsync(Guid currentUserId)
        {
            var children = await _context.StudentGuardians
                .AsNoTracking()
                .Where(x => x.GuardianId == currentUserId)
                .Select(x => new
                {
                    x.StudentId,
                    StudentName = x.Student.FullName,
                    ClassName = x.Student.ClassStudents
                        .Where(cs => cs.IsEnable != false)
                        .Select(cs => cs.Class.ClassName)
                        .FirstOrDefault(),
                    PresentCount = x.Student.AttendanceCheckStudents.Count(ac => ac.Status.StatusName == "PRESENT"),
                    TotalAttendance = x.Student.AttendanceCheckStudents.Count(),
                    RemainingTuition = x.Student.TuitionFees.Sum(tf => tf.TotalAmount - (tf.PaidAmount ?? 0)),
                    PaidTuition = x.Student.TuitionFees.Sum(tf => tf.PaidAmount ?? 0),
                    AbsentCount = x.Student.AttendanceCheckStudents.Count(ac => ac.Status.StatusName == "ABSENT")
                })
                .ToListAsync();

            var totalAttendance = children.Sum(x => x.TotalAttendance);
            var totalPresent = children.Sum(x => x.PresentCount);

            return new ParentDashboardDto
            {
                MyChildren = children.Count,
                TotalAbsentCount = children.Sum(x => x.AbsentCount),
                TotalTuitionPaid = children.Sum(x => x.PaidTuition),
                TotalTuitionRemaining = children.Sum(x => x.RemainingTuition),
                OverallAttendanceRate = totalAttendance == 0 ? 0 : Math.Round((double)totalPresent * 100 / totalAttendance, 2),
                Children = children.Select(x => new DashboardChildItemDto
                {
                    ChildName = x.StudentName,
                    ClassName = x.ClassName ?? "N/A",
                    AttendanceRate = x.TotalAttendance == 0 ? 0 : Math.Round((double)x.PresentCount * 100 / x.TotalAttendance, 2),
                    RemainingTuition = x.RemainingTuition
                }).ToList(),
                LatestNotifications = await GetLatestNotificationsAsync(currentUserId)
            };
        }

        public async Task<StudentDashboardDto> GetStudentDashboardAsync(Guid currentUserId)
        {
            var attendanceTotal = await _context.AttendanceChecks.CountAsync(x => x.StudentId == currentUserId);
            var presentCount = await _context.AttendanceChecks.CountAsync(x => x.StudentId == currentUserId && x.Status.StatusName == "PRESENT");

            return new StudentDashboardDto
            {
                MyClasses = await _context.ClassStudents.CountAsync(x => x.StudentId == currentUserId && x.IsEnable != false),
                AbsentCount = await _context.AttendanceChecks.CountAsync(x => x.StudentId == currentUserId && x.Status.StatusName == "ABSENT"),
                TuitionRemaining = await _context.TuitionFees
                    .Where(x => x.StudentId == currentUserId)
                    .SumAsync(x => (decimal?)(x.TotalAmount - (x.PaidAmount ?? 0))) ?? 0,
                AttendanceRate = attendanceTotal == 0 ? 0 : Math.Round((double)presentCount * 100 / attendanceTotal, 2),
                UpcomingSchedules = await GetScheduleItemsAsync(x => x.Class.ClassStudents.Any(cs => cs.StudentId == currentUserId && cs.IsEnable != false) && x.ScheduleDate >= DateOnly.FromDateTime(DateTime.Today)),
                LatestNotifications = await GetLatestNotificationsAsync(currentUserId)
            };
        }

        private async Task<int> CountUsersByRoleAsync(string roleName)
        {
            return await _context.Users.CountAsync(x => x.Role.RoleName == roleName);
        }

        private async Task<double> GetOverallAttendanceRateAsync()
        {
            var total = await _context.AttendanceChecks.CountAsync();
            if (total == 0)
            {
                return 0;
            }

            var present = await _context.AttendanceChecks.CountAsync(x => x.Status.StatusName == "PRESENT");
            return Math.Round((double)present * 100 / total, 2);
        }

        private async Task<List<DashboardNotificationItemDto>> GetLatestNotificationsAsync(Guid? receiverId = null)
        {
            var query = _context.Notifications.AsNoTracking().AsQueryable();

            if (receiverId.HasValue)
            {
                query = query.Where(x => x.ReceiverId == receiverId.Value);
            }

            return await query
                .OrderByDescending(x => x.CreatedAt)
                .Take(5)
                .Select(x => new DashboardNotificationItemDto
                {
                    Title = x.Title,
                    CreatedAt = x.CreatedAt,
                    IsRead = x.IsRead == true
                })
                .ToListAsync();
        }

        private async Task<List<DashboardScheduleItemDto>> GetScheduleItemsAsync(System.Linq.Expressions.Expression<Func<Models.Entities.Schedule, bool>> predicate)
        {
            return await _context.Schedules
                .AsNoTracking()
                .Include(x => x.Class)
                .Where(predicate)
                .OrderBy(x => x.ScheduleDate)
                .ThenBy(x => x.StartTime)
                .Take(5)
                .Select(x => new DashboardScheduleItemDto
                {
                    ClassName = x.Class.ClassName,
                    ScheduleDate = x.ScheduleDate,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    Topic = x.Topic
                })
                .ToListAsync();
        }
    }
}
