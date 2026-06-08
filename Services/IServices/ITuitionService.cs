using FapWeb.Models.Dtos.TuitionDtos;

namespace FapWeb.Services.IServices
{
    public interface ITuitionService
    {
        Task<List<TuitionStudentStatusDto>> GetTuitionStatusesAsync(Guid currentUserId, string? roleName);

        Task<PaymentCreateDto?> GetPaymentCreateAsync(Guid tuitionFeeId, Guid currentUserId, string? roleName);

        Task<bool> RecordPaymentAsync(PaymentCreateDto request, Guid receiverId, string? roleName);

        Task<List<PaymentHistoryDto>> GetPaymentHistoryAsync(Guid currentUserId, string? roleName, Guid? tuitionFeeId = null);

        Task<bool> SendTuitionReminderAsync(Guid tuitionFeeId, Guid teacherId, string? roleName);
    }
}
