using FapWeb.Infrastructure;
using FapWeb.Models.Data;
using FapWeb.Models.Dtos.ScheduleManagementDtos;
using FapWeb.Models.Dtos.SharedDtos;
using FapWeb.Models.Entities;
using FapWeb.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace FapWeb.Services.Service
{
    public class ScheduleManagementService : IScheduleManagementService
    {
        private readonly PostgresContext _context;

        public ScheduleManagementService(PostgresContext context)
        {
            _context = context;
        }

        public async Task<ScheduleManagementIndexDto> GetIndexAsync(Guid? classId, DateOnly? dateFilter, Guid currentUserId, string? roleName)
        {
            if (!CanManageSchedules(roleName))
            {
                return new ScheduleManagementIndexDto();
            }

            var query = _context.Schedules
                .AsNoTracking()
                .Include(x => x.Class)
                    .ThenInclude(x => x.Teacher)
                .AsQueryable();

            if (AppRoles.IsTeacher(roleName))
            {
                query = query.Where(x => x.Class.TeacherId == currentUserId);
            }

            if (classId.HasValue)
            {
                query = query.Where(x => x.ClassId == classId.Value);
            }

            if (dateFilter.HasValue)
            {
                query = query.Where(x => x.ScheduleDate == dateFilter.Value);
            }

            var schedules = await query
                .OrderBy(x => x.ScheduleDate)
                .ThenBy(x => x.StartTime)
                .Select(x => new ScheduleListItemDto
                {
                    ScheduleId = x.Id,
                    ClassId = x.ClassId,
                    ClassName = x.Class.ClassName,
                    TeacherName = x.Class.Teacher != null ? x.Class.Teacher.FullName : null,
                    ScheduleDate = x.ScheduleDate,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    Topic = x.Topic
                })
                .ToListAsync();

            var today = DateOnly.FromDateTime(DateTime.Today);

            return new ScheduleManagementIndexDto
            {
                ClassIdFilter = classId,
                DateFilter = dateFilter,
                TotalSchedules = schedules.Count,
                TodaySchedules = schedules.Count(x => x.ScheduleDate == today),
                UpcomingSchedules = schedules.Count(x => x.ScheduleDate >= today),
                ClassesCovered = schedules.Select(x => x.ClassId).Distinct().Count(),
                ClassOptions = await GetClassOptionsAsync(currentUserId, roleName),
                Schedules = schedules
            };
        }

        public async Task<ScheduleFormDto?> GetCreateModelAsync(Guid? classId, Guid currentUserId, string? roleName)
        {
            var options = await GetClassOptionsAsync(currentUserId, roleName);
            if (!options.Any())
            {
                return null;
            }

            var model = new ScheduleFormDto
            {
                ClassOptions = options
            };

            if (classId.HasValue)
            {
                model.ClassId = classId.Value;
            }

            return model;
        }

        public async Task<ScheduleFormDto?> GetEditModelAsync(Guid scheduleId, Guid currentUserId, string? roleName)
        {
            if (!CanManageSchedules(roleName))
            {
                return null;
            }

            var restrictToOwnClasses = AppRoles.IsTeacher(roleName);
            var schedule = await _context.Schedules
                .AsNoTracking()
                .Include(x => x.Class)
                .FirstOrDefaultAsync(x => x.Id == scheduleId && (!restrictToOwnClasses || x.Class.TeacherId == currentUserId));

            if (schedule == null)
            {
                return null;
            }

            return new ScheduleFormDto
            {
                Id = schedule.Id,
                ClassId = schedule.ClassId,
                ScheduleDate = schedule.ScheduleDate,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                Topic = schedule.Topic,
                ClassOptions = await GetClassOptionsAsync(currentUserId, roleName)
            };
        }

        public async Task<Guid?> CreateAsync(ScheduleFormDto request, Guid currentUserId, string? roleName)
        {
            if (request.EndTime <= request.StartTime || !CanManageSchedules(roleName))
            {
                return null;
            }

            var restrictToOwnClasses = AppRoles.IsTeacher(roleName);
            var classExists = await _context.Classes
                .AnyAsync(x => x.Id == request.ClassId && (!restrictToOwnClasses || x.TeacherId == currentUserId));

            if (!classExists)
            {
                return null;
            }

            var duplicateExists = await _context.Schedules.AnyAsync(x =>
                x.ClassId == request.ClassId &&
                x.ScheduleDate == request.ScheduleDate &&
                x.StartTime == request.StartTime &&
                x.EndTime == request.EndTime);

            if (duplicateExists)
            {
                return null;
            }

            var entity = new Schedule
            {
                ClassId = request.ClassId,
                ScheduleDate = request.ScheduleDate,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Topic = request.Topic,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Schedules.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<bool> UpdateAsync(ScheduleFormDto request, Guid currentUserId, string? roleName)
        {
            if (request.Id == null || request.EndTime <= request.StartTime || !CanManageSchedules(roleName))
            {
                return false;
            }

            var restrictToOwnClasses = AppRoles.IsTeacher(roleName);
            var entity = await _context.Schedules
                .Include(x => x.Class)
                .FirstOrDefaultAsync(x => x.Id == request.Id.Value && (!restrictToOwnClasses || x.Class.TeacherId == currentUserId));

            if (entity == null)
            {
                return false;
            }

            // Cho phep doi buoi hoc sang lop khac, nhung lop dich cung phai thuoc quyen
            // cua nguoi dang sua, neu khong TEACHER co the day buoi hoc sang lop nguoi khac.
            var targetClassAllowed = await _context.Classes
                .AnyAsync(x => x.Id == request.ClassId && (!restrictToOwnClasses || x.TeacherId == currentUserId));

            if (!targetClassAllowed)
            {
                return false;
            }

            var duplicateExists = await _context.Schedules.AnyAsync(x =>
                x.Id != request.Id.Value &&
                x.ClassId == request.ClassId &&
                x.ScheduleDate == request.ScheduleDate &&
                x.StartTime == request.StartTime &&
                x.EndTime == request.EndTime);

            if (duplicateExists)
            {
                return false;
            }

            entity.ClassId = request.ClassId;
            entity.ScheduleDate = request.ScheduleDate;
            entity.StartTime = request.StartTime;
            entity.EndTime = request.EndTime;
            entity.Topic = request.Topic;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ClassScheduleViewDto?> GetClassScheduleAsync(Guid classId, Guid currentUserId, string? roleName)
        {
            if (!CanManageSchedules(roleName))
            {
                return null;
            }

            var restrictToOwnClasses = AppRoles.IsTeacher(roleName);
            var classEntity = await _context.Classes
                .AsNoTracking()
                .Include(x => x.Teacher)
                .Include(x => x.Schedules)
                .FirstOrDefaultAsync(x => x.Id == classId && (!restrictToOwnClasses || x.TeacherId == currentUserId));

            if (classEntity == null)
            {
                return null;
            }

            return new ClassScheduleViewDto
            {
                ClassId = classEntity.Id,
                ClassName = classEntity.ClassName,
                TeacherName = classEntity.Teacher?.FullName,
                Schedules = classEntity.Schedules
                    .OrderBy(x => x.ScheduleDate)
                    .ThenBy(x => x.StartTime)
                    .Select(x => new ScheduleListItemDto
                    {
                        ScheduleId = x.Id,
                        ClassId = x.ClassId,
                        ClassName = classEntity.ClassName,
                        TeacherName = classEntity.Teacher != null ? classEntity.Teacher.FullName : null,
                        ScheduleDate = x.ScheduleDate,
                        StartTime = x.StartTime,
                        EndTime = x.EndTime,
                        Topic = x.Topic
                    }).ToList()
            };
        }

        /// <summary>
        /// Chi ADMIN va TEACHER duoc quan ly thoi khoa bieu. Truoc day dieu kien
        /// "!IsTeacher(role) || ..." khien STUDENT/PARENT tao va sua duoc lich cua moi lop.
        /// </summary>
        private static bool CanManageSchedules(string? roleName)
        {
            return AppRoles.IsStaff(roleName);
        }

        private async Task<List<SelectOptionDto>> GetClassOptionsAsync(Guid currentUserId, string? roleName)
        {
            if (!CanManageSchedules(roleName))
            {
                return new List<SelectOptionDto>();
            }

            var query = _context.Classes.AsNoTracking().AsQueryable();

            if (AppRoles.IsTeacher(roleName))
            {
                query = query.Where(x => x.TeacherId == currentUserId);
            }

            return await query
                .OrderBy(x => x.ClassName)
                .Select(x => new SelectOptionDto
                {
                    Value = x.Id.ToString(),
                    Label = x.ClassName
                })
                .ToListAsync();
        }
    }
}
