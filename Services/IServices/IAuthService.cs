using FapWeb.Models.Dtos.ChangePasswordDtos;
using FapWeb.Models.Dtos.LoginDtos;

namespace FapWeb.Services.IServices
{
    public interface IAuthService
    {
        LoginResponseDto? LoginAsync(LoginRequestDto request);
        Task<bool> ChangePassword(ChangePasswordRequestDto request);
    }
}
