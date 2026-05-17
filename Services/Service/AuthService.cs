using FapWeb.Models.Dtos.LoginDtos;
using FapWeb.Services.IServices;
using BCrypt.Net;
using FapWeb.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace FapWeb.Services.Service
{
    public class AuthService : IAuthService
    {
        public PostgresContext _context;
        public AuthService(PostgresContext context)
        {
            _context = context;
        }
        public LoginResponseDto LoginAsync(LoginRequestDto request)
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
