using FapWeb.Infrastructure;
using FapWeb.Models.Data;
using FapWeb.Models.Dtos.UserManagementDtos;
using FapWeb.Models.Entities;
using FapWeb.Services.IServices;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FapWeb.Services.Service
{
    public class UserManagementService : IUserManagementService
    {
        private const string DefaultAdminPhone = "0900000000";
        private const string DefaultAdminPassword = "Admin@123";

        private readonly PostgresContext _context;

        public UserManagementService(PostgresContext context)
        {
            _context = context;
        }

        public async Task<List<UserListItemDto>> GetUsersAsync(string? searchTerm, string? roleFilter)
        {
            var query = _context.Users
                .AsNoTracking()
                .Include(x => x.Role)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(x =>
                    x.FullName.ToLower().Contains(term) ||
                    (x.Phone != null && x.Phone.Contains(term)) ||
                    (x.Email != null && x.Email.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(roleFilter))
            {
                query = query.Where(x => x.Role.RoleName == roleFilter.ToUpper());
            }

            return await query
                .OrderBy(x => x.Role.RoleName)
                .ThenBy(x => x.FullName)
                .Select(x => new UserListItemDto
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    Phone = x.Phone,
                    Email = x.Email,
                    RoleName = x.Role.RoleName,
                    IsActive = x.IsActive ?? true,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<UserFormDto> GetCreateModelAsync()
        {
            return new UserFormDto
            {
                RoleOptions = await GetRoleOptionsAsync()
            };
        }

        public async Task<UserFormDto?> GetEditModelAsync(Guid id)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
            {
                return null;
            }

            return new UserFormDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Phone = user.Phone ?? string.Empty,
                Email = user.Email,
                RoleId = user.RoleId,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                IsActive = user.IsActive ?? true,
                RoleOptions = await GetRoleOptionsAsync()
            };
        }

        public async Task<(bool Success, string? Error)> CreateAsync(UserFormDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return (false, "Mật khẩu là bắt buộc khi tạo tài khoản mới.");
            }

            var phone = request.Phone.Trim();
            if (await _context.Users.AnyAsync(x => x.Phone == phone))
            {
                return (false, "Số điện thoại đã được sử dụng bởi tài khoản khác.");
            }

            if (!await _context.Roles.AnyAsync(x => x.Id == request.RoleId))
            {
                return (false, "Vai trò không hợp lệ.");
            }

            await _context.Users.AddAsync(new User
            {
                Id = Guid.NewGuid(),
                FullName = request.FullName.Trim(),
                Phone = phone,
                Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                RoleId = request.RoleId,
                Gender = request.Gender,
                DateOfBirth = request.DateOfBirth,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(UserFormDto request)
        {
            if (!request.Id.HasValue)
            {
                return (false, "Thiếu thông tin tài khoản.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.Id.Value);
            if (user == null)
            {
                return (false, "Không tìm thấy tài khoản.");
            }

            var phone = request.Phone.Trim();
            if (await _context.Users.AnyAsync(x => x.Phone == phone && x.Id != user.Id))
            {
                return (false, "Số điện thoại đã được sử dụng bởi tài khoản khác.");
            }

            if (!await _context.Roles.AnyAsync(x => x.Id == request.RoleId))
            {
                return (false, "Vai trò không hợp lệ.");
            }

            user.FullName = request.FullName.Trim();
            user.Phone = phone;
            user.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
            user.RoleId = request.RoleId;
            user.Gender = request.Gender;
            user.DateOfBirth = request.DateOfBirth;
            user.IsActive = request.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }

            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<List<GuardianLinkListItemDto>> GetGuardianLinksAsync()
        {
            return await _context.StudentGuardians
                .AsNoTracking()
                .Include(x => x.Student)
                .Include(x => x.Guardian)
                .Include(x => x.Relationship)
                .OrderBy(x => x.Student.FullName)
                .Select(x => new GuardianLinkListItemDto
                {
                    Id = x.Id,
                    StudentName = x.Student.FullName,
                    GuardianName = x.Guardian.FullName,
                    GuardianPhone = x.Guardian.Phone,
                    RelationshipName = x.Relationship != null ? x.Relationship.RelationshipName : null,
                    IsPrimary = x.IsPrimary ?? false
                })
                .ToListAsync();
        }

        public async Task<GuardianLinkFormDto> GetGuardianLinkModelAsync()
        {
            return new GuardianLinkFormDto
            {
                StudentOptions = await GetUserOptionsByRoleAsync(AppRoles.Student),
                GuardianOptions = await GetUserOptionsByRoleAsync(AppRoles.Parent),
                RelationshipOptions = await _context.FamilyRelationships
                    .AsNoTracking()
                    .OrderBy(x => x.Id)
                    .Select(x => new SelectListItem(x.RelationshipName, x.Id.ToString()))
                    .ToListAsync()
            };
        }

        public async Task<(bool Success, string? Error)> CreateGuardianLinkAsync(GuardianLinkFormDto request)
        {
            var studentValid = await _context.Users.AnyAsync(x => x.Id == request.StudentId && x.Role.RoleName == AppRoles.Student);
            var guardianValid = await _context.Users.AnyAsync(x => x.Id == request.GuardianId && x.Role.RoleName == AppRoles.Parent);

            if (!studentValid || !guardianValid)
            {
                return (false, "Học sinh hoặc phụ huynh không hợp lệ.");
            }

            if (await _context.StudentGuardians.AnyAsync(x => x.StudentId == request.StudentId && x.GuardianId == request.GuardianId))
            {
                return (false, "Liên kết phụ huynh – học sinh này đã tồn tại.");
            }

            await _context.StudentGuardians.AddAsync(new StudentGuardian
            {
                Id = Guid.NewGuid(),
                StudentId = request.StudentId,
                GuardianId = request.GuardianId,
                RelationshipId = request.RelationshipId,
                IsPrimary = request.IsPrimary,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<bool> RemoveGuardianLinkAsync(Guid linkId)
        {
            var link = await _context.StudentGuardians.FirstOrDefaultAsync(x => x.Id == linkId);
            if (link == null)
            {
                return false;
            }

            _context.StudentGuardians.Remove(link);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Tạo tài khoản Admin mặc định khi hệ thống chưa có user nào,
        /// để tránh tình trạng không thể đăng nhập với database trống.
        /// </summary>
        public async Task EnsureAdminAccountAsync()
        {
            if (await _context.Users.AnyAsync())
            {
                return;
            }

            var adminRole = await _context.Roles.FirstOrDefaultAsync(x => x.RoleName == AppRoles.Admin);
            if (adminRole == null)
            {
                adminRole = new Role { RoleName = AppRoles.Admin, Description = "Administrator" };
                await _context.Roles.AddAsync(adminRole);
                await _context.SaveChangesAsync();
            }

            await _context.Users.AddAsync(new User
            {
                Id = Guid.NewGuid(),
                FullName = "System Administrator",
                Phone = DefaultAdminPhone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(DefaultAdminPassword),
                RoleId = adminRole.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        private async Task<List<SelectListItem>> GetRoleOptionsAsync()
        {
            return await _context.Roles
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Select(x => new SelectListItem(x.RoleName, x.Id.ToString()))
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetUserOptionsByRoleAsync(string roleName)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(x => x.Role.RoleName == roleName && (x.IsActive ?? true))
                .OrderBy(x => x.FullName)
                .Select(x => new SelectListItem($"{x.FullName} ({x.Phone})", x.Id.ToString()))
                .ToListAsync();
        }
    }
}
