using FapWeb.Models.Dtos.LoginDtos;
using FapWeb.Services.IServices;
using BCrypt.Net;

namespace FapWeb.Services.Service
{
    public class AuthService : IAuthService
    {
        public string LoginAsync(LoginRequestDto request)
        {

            return "Login successful";
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
