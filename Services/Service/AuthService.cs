using FapWeb.Models.Dtos.LoginDtos;
using FapWeb.Services.IServices;
using BCrypt.Net;
using FapWeb.Models.Data;
using Microsoft.EntityFrameworkCore;
using FapWeb.Models.Dtos.ChangePasswordDtos;

namespace FapWeb.Services.Service
{
    public class AuthService : IAuthService
    {
        private readonly PostgresContext _context;
        public AuthService(PostgresContext context)
        {
            _context = context;
        }
        public LoginResponseDto? LoginAsync(LoginRequestDto request)
        {
            // IsActive = null la du lieu cu chua set co, van coi la dang hoat dong.
            // Tai khoan bi admin tat (IsActive = false) thi khong duoc dang nhap.
            var user = _context.Users
                                                .Include(u => u.Role)
                                                .Where(u => u.Phone == request.UserPhoneNumber && u.IsActive != false)
                                                .Select(u => new
                                                {
                                                    u.Id,
                                                    u.FullName,
                                                    u.Role.RoleName,
                                                    u.PasswordHash
                                                })
                                                .FirstOrDefault();
            if (user == null)
            {
                return null;
            }

            bool isValidPassword = VerifyPassword(request.UserPassword, user.PasswordHash);
            if (!isValidPassword)
            {
                return null;
            }

            return new LoginResponseDto
            {
                UserId = user.Id,
                UserName = user.FullName,
                UserRole = user.RoleName
            };
        }

        public async Task<bool> ChangePassword(ChangePasswordRequestDto request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
            {
                return false;
            }

            bool isValidPassword = VerifyPassword(request.OldPassword, user.PasswordHash);
            if (!isValidPassword)
            {
                return false;
            }

            user.PasswordHash = HashPassword(request.NewPassword);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// BCrypt.Verify nem SaltParseException neu PasswordHash trong database khong
        /// dung dinh dang BCrypt (du lieu cu hoac hash duoc chen tay). Truoc day loi nay
        /// khong duoc bat nen nguoi dung gap trang loi 500 thay vi thong bao sai mat khau.
        /// </summary>
        private bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(hashedPassword))
            {
                return false;
            }

            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch (SaltParseException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}