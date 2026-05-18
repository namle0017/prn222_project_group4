using FapWeb.Models.Data;
using FapWeb.Models.Dtos.TransactionHistoryDtos;
using FapWeb.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace FapWeb.Services.Service
{
    public class TransactionService : ITransactionService
    {
        private readonly PostgresContext _context;
        public TransactionService(PostgresContext context)
        {
            _context = context;
        }
        public List<TransactionHistoryResponseDto> GetTransactionHistory(TransactionHistoryRequestDto request)
        {
            List<TransactionHistoryResponseDto> transactionHistory = _context.Transactions
                                                                                .Include(x => x.Status)
                                                                                .Where(x => x.PaidBy == request.UserId)
                                                                                .OrderByDescending(x => x.CreatedAt)
                                                                                .Skip((request.PageNumber - 1) * request.PageSize)
                                                                                .Take(request.PageSize)
                                                                                .Select(x => new TransactionHistoryResponseDto
                                                                                {
                                                                                    Id = x.Id,
                                                                                    TransactionType = x.PaymentMethod,
                                                                                    Amount = x.Amount,
                                                                                    TransactionStatus = x.Status.StatusName,
                                                                                    CreatedAt = x.CreatedAt,
                                                                                    UpdatedAt = x.UpdatedAt
                                                                                }).ToList();
            return transactionHistory;
        }
    }
}
