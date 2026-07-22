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
            var schedule = await _context.Schedules
                .AsNoTracking()
                .Include(x => x.Class)
                .FirstOrDefaultAsync(x => x.Id == scheduleId && (!AppRoles.IsTeacher(roleName) || x.Class.TeacherId == currentUserId));

            if (schedule == null)
            {
                return null;
            }

            int? remainingEdits = null;
            if (AppRoles.IsTeacher(roleName))
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var monthStart = new DateOnly(today.Year, today.Month, 1);
                var usedThisMonth = schedule.EditCountMonth == monthStart ? schedule.EditCount : 0;
                remainingEdits = Math.Max(0, MaxScheduleEditsPerMonth - usedThisMonth);
            }

            return new ScheduleFormDto
            {
                Id = schedule.Id,
                ClassId = schedule.ClassId,
                ScheduleDate = schedule.ScheduleDate,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                Topic = schedule.Topic,
                ClassOptions = await GetClassOptionsAsync(currentUserId, roleName),
                RemainingEditsThisMonth = remainingEdits
            };
        }

        public async Task<Guid?> CreateAsync(ScheduleFormDto request, Guid currentUserId, string? roleName)
        {
            if (request.EndTime <= request.StartTime)
            {
                return null;
            }

            var classExists = await _context.Classes
                .AnyAsync(x => x.Id == request.ClassId && (!AppRoles.IsTeacher(roleName) || x.TeacherId == currentUserId));

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

        // Teacher chỉ được sửa mỗi buổi lịch tối đa 3 lần trong 1 tháng. Admin không giới hạn.
        private const int MaxScheduleEditsPerMonth = 3;

        public async Task<(bool Ok, string? Error)> UpdateAsync(ScheduleFormDto request, Guid currentUserId, string? roleName)
        {
            if (request.Id == null || request.EndTime <= request.StartTime)
            {
                return (false, "Thời gian không hợp lệ.");
            }

            var entity = await _context.Schedules
                .Include(x => x.Class)
                .FirstOrDefaultAsync(x => x.Id == request.Id.Value && (!AppRoles.IsTeacher(roleName) || x.Class.TeacherId == currentUserId));

            if (entity == null)
            {
                return (false, "Không tìm thấy lịch hoặc bạn không có quyền với lịch này.");
            }

            var duplicateExists = await _context.Schedules.AnyAsync(x =>
                x.Id != request.Id.Value &&
                x.ClassId == request.ClassId &&
                x.ScheduleDate == request.ScheduleDate &&
                x.StartTime == request.StartTime &&
                x.EndTime == request.EndTime);

            if (duplicateExists)
            {
                return (false, "Đã tồn tại buổi học trùng ngày/giờ.");
            }

            if (AppRoles.IsTeacher(roleName))
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var monthStart = new DateOnly(today.Year, today.Month, 1);

                if (entity.EditCountMonth != monthStart)
                {
                    entity.EditCount = 0;
                    entity.EditCountMonth = monthStart;
                }

                if (entity.EditCount >= MaxScheduleEditsPerMonth)
                {
                    return (false, $"Đã đổi lịch {MaxScheduleEditsPerMonth} lần trong tháng này. Không thể đổi thêm.");
                }

                entity.EditCount++;
            }

            entity.ClassId = request.ClassId;
            entity.ScheduleDate = request.ScheduleDate;
            entity.StartTime = request.StartTime;
            entity.EndTime = request.EndTime;
            entity.Topic = request.Topic;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<ClassScheduleViewDto?> GetClassScheduleAsync(Guid classId, Guid currentUserId, string? roleName)
        {
            var classEntity = await _context.Classes
                .AsNoTracking()
                .Include(x => x.Teacher)
                .Include(x => x.Schedules)
                .FirstOrDefaultAsync(x => x.Id == classId && (!AppRoles.IsTeacher(roleName) || x.TeacherId == currentUserId));

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

        private async Task<List<SelectOptionDto>> GetClassOptionsAsync(Guid currentUserId, string? roleName)
        {
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
