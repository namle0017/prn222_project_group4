using FapWeb.Models.Dtos.ScheduleManagementDtos;

namespace FapWeb.Services.IServices
{
    public interface IScheduleManagementService
    {
        Task<ScheduleManagementIndexDto> GetIndexAsync(Guid? classId, DateOnly? dateFilter, Guid currentUserId, string? roleName);

        Task<ScheduleFormDto?> GetCreateModelAsync(Guid? classId, Guid currentUserId, string? roleName);

        Task<ScheduleFormDto?> GetEditModelAsync(Guid scheduleId, Guid currentUserId, string? roleName);

        Task<Guid?> CreateAsync(ScheduleFormDto request, Guid currentUserId, string? roleName);

        Task<bool> UpdateAsync(ScheduleFormDto request, Guid currentUserId, string? roleName);

        Task<ClassScheduleViewDto?> GetClassScheduleAsync(Guid classId, Guid currentUserId, string? roleName);
    }
}
