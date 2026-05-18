using FapWeb.Models.Dtos.TransactionHistoryDtos;

namespace FapWeb.Services.IServices
{
    public interface ITransactionService
    {
        List<TransactionHistoryResponseDto> GetTransactionHistory(TransactionHistoryRequestDto request);
    }
}
