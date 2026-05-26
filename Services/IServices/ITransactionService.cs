using FapWeb.Models.Dtos.PaginatedDtos;
using FapWeb.Models.Dtos.TransactionHistoryDtos;

namespace FapWeb.Services.IServices
{
    public interface ITransactionService
    {
        Task<List<TransactionHistoryResponseDto>> GetTransactionAsync(TransactionHistoryRequestDto request);
    }
}
