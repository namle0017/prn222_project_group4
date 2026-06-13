using FapWeb.Models.Data;
using FapWeb.Models.Dtos.PaginatedDtos;
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

        public async Task<List<TransactionHistoryResponseDto>> GetTransactionAsync(TransactionHistoryRequestDto request)
        {
            if (request.QueryUser.UserId != null)
            {
                return await GetTransactionHistoryById(request.QueryUser.UserId.Value, request.Paginated.PageNumber, request.Paginated.PageSize);
            }
            else if (!string.IsNullOrWhiteSpace(request.QueryUser.SearchTerm))
            {
                return await SearchTransactionAsync(request.QueryUser.SearchTerm, request.Paginated);
            }
            else
            {
                return await GetTransactionHistory(request.Paginated);
            }
        }

        private async Task<List<TransactionHistoryResponseDto>> GetTransactionHistoryById(Guid userId, int pageNumber, int pageSize)
        {
            List<TransactionHistoryResponseDto> transactionHistory = await _context.Transactions
                                                                                .Include(x => x.Status)
                                                                                .Where(x => x.PaidBy == userId)
                                                                                .OrderByDescending(x => x.CreatedAt)
                                                                                .Skip((pageNumber - 1) * pageSize)
                                                                                .Take(pageSize)
                                                                                .Select(x => new TransactionHistoryResponseDto
                                                                                {
                                                                                    Id = x.Id,
                                                                                    TransactionType = x.PaymentMethod,
                                                                                    Amount = x.Amount,
                                                                                    TransactionStatus = x.Status != null ? x.Status.StatusName : "SUCCESS",
                                                                                    CreatedAt = x.CreatedAt,
                                                                                    UpdatedAt = x.UpdatedAt
                                                                                }).ToListAsync();
            return transactionHistory;
        }


        private async Task<List<TransactionHistoryResponseDto>> GetTransactionHistory(PaginatedDto request)
        {
            List<TransactionHistoryResponseDto> transactionHistory = await _context.Transactions
                                                                                .Include(x => x.Status)
                                                                                .OrderByDescending(x => x.CreatedAt)
                                                                                .Skip((request.PageNumber - 1) * request.PageSize)
                                                                                .Take(request.PageSize)
                                                                                .Select(x => new TransactionHistoryResponseDto
                                                                                {
                                                                                    Id = x.Id,
                                                                                    TransactionType = x.PaymentMethod,
                                                                                    Amount = x.Amount,
                                                                                    TransactionStatus = x.Status != null ? x.Status.StatusName : "SUCCESS",
                                                                                    CreatedAt = x.CreatedAt,
                                                                                    UpdatedAt = x.UpdatedAt
                                                                                }).ToListAsync();
            return transactionHistory;
        }

        private async Task<List<TransactionHistoryResponseDto>> SearchTransactionAsync(string searchTerm, PaginatedDto paginated)
        {
            string term = searchTerm.Trim().ToLower();

            List<TransactionHistoryResponseDto> transactionHistory = await _context.Transactions
                .Include(x => x.Status)
                .Include(x => x.TuitionFee)
                    .ThenInclude(tf => tf.Student)
                        .ThenInclude(s => s.StudentGuardianStudents)
                            .ThenInclude(sg => sg.Guardian)
                .Where(x =>
                    // Tìm theo tên học sinh
                    x.TuitionFee.Student.FullName.ToLower().Contains(term) ||
                    // Tìm theo tên phụ huynh/người giám hộ
                    x.TuitionFee.Student.StudentGuardianStudents
                        .Any(sg => sg.Guardian.FullName.ToLower().Contains(term))
                )
                .OrderByDescending(x => x.CreatedAt)
                .Skip((paginated.PageNumber - 1) * paginated.PageSize)
                .Take(paginated.PageSize)
                .Select(x => new TransactionHistoryResponseDto
                {
                    Id = x.Id,
                    TransactionType = x.PaymentMethod,
                    Amount = x.Amount,
                    TransactionStatus = x.Status != null ? x.Status.StatusName : "SUCCESS",
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync();

            return transactionHistory;
        }
    }
}