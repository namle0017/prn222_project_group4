using FapWeb.Models.Dtos.ChangePasswordDtos;
using FapWeb.Models.Dtos.LoginDtos;

namespace FapWeb.Services.IServices
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
        Task<bool> ChangePasswordAsync(ChangePasswordRequestDto request);
    }
}
