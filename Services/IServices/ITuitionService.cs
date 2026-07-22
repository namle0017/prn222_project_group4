using FapWeb.Models.Dtos.SePayDtos;
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

        Task<TuitionFeeCreateDto?> GetCreateFeeModelAsync(Guid currentUserId, string? roleName);

        Task<(int Created, int Skipped, string? Error)> GenerateClassFeesAsync(TuitionFeeCreateDto request, Guid currentUserId, string? roleName);

        Task<SePayCheckoutFormDto?> CreateOnlinePaymentAsync(Guid tuitionFeeId, Guid currentUserId, string? roleName, string baseCallbackUrl);

        Task<bool> FinalizeOnlinePaymentAsync(string invoiceNumber, string statusName);

        Task<OtherFeeCreateDto?> GetCreateOtherFeeModelAsync(Guid currentUserId, string? roleName);

        Task<(int Created, string? Error)> CreateOtherFeeAsync(OtherFeeCreateDto request, Guid currentUserId, string? roleName);

        Task<List<PendingFeeApprovalDto>> GetPendingApprovalsAsync(string? roleName);

        Task<int> CountPendingApprovalsAsync(string? roleName);

        Task<bool> ApproveFeeAsync(Guid tuitionFeeId, Guid adminId, string? roleName);

        Task<bool> RejectFeeAsync(Guid tuitionFeeId, Guid adminId, string? roleName);
    }
}
