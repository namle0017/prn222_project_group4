using FapWeb.Models.Dtos.LoginDtos;

namespace FapWeb.Services.IServices
{
    public interface IAuthService
    {
        LoginResponseDto LoginAsync(LoginRequestDto request);
    }
}
