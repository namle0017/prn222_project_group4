using FapWeb.Models.Dtos.AttendanceDtos;

namespace FapWeb.Services.IServices
{
    public interface IAttendanceService
    {
        Task<List<AttendanceClassDto>> GetAttendanceClassesAsync(Guid currentUserId, string? roleName);

        Task<AttendanceTakeViewModel?> GetAttendanceTakeViewAsync(Guid classId, Guid? scheduleId, DateTime? attendanceDate, Guid currentUserId, string? roleName);

        Task<bool> SaveAttendanceAsync(AttendanceSaveRequestDto request, Guid teacherId, string? roleName);

        Task<List<AttendanceHistoryDto>> GetAttendanceHistoryAsync(Guid currentUserId, string? roleName, Guid? studentId = null);
    }
}
