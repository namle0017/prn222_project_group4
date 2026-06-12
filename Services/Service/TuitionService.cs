using FapWeb.Models.Data;
using FapWeb.Models.Dtos.SePayDtos;
using FapWeb.Models.Dtos.TuitionDtos;
using FapWeb.Models.Entities;
using FapWeb.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace FapWeb.Services.Service
{
    public class TuitionService : ITuitionService
    {
        private const string TransactionStatusPending = "PENDING";
        private const string TransactionStatusSuccess = "SUCCESS";

        private readonly PostgresContext _context;
        private readonly INotificationService _notificationService;
        private readonly ISePayService _sePayService;

        public TuitionService(PostgresContext context, INotificationService notificationService, ISePayService sePayService)
        {
            _context = context;
            _notificationService = notificationService;
            _sePayService = sePayService;
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

        public async Task<TuitionFeeCreateDto?> GetCreateFeeModelAsync(Guid currentUserId, string? roleName)
        {
            if (!CanManageTuition(roleName))
            {
                return null;
            }

            return new TuitionFeeCreateDto
            {
                ClassOptions = await _context.Classes
                    .AsNoTracking()
                    .Where(x => !IsTeacher(roleName) || x.TeacherId == currentUserId)
                    .OrderBy(x => x.ClassName)
                    .Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(x.ClassName, x.Id.ToString()))
                    .ToListAsync()
            };
        }

        public async Task<(int Created, int Skipped, string? Error)> GenerateClassFeesAsync(TuitionFeeCreateDto request, Guid currentUserId, string? roleName)
        {
            if (!CanManageTuition(roleName) || request.TotalAmount <= 0 || !request.ClassId.HasValue)
            {
                return (0, 0, "Yêu cầu không hợp lệ.");
            }

            if (!DateOnly.TryParse($"{request.BillingMonth}-01", out var monthStart))
            {
                return (0, 0, "Tháng học phí không hợp lệ.");
            }

            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var classValid = await _context.Classes
                .AnyAsync(x => x.Id == request.ClassId.Value && (!IsTeacher(roleName) || x.TeacherId == currentUserId));
            if (!classValid)
            {
                return (0, 0, "Lớp học không hợp lệ hoặc bạn không có quyền với lớp này.");
            }

            var studentIds = await _context.ClassStudents
                .AsNoTracking()
                .Where(x => x.ClassId == request.ClassId.Value && x.IsEnable != false)
                .Select(x => x.StudentId)
                .Distinct()
                .ToListAsync();

            if (studentIds.Count == 0)
            {
                return (0, 0, "Lớp này chưa có học sinh nào.");
            }

            // Học sinh đã có học phí của lớp này trong tháng được chọn thì bỏ qua
            var existingStudentIds = await _context.TuitionFees
                .AsNoTracking()
                .Where(x => x.ClassId == request.ClassId.Value &&
                            studentIds.Contains(x.StudentId) &&
                            x.DueDate >= monthStart && x.DueDate <= monthEnd)
                .Select(x => x.StudentId)
                .Distinct()
                .ToListAsync();

            var newStudentIds = studentIds.Except(existingStudentIds).ToList();
            if (newStudentIds.Count == 0)
            {
                return (0, existingStudentIds.Count, "Tất cả học sinh trong lớp đã có học phí cho tháng này.");
            }

            var tuitionStatusIds = await EnsureTuitionStatusesAsync();
            var now = DateTime.UtcNow;

            foreach (var studentId in newStudentIds)
            {
                await _context.TuitionFees.AddAsync(new TuitionFee
                {
                    Id = Guid.NewGuid(),
                    StudentId = studentId,
                    ClassId = request.ClassId,
                    TotalAmount = request.TotalAmount,
                    PaidAmount = 0,
                    DueDate = monthEnd,
                    StatusId = tuitionStatusIds["UNPAID"],
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }

            await _context.SaveChangesAsync();
            return (newStudentIds.Count, existingStudentIds.Count, null);
        }

        public async Task<SePayCheckoutFormDto?> CreateOnlinePaymentAsync(Guid tuitionFeeId, Guid currentUserId, string? roleName, string baseCallbackUrl)
        {
            var tuitionFee = await _context.TuitionFees
                .Include(x => x.Student)
                .Include(x => x.Class)
                .FirstOrDefaultAsync(x => x.Id == tuitionFeeId);

            if (tuitionFee == null || !await CanAccessTuitionFeeAsync(tuitionFee, currentUserId, roleName))
            {
                return null;
            }

            var remainingAmount = tuitionFee.TotalAmount - (tuitionFee.PaidAmount ?? 0);
            if (remainingAmount <= 0)
            {
                return null;
            }

            var invoiceNumber = $"INV{DateTime.UtcNow:yyyyMMddHHmmss}{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";

            var transactionStatusIds = await EnsureTransactionStatusesAsync();
            await _context.Transactions.AddAsync(new Transaction
            {
                TuitionFeeId = tuitionFee.Id,
                Amount = remainingAmount,
                PaymentMethod = "SePay",
                TransactionCode = invoiceNumber,
                StatusId = transactionStatusIds[TransactionStatusPending],
                PaidBy = currentUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            var callbackUrl = $"{baseCallbackUrl.TrimEnd('/')}/Tuition/PaymentCallback?invoice={invoiceNumber}";

            return _sePayService.BuildCheckoutForm(new SePayCheckoutOrderDto
            {
                Amount = remainingAmount,
                Description = $"Thanh toan hoc phi {tuitionFee.Student.FullName}",
                InvoiceNumber = invoiceNumber,
                CustomerId = currentUserId.ToString("N"),
                SuccessUrl = $"{callbackUrl}&result=success",
                ErrorUrl = $"{callbackUrl}&result=error",
                CancelUrl = $"{callbackUrl}&result=cancel"
            });
        }

        public async Task<bool> FinalizeOnlinePaymentAsync(string invoiceNumber, string statusName)
        {
            if (string.IsNullOrWhiteSpace(invoiceNumber))
            {
                return false;
            }

            var transactionStatusIds = await EnsureTransactionStatusesAsync();
            if (!transactionStatusIds.TryGetValue(statusName.ToUpperInvariant(), out var newStatusId))
            {
                return false;
            }

            var transaction = await _context.Transactions
                .Include(x => x.TuitionFee)
                .FirstOrDefaultAsync(x =>
                    x.TransactionCode == invoiceNumber &&
                    x.StatusId == transactionStatusIds[TransactionStatusPending]);

            if (transaction == null)
            {
                return false;
            }

            transaction.StatusId = newStatusId;
            transaction.UpdatedAt = DateTime.UtcNow;

            var isSuccess = string.Equals(statusName, TransactionStatusSuccess, StringComparison.OrdinalIgnoreCase);
            if (isSuccess)
            {
                var tuitionFee = transaction.TuitionFee;
                tuitionFee.PaidAmount = (tuitionFee.PaidAmount ?? 0) + transaction.Amount;
                tuitionFee.UpdatedAt = DateTime.UtcNow;

                var tuitionStatusIds = await EnsureTuitionStatusesAsync();
                tuitionFee.StatusId = tuitionStatusIds[GetTuitionStatusName(tuitionFee.TotalAmount, tuitionFee.PaidAmount ?? 0).ToUpperInvariant()];
            }

            await _context.SaveChangesAsync();
            return isSuccess;
        }

        private async Task<bool> CanAccessTuitionFeeAsync(TuitionFee tuitionFee, Guid currentUserId, string? roleName)
        {
            if (string.Equals(roleName, "ADMIN", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (IsTeacher(roleName))
            {
                return tuitionFee.Class != null && tuitionFee.Class.TeacherId == currentUserId;
            }

            if (IsStudent(roleName))
            {
                return tuitionFee.StudentId == currentUserId;
            }

            if (IsParent(roleName))
            {
                return await _context.StudentGuardians
                    .AnyAsync(sg => sg.StudentId == tuitionFee.StudentId && sg.GuardianId == currentUserId);
            }

            return false;
        }

        private async Task<Dictionary<string, int>> EnsureTransactionStatusesAsync()
        {
            var existingStatuses = await _context.TransactionStatuses.ToListAsync();
            var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var status in existingStatuses)
            {
                result[status.StatusName.ToUpperInvariant()] = status.Id;
            }

            var requiredStatusNames = new[] { TransactionStatusPending, TransactionStatusSuccess, "FAILED", "CANCELLED" };
            var nextId = existingStatuses.Count == 0 ? 1 : existingStatuses.Max(x => x.Id) + 1;
            var added = false;

            foreach (var statusName in requiredStatusNames)
            {
                if (result.ContainsKey(statusName))
                {
                    continue;
                }

                await _context.TransactionStatuses.AddAsync(new TransactionStatus { Id = nextId, StatusName = statusName });
                result[statusName] = nextId;
                nextId++;
                added = true;
            }

            if (added)
            {
                await _context.SaveChangesAsync();
            }

            return result;
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
