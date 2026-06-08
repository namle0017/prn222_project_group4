using FapWeb.Models.Dtos.ClassManagementDtos;

namespace FapWeb.Services.IServices
{
    public interface IClassManagementService
    {
        Task<ClassManagementIndexDto> GetIndexAsync(string? searchTerm, Guid? teacherId, string? statusFilter, Guid currentUserId, string? roleName);

        Task<ClassFormDto> GetCreateModelAsync();

        Task<ClassFormDto?> GetEditModelAsync(Guid classId, Guid currentUserId, string? roleName);

        Task<Guid?> CreateAsync(ClassFormDto request);

        Task<bool> UpdateAsync(ClassFormDto request, Guid currentUserId, string? roleName);

        Task<ClassDetailsDto?> GetDetailsAsync(Guid classId, Guid currentUserId, string? roleName);

        Task<ClassStudentsViewDto?> GetStudentsAsync(Guid classId, Guid currentUserId, string? roleName);

        Task<AddStudentToClassDto?> GetAddStudentModelAsync(Guid classId, Guid currentUserId, string? roleName);

        Task<bool> AddStudentAsync(AddStudentToClassDto request, Guid currentUserId, string? roleName);

        Task<bool> RemoveStudentAsync(Guid classId, Guid studentId, Guid currentUserId, string? roleName);
    }
}
