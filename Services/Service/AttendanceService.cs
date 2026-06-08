using FapWeb.Infrastructure;
using FapWeb.Models.Data;
using FapWeb.Models.Dtos.AttendanceDtos;
using FapWeb.Models.Dtos.SharedDtos;
using FapWeb.Models.Entities;
using FapWeb.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace FapWeb.Services.Service
{
    public class AttendanceService : IAttendanceService
    {
        private readonly PostgresContext _context;
        private readonly INotificationService _notificationService;

        public AttendanceService(PostgresContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<List<AttendanceClassDto>> GetAttendanceClassesAsync(Guid currentUserId, string? roleName)
        {
            var query = _context.Classes
                .AsNoTracking()
                .Include(x => x.Teacher)
                .Include(x => x.ClassStudents)
                .AsQueryable();

            if (AppRoles.IsTeacher(roleName))
            {
                query = query.Where(x => x.TeacherId == currentUserId);
            }

            return await query
                .OrderBy(x => x.ClassName)
                .Select(x => new AttendanceClassDto
                {
                    ClassId = x.Id,
                    ClassName = x.ClassName,
                    RoomName = x.RoomName,
                    StudentCount = x.ClassStudents.Count(cs => cs.IsEnable != false),
                    TeacherName = x.Teacher != null ? x.Teacher.FullName : null
                })
                .ToListAsync();
        }

        public async Task<AttendanceTakeViewModel?> GetAttendanceTakeViewAsync(Guid classId, Guid? scheduleId, DateTime? attendanceDate, Guid currentUserId, string? roleName)
        {
            if (!CanManageAttendance(roleName))
            {
                return null;
            }

            var classEntity = await _context.Classes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == classId && (!AppRoles.IsTeacher(roleName) || x.TeacherId == currentUserId));

            if (classEntity == null)
            {
                return null;
            }

            var selectedDate = attendanceDate?.Date ?? DateTime.Today;
            var selectedDateOnly = DateOnly.FromDateTime(selectedDate);

            var classSchedules = await _context.Schedules
                .AsNoTracking()
                .Where(x => x.ClassId == classId)
                .OrderByDescending(x => x.ScheduleDate)
                .ThenBy(x => x.StartTime)
                .ToListAsync();

            var scheduleOptions = classSchedules
                .Select(x => new SelectOptionDto
                {
                    Value = x.Id.ToString(),
                    Label = $"{x.ScheduleDate:dd/MM/yyyy} | {x.StartTime:HH\\:mm} - {x.EndTime:HH\\:mm}" +
                            (string.IsNullOrWhiteSpace(x.Topic) ? string.Empty : $" | {x.Topic}")
                })
                .ToList();

            var effectiveScheduleId = scheduleId;

            if (!effectiveScheduleId.HasValue)
            {
                effectiveScheduleId = await _context.Schedules
                    .AsNoTracking()
                    .Where(x => x.ClassId == classId && x.ScheduleDate == selectedDateOnly)
                    .OrderBy(x => x.StartTime)
                    .Select(x => (Guid?)x.Id)
                    .FirstOrDefaultAsync();
            }

            var schedule = effectiveScheduleId.HasValue
                ? await _context.Schedules
                .AsNoTracking()
                .Include(x => x.AttendanceChecks)
                    .ThenInclude(x => x.Status)
                .FirstOrDefaultAsync(x => x.Id == effectiveScheduleId.Value && x.ClassId == classId)
                : null;

            var model = new AttendanceTakeViewModel
            {
                ClassId = classEntity.Id,
                ClassName = classEntity.ClassName,
                AttendanceDate = schedule?.ScheduleDate.ToDateTime(TimeOnly.MinValue) ?? selectedDate,
                ScheduleId = schedule?.Id,
                ScheduleLabel = schedule == null
                    ? null
                    : $"{schedule.ScheduleDate:dd/MM/yyyy} | {schedule.StartTime:HH\\:mm} - {schedule.EndTime:HH\\:mm}" +
                      (string.IsNullOrWhiteSpace(schedule.Topic) ? string.Empty : $" | {schedule.Topic}"),
                ScheduleOptions = scheduleOptions
            };

            if (schedule == null)
            {
                model.ErrorMessage = $"No schedule exists for this class on {selectedDate:dd/MM/yyyy}. Please create a schedule first.";
                return model;
            }

            var students = await _context.ClassStudents
                .AsNoTracking()
                .Include(x => x.Student)
                .Where(x => x.ClassId == classId && x.IsEnable != false)
                .OrderBy(x => x.Student.FullName)
                .Select(x => new AttendanceStudentRowDto
                {
                    StudentId = x.StudentId,
                    StudentName = x.Student.FullName,
                    StatusName = "PRESENT"
                })
                .ToListAsync();

            if (schedule != null)
            {
                var existingStatuses = schedule.AttendanceChecks.ToDictionary(
                    x => x.StudentId,
                    x => x.Status.StatusName.ToUpperInvariant());

                foreach (var student in students)
                {
                    if (existingStatuses.TryGetValue(student.StudentId, out var statusName))
                    {
                        student.StatusName = statusName;
                    }
                }
            }

            model.Students = students;
            return model;
        }

        public async Task<bool> SaveAttendanceAsync(AttendanceSaveRequestDto request, Guid teacherId, string? roleName)
        {
            if (!CanManageAttendance(roleName) || request.Students.Count == 0 || !request.ScheduleId.HasValue)
            {
                return false;
            }

            var classEntity = await _context.Classes
                .FirstOrDefaultAsync(x => x.Id == request.ClassId && (!AppRoles.IsTeacher(roleName) || x.TeacherId == teacherId));

            if (classEntity == null)
            {
                return false;
            }

            var schedule = await _context.Schedules
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.ScheduleId.Value && x.ClassId == request.ClassId);

            if (schedule == null)
            {
                return false;
            }

            var attendanceDate = schedule.ScheduleDate.ToDateTime(schedule.StartTime);

            var statusIds = await EnsureAttendanceStatusesAsync();
            var studentIds = request.Students.Select(x => x.StudentId).Distinct().ToList();

            var enrolledStudentIds = await _context.ClassStudents
                .AsNoTracking()
                .Where(x => x.ClassId == request.ClassId && x.IsEnable != false)
                .Select(x => x.StudentId)
                .ToListAsync();

            if (studentIds.Except(enrolledStudentIds).Any())
            {
                return false;
            }

            var existingRecords = await _context.AttendanceChecks
                .Include(x => x.Status)
                .Where(x => x.ScheduleId == request.ScheduleId.Value && studentIds.Contains(x.StudentId))
                .ToListAsync();

            var existingByStudent = existingRecords.ToDictionary(x => x.StudentId);
            var absentNotifications = new HashSet<Guid>();

            foreach (var student in request.Students)
            {
                var normalizedStatus = NormalizeAttendanceStatus(student.StatusName);
                var nextStatusId = statusIds[normalizedStatus];

                if (existingByStudent.TryGetValue(student.StudentId, out var existing))
                {
                    var wasAbsent = IsAbsentStatus(existing.Status?.StatusName);

                    existing.StatusId = nextStatusId;
                    existing.CheckedBy = teacherId;
                    existing.CheckedAt = attendanceDate;
                    existing.UpdatedAt = DateTime.UtcNow;

                    if (normalizedStatus == "ABSENT" && !wasAbsent)
                    {
                        absentNotifications.Add(student.StudentId);
                    }
                }
                else
                {
                    await _context.AttendanceChecks.AddAsync(new AttendanceCheck
                    {
                        ScheduleId = request.ScheduleId.Value,
                        StudentId = student.StudentId,
                        StatusId = nextStatusId,
                        CheckedBy = teacherId,
                        CheckedAt = attendanceDate,
                        UpdatedAt = DateTime.UtcNow
                    });

                    if (normalizedStatus == "ABSENT")
                    {
                        absentNotifications.Add(student.StudentId);
                    }
                }
            }

            await _context.SaveChangesAsync();

            foreach (var absentStudentId in absentNotifications)
            {
                await _notificationService.CreateAbsentAttendanceNotificationAsync(teacherId, absentStudentId, attendanceDate);
            }

            return true;
        }

        public async Task<List<AttendanceHistoryDto>> GetAttendanceHistoryAsync(Guid currentUserId, string? roleName, Guid? studentId = null)
        {
            var query = _context.AttendanceChecks
                .AsNoTracking()
                .Include(x => x.Student)
                .Include(x => x.Status)
                .Include(x => x.CheckedByNavigation)
                .Include(x => x.Schedule)
                    .ThenInclude(x => x.Class)
                .AsQueryable();

            if (AppRoles.IsStudent(roleName))
            {
                query = query.Where(x => x.StudentId == currentUserId);
            }
            else if (AppRoles.IsParent(roleName))
            {
                query = query.Where(x => x.Student.StudentGuardianStudents.Any(sg => sg.GuardianId == currentUserId));
            }
            else if (AppRoles.IsTeacher(roleName))
            {
                query = query.Where(x => x.Schedule.Class.TeacherId == currentUserId);
            }

            if (studentId.HasValue)
            {
                query = query.Where(x => x.StudentId == studentId.Value);
            }

            return await query
                .OrderByDescending(x => x.Schedule.ScheduleDate)
                .ThenByDescending(x => x.CheckedAt)
                .Select(x => new AttendanceHistoryDto
                {
                    StudentId = x.StudentId,
                    StudentName = x.Student.FullName,
                    ClassName = x.Schedule.Class.ClassName,
                    AttendanceDate = x.Schedule.ScheduleDate.ToDateTime(TimeOnly.MinValue),
                    StatusName = x.Status.StatusName,
                    TeacherName = x.CheckedByNavigation != null ? x.CheckedByNavigation.FullName : null
                })
                .ToListAsync();
        }

        private async Task<Dictionary<string, int>> EnsureAttendanceStatusesAsync()
        {
            var existingStatuses = await _context.AttendanceCheckStatuses.ToListAsync();
            var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var status in existingStatuses)
            {
                result[status.StatusName.ToUpperInvariant()] = status.Id;
            }

            var requiredStatuses = new[]
            {
                new AttendanceCheckStatus { Id = 1, StatusName = "PRESENT" },
                new AttendanceCheckStatus { Id = 2, StatusName = "ABSENT" }
            };

            var added = false;

            foreach (var status in requiredStatuses)
            {
                if (!result.ContainsKey(status.StatusName))
                {
                    await _context.AttendanceCheckStatuses.AddAsync(status);
                    result[status.StatusName] = status.Id;
                    added = true;
                }
            }

            if (added)
            {
                await _context.SaveChangesAsync();
            }

            return result;
        }

        private static bool CanManageAttendance(string? roleName)
        {
            return AppRoles.IsAdmin(roleName) || AppRoles.IsTeacher(roleName);
        }

        private static string NormalizeAttendanceStatus(string? statusName)
        {
            return string.Equals(statusName, "ABSENT", StringComparison.OrdinalIgnoreCase) ? "ABSENT" : "PRESENT";
        }

        private static bool IsAbsentStatus(string? statusName)
        {
            return string.Equals(statusName, "ABSENT", StringComparison.OrdinalIgnoreCase);
        }
    }
}
