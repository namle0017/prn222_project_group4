using FapWeb.Models.Data;
using FapWeb.Models.Dtos.TuitionDtos;
using FapWeb.Models.Entities;
using FapWeb.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace FapWeb.Services.Service
{
    public class TuitionService : ITuitionService
    {
        private readonly PostgresContext _context;
        private readonly INotificationService _notificationService;

        public TuitionService(PostgresContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<List<TuitionStudentStatusDto>> GetTuitionStatusesAsync(Guid currentUserId, string? roleName)
        {
            var query = _context.TuitionFees
                .AsNoTracking()
                .Include(x => x.Student)
                .Include(x => x.Class)
                .AsQueryable();

            if (IsTeacher(roleName))
            {
                query = query.Where(x => x.Class != null && x.Class.TeacherId == currentUserId);
            }
            else if (IsStudent(roleName))
            {
                query = query.Where(x => x.StudentId == currentUserId);
            }
            else if (IsParent(roleName))
            {
                query = query.Where(x => x.Student.StudentGuardianStudents.Any(sg => sg.GuardianId == currentUserId));
            }

            return await query
                .OrderBy(x => x.Student.FullName)
                .Select(x => new TuitionStudentStatusDto
                {
                    TuitionFeeId = x.Id,
                    StudentId = x.StudentId,
                    StudentName = x.Student.FullName,
                    ClassName = x.Class != null ? x.Class.ClassName : null,
                    RequiredFee = x.TotalAmount,
                    PaidAmount = x.PaidAmount ?? 0,
                    RemainingAmount = x.TotalAmount - (x.PaidAmount ?? 0),
                    StatusName = GetTuitionStatusName(x.TotalAmount, x.PaidAmount ?? 0),
                    DueDate = x.DueDate
                })
                .ToListAsync();
        }

        public async Task<PaymentCreateDto?> GetPaymentCreateAsync(Guid tuitionFeeId, Guid currentUserId, string? roleName)
        {
            if (!CanManageTuition(roleName))
            {
                return null;
            }

            var tuitionFee = await _context.TuitionFees
                .AsNoTracking()
                .Include(x => x.Student)
                .Include(x => x.Class)
                .FirstOrDefaultAsync(x => x.Id == tuitionFeeId && (!IsTeacher(roleName) || (x.Class != null && x.Class.TeacherId == currentUserId)));

            if (tuitionFee == null)
            {
                return null;
            }

            return new PaymentCreateDto
            {
                TuitionFeeId = tuitionFee.Id,
                StudentId = tuitionFee.StudentId,
                StudentName = tuitionFee.Student.FullName,
                RemainingAmount = Math.Max(0, tuitionFee.TotalAmount - (tuitionFee.PaidAmount ?? 0)),
                PaymentDate = DateTime.Today
            };
        }

        public async Task<bool> RecordPaymentAsync(PaymentCreateDto request, Guid receiverId, string? roleName)
        {
            if (!CanManageTuition(roleName) || request.Amount <= 0)
            {
                return false;
            }

            var tuitionFee = await _context.TuitionFees
                .Include(x => x.Class)
                .FirstOrDefaultAsync(x => x.Id == request.TuitionFeeId && (!IsTeacher(roleName) || (x.Class != null && x.Class.TeacherId == receiverId)));

            if (tuitionFee == null)
            {
                return false;
            }

            tuitionFee.PaidAmount = (tuitionFee.PaidAmount ?? 0) + request.Amount;
            tuitionFee.UpdatedAt = DateTime.UtcNow;

            var remainingAmount = Math.Max(0, tuitionFee.TotalAmount - (tuitionFee.PaidAmount ?? 0));
            var tuitionStatusIds = await EnsureTuitionStatusesAsync();
            tuitionFee.StatusId = tuitionStatusIds[GetTuitionStatusName(tuitionFee.TotalAmount, tuitionFee.PaidAmount ?? 0).ToUpperInvariant()];

            await _context.Transactions.AddAsync(new Transaction
            {
                TuitionFeeId = tuitionFee.Id,
                Amount = request.Amount,
                PaymentMethod = "Manual",
                TransactionCode = string.IsNullOrWhiteSpace(request.Note)
                    ? $"PAY-{DateTime.UtcNow:yyyyMMddHHmmss}"
                    : request.Note.Trim(),
                PaidBy = receiverId,
                CreatedAt = request.PaymentDate,
                UpdatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return remainingAmount >= 0;
        }

        public async Task<List<PaymentHistoryDto>> GetPaymentHistoryAsync(Guid currentUserId, string? roleName, Guid? tuitionFeeId = null)
        {
            var query = _context.Transactions
                .AsNoTracking()
                .Include(x => x.Status)
                .Include(x => x.PaidByNavigation)
                .Include(x => x.TuitionFee)
                    .ThenInclude(x => x.Student)
                .Include(x => x.TuitionFee)
                    .ThenInclude(x => x.Class)
                .AsQueryable();

            if (IsTeacher(roleName))
            {
                query = query.Where(x => x.TuitionFee.Class != null && x.TuitionFee.Class.TeacherId == currentUserId);
            }
            else if (IsStudent(roleName))
            {
                query = query.Where(x => x.TuitionFee.StudentId == currentUserId);
            }
            else if (IsParent(roleName))
            {
                query = query.Where(x => x.TuitionFee.Student.StudentGuardianStudents.Any(sg => sg.GuardianId == currentUserId));
            }

            if (tuitionFeeId.HasValue)
            {
                query = query.Where(x => x.TuitionFeeId == tuitionFeeId.Value);
            }

            return await query
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new PaymentHistoryDto
                {
                    TransactionId = x.Id,
                    StudentId = x.TuitionFee.StudentId,
                    StudentName = x.TuitionFee.Student.FullName,
                    Amount = x.Amount,
                    PaymentDate = x.CreatedAt,
                    RecordedByName = x.PaidByNavigation != null ? x.PaidByNavigation.FullName : "N/A",
                    StatusName = x.Status != null ? x.Status.StatusName : "RECORDED",
                    Note = x.TransactionCode
                })
                .ToListAsync();
        }

        public async Task<bool> SendTuitionReminderAsync(Guid tuitionFeeId, Guid teacherId, string? roleName)
        {
            if (!CanManageTuition(roleName))
            {
                return false;
            }

            var tuitionFee = await _context.TuitionFees
                .AsNoTracking()
                .Include(x => x.Student)
                .Include(x => x.Class)
                .FirstOrDefaultAsync(x => x.Id == tuitionFeeId && (!IsTeacher(roleName) || (x.Class != null && x.Class.TeacherId == teacherId)));

            if (tuitionFee == null)
            {
                return false;
            }

            var remainingAmount = Math.Max(0, tuitionFee.TotalAmount - (tuitionFee.PaidAmount ?? 0));
            if (remainingAmount <= 0)
            {
                return false;
            }

            var dueDate = tuitionFee.DueDate?.ToDateTime(TimeOnly.MinValue);
            await _notificationService.CreateTuitionReminderNotificationAsync(teacherId, tuitionFee.StudentId, remainingAmount, dueDate);
            return true;
        }

        private async Task<Dictionary<string, int>> EnsureTuitionStatusesAsync()
        {
            var existingStatuses = await _context.TuitionFeeStatuses.ToListAsync();
            var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var status in existingStatuses)
            {
                result[status.StatusName.ToUpperInvariant()] = status.Id;
            }

            var requiredStatuses = new[]
            {
                new TuitionFeeStatus { Id = 1, StatusName = "PAID" },
                new TuitionFeeStatus { Id = 2, StatusName = "PARTIAL" },
                new TuitionFeeStatus { Id = 3, StatusName = "UNPAID" }
            };

            var added = false;

            foreach (var status in requiredStatuses)
            {
                if (!result.ContainsKey(status.StatusName))
                {
                    await _context.TuitionFeeStatuses.AddAsync(status);
                    result[status.StatusName] = status.Id;
                    added = true;
                }
            }

            if (added)
            {
                await _context.SaveChangesAsync();
            }

            return result;
        }

        private static string GetTuitionStatusName(decimal totalAmount, decimal paidAmount)
        {
            var remaining = totalAmount - paidAmount;

            if (remaining <= 0)
            {
                return "PAID";
            }

            if (paidAmount > 0)
            {
                return "PARTIAL";
            }

            return "UNPAID";
        }

        private static bool CanManageTuition(string? roleName)
        {
            return string.Equals(roleName, "ADMIN", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(roleName, "TEACHER", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsTeacher(string? roleName)
        {
            return string.Equals(roleName, "TEACHER", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsStudent(string? roleName)
        {
            return string.Equals(roleName, "STUDENT", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsParent(string? roleName)
        {
            return string.Equals(roleName, "PARENT", StringComparison.OrdinalIgnoreCase);
        }
    }
}
