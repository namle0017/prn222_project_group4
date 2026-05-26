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
            var user = _context.Users
                                                .Include(u => u.Role)
                                                .Where(u => u.Phone == request.UserPhoneNumber)
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

        private bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}
