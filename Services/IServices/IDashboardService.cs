using FapWeb.Models.Dtos.DashboardDtos;

namespace FapWeb.Services.IServices
{
    public interface IDashboardService
    {
        Task<AdminDashboardDto> GetAdminDashboardAsync();

        Task<TeacherDashboardDto> GetTeacherDashboardAsync(Guid currentUserId);

        Task<ParentDashboardDto> GetParentDashboardAsync(Guid currentUserId);

        Task<StudentDashboardDto> GetStudentDashboardAsync(Guid currentUserId);
    }
}
