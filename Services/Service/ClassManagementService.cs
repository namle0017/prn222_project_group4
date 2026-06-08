using FapWeb.Infrastructure;
using FapWeb.Models.Data;
using FapWeb.Models.Dtos.ClassManagementDtos;
using FapWeb.Models.Dtos.SharedDtos;
using FapWeb.Models.Entities;
using FapWeb.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace FapWeb.Services.Service
{
    public class ClassManagementService : IClassManagementService
    {
        private readonly PostgresContext _context;

        public ClassManagementService(PostgresContext context)
        {
            _context = context;
        }

        public async Task<ClassManagementIndexDto> GetIndexAsync(string? searchTerm, Guid? teacherId, string? statusFilter, Guid currentUserId, string? roleName)
        {
            var query = _context.Classes
                .AsNoTracking()
                .Include(x => x.Teacher)
                .Include(x => x.ClassStudents)
                .Include(x => x.Schedules)
                .AsQueryable();

            if (AppRoles.IsTeacher(roleName))
            {
                query = query.Where(x => x.TeacherId == currentUserId);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    x.ClassName.ToLower().Contains(term) ||
                    (x.RoomName != null && x.RoomName.ToLower().Contains(term)) ||
                    (x.Teacher != null && x.Teacher.FullName.ToLower().Contains(term)));
            }

            if (teacherId.HasValue)
            {
                query = query.Where(x => x.TeacherId == teacherId.Value);
            }

            var classes = await query
                .OrderBy(x => x.ClassName)
                .Select(x => new ClassListItemDto
                {
                    ClassId = x.Id,
                    ClassName = x.ClassName,
                    TeacherId = x.TeacherId,
                    TeacherName = x.Teacher != null ? x.Teacher.FullName : null,
                    RoomName = x.RoomName,
                    StudentCount = x.ClassStudents.Count(cs => cs.IsEnable != false),
                    ScheduleCount = x.Schedules.Count,
                    StatusName = GetClassStatusName(x.StartDate, x.EndDate)
                })
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                classes = classes
                    .Where(x => x.StatusName.Equals(statusFilter, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return new ClassManagementIndexDto
            {
                SearchTerm = searchTerm,
                TeacherId = teacherId,
                StatusFilter = statusFilter,
                TotalClasses = classes.Count,
                ActiveClasses = classes.Count(x => x.StatusName == "ACTIVE"),
                TotalStudentsEnrolled = classes.Sum(x => x.StudentCount),
                ClassesWithoutSchedule = classes.Count(x => x.ScheduleCount == 0),
                TeacherOptions = await GetTeacherOptionsAsync(),
                Classes = classes
            };
        }

        public async Task<ClassFormDto> GetCreateModelAsync()
        {
            return new ClassFormDto
            {
                TeacherOptions = await GetTeacherOptionsAsync()
            };
        }

        public async Task<ClassFormDto?> GetEditModelAsync(Guid classId, Guid currentUserId, string? roleName)
        {
            var classEntity = await _context.Classes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == classId && (!AppRoles.IsTeacher(roleName) || x.TeacherId == currentUserId));

            if (classEntity == null)
            {
                return null;
            }

            return new ClassFormDto
            {
                Id = classEntity.Id,
                ClassName = classEntity.ClassName,
                TeacherId = classEntity.TeacherId,
                RoomName = classEntity.RoomName,
                MaxStudents = classEntity.MaxStudents,
                StartDate = classEntity.StartDate,
                EndDate = classEntity.EndDate,
                TotalSessions = classEntity.TotalSessions,
                TeacherOptions = await GetTeacherOptionsAsync()
            };
        }

        public async Task<Guid?> CreateAsync(ClassFormDto request)
        {
            var entity = new Class
            {
                ClassName = request.ClassName.Trim(),
                TeacherId = request.TeacherId,
                RoomName = request.RoomName,
                MaxStudents = request.MaxStudents,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                TotalSessions = request.TotalSessions,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Classes.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(ClassFormDto request, Guid currentUserId, string? roleName)
        {
            if (request.Id == null)
            {
                return false;
            }

            var entity = await _context.Classes
                .FirstOrDefaultAsync(x => x.Id == request.Id.Value && (!AppRoles.IsTeacher(roleName) || x.TeacherId == currentUserId));

            if (entity == null)
            {
                return false;
            }

            entity.ClassName = request.ClassName.Trim();
            entity.TeacherId = request.TeacherId;
            entity.RoomName = request.RoomName;
            entity.MaxStudents = request.MaxStudents;
            entity.StartDate = request.StartDate;
            entity.EndDate = request.EndDate;
            entity.TotalSessions = request.TotalSessions;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ClassDetailsDto?> GetDetailsAsync(Guid classId, Guid currentUserId, string? roleName)
        {
            var classEntity = await _context.Classes
                .AsNoTracking()
                .Include(x => x.Teacher)
                .Include(x => x.ClassStudents)
                    .ThenInclude(x => x.Student)
                        .ThenInclude(x => x.StudentGuardianStudents)
                            .ThenInclude(x => x.Guardian)
                .Include(x => x.Schedules)
                    .ThenInclude(x => x.AttendanceChecks)
                .Include(x => x.TuitionFees)
                .FirstOrDefaultAsync(x => x.Id == classId && (!AppRoles.IsTeacher(roleName) || x.TeacherId == currentUserId));

            if (classEntity == null)
            {
                return null;
            }

            return new ClassDetailsDto
            {
                ClassId = classEntity.Id,
                ClassName = classEntity.ClassName,
                TeacherId = classEntity.TeacherId,
                TeacherName = classEntity.Teacher?.FullName,
                RoomName = classEntity.RoomName,
                MaxStudents = classEntity.MaxStudents,
                StartDate = classEntity.StartDate,
                EndDate = classEntity.EndDate,
                TotalSessions = classEntity.TotalSessions,
                StatusName = GetClassStatusName(classEntity.StartDate, classEntity.EndDate),
                StudentCount = classEntity.ClassStudents.Count(x => x.IsEnable != false),
                ScheduleCount = classEntity.Schedules.Count,
                AttendanceRecordsCount = classEntity.Schedules.Sum(x => x.AttendanceChecks.Count),
                TotalExpectedTuition = classEntity.TuitionFees.Sum(x => x.TotalAmount),
                TotalCollectedTuition = classEntity.TuitionFees.Sum(x => x.PaidAmount ?? 0),
                TotalOutstandingTuition = classEntity.TuitionFees.Sum(x => x.TotalAmount - (x.PaidAmount ?? 0)),
                Students = classEntity.ClassStudents
                    .OrderBy(x => x.Student.FullName)
                    .Select(x => new ClassStudentItemDto
                    {
                        StudentId = x.StudentId,
                        StudentName = x.Student.FullName,
                        GuardianName = x.Student.StudentGuardianStudents
                            .OrderByDescending(sg => sg.IsPrimary == true)
                            .Select(sg => sg.Guardian.FullName)
                            .FirstOrDefault(),
                        EnrolledAt = x.EnrolledAt,
                        Status = x.Status,
                        IsActive = x.IsEnable != false
                    }).ToList(),
                Schedules = classEntity.Schedules
                    .OrderBy(x => x.ScheduleDate)
                    .ThenBy(x => x.StartTime)
                    .Select(x => new ClassScheduleItemDto
                    {
                        ScheduleId = x.Id,
                        ScheduleDate = x.ScheduleDate,
                        StartTime = x.StartTime,
                        EndTime = x.EndTime,
                        Topic = x.Topic,
                        AttendanceCount = x.AttendanceChecks.Count
                    }).ToList()
            };
        }

        public async Task<ClassStudentsViewDto?> GetStudentsAsync(Guid classId, Guid currentUserId, string? roleName)
        {
            var details = await GetDetailsAsync(classId, currentUserId, roleName);
            if (details == null)
            {
                return null;
            }

            return new ClassStudentsViewDto
            {
                ClassId = details.ClassId,
                ClassName = details.ClassName,
                Students = details.Students
            };
        }

        public async Task<AddStudentToClassDto?> GetAddStudentModelAsync(Guid classId, Guid currentUserId, string? roleName)
        {
            var classEntity = await _context.Classes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == classId && (!AppRoles.IsTeacher(roleName) || x.TeacherId == currentUserId));

            if (classEntity == null)
            {
                return null;
            }

            var enrolledStudentIds = await _context.ClassStudents
                .Where(x => x.ClassId == classId && x.IsEnable != false)
                .Select(x => x.StudentId)
                .ToListAsync();

            var students = await _context.Users
                .AsNoTracking()
                .Include(x => x.Role)
                .Where(x => x.Role.RoleName == AppRoles.Student && !enrolledStudentIds.Contains(x.Id))
                .OrderBy(x => x.FullName)
                .Select(x => new SelectOptionDto
                {
                    Value = x.Id.ToString(),
                    Label = x.FullName
                })
                .ToListAsync();

            return new AddStudentToClassDto
            {
                ClassId = classEntity.Id,
                ClassName = classEntity.ClassName,
                StudentOptions = students
            };
        }

        public async Task<bool> AddStudentAsync(AddStudentToClassDto request, Guid currentUserId, string? roleName)
        {
            var classEntity = await _context.Classes
                .FirstOrDefaultAsync(x => x.Id == request.ClassId && (!AppRoles.IsTeacher(roleName) || x.TeacherId == currentUserId));

            if (classEntity == null)
            {
                return false;
            }

            var studentExists = await _context.Users
                .Include(x => x.Role)
                .AnyAsync(x => x.Id == request.StudentId && x.Role.RoleName == AppRoles.Student);

            if (!studentExists)
            {
                return false;
            }

            var existing = await _context.ClassStudents
                .FirstOrDefaultAsync(x => x.ClassId == request.ClassId && x.StudentId == request.StudentId);

            if (existing != null)
            {
                existing.IsEnable = true;
                existing.Status = "ACTIVE";
                if (existing.EnrolledAt == null)
                {
                    existing.EnrolledAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return true;
            }

            await _context.ClassStudents.AddAsync(new ClassStudent
            {
                ClassId = request.ClassId,
                StudentId = request.StudentId,
                EnrolledAt = DateTime.UtcNow,
                Status = "ACTIVE",
                IsEnable = true
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveStudentAsync(Guid classId, Guid studentId, Guid currentUserId, string? roleName)
        {
            var classEntity = await _context.Classes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == classId && (!AppRoles.IsTeacher(roleName) || x.TeacherId == currentUserId));

            if (classEntity == null)
            {
                return false;
            }

            var enrollment = await _context.ClassStudents
                .FirstOrDefaultAsync(x => x.ClassId == classId && x.StudentId == studentId);

            if (enrollment == null)
            {
                return false;
            }

            enrollment.IsEnable = false;
            enrollment.Status = "INACTIVE";
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<List<SelectOptionDto>> GetTeacherOptionsAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .Include(x => x.Role)
                .Where(x => x.Role.RoleName == AppRoles.Teacher)
                .OrderBy(x => x.FullName)
                .Select(x => new SelectOptionDto
                {
                    Value = x.Id.ToString(),
                    Label = x.FullName
                })
                .ToListAsync();
        }

        private static string GetClassStatusName(DateOnly? startDate, DateOnly? endDate)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            if (endDate.HasValue && endDate.Value < today)
            {
                return "INACTIVE";
            }

            if (startDate.HasValue && startDate.Value > today)
            {
                return "UPCOMING";
            }

            return "ACTIVE";
        }
    }
}
