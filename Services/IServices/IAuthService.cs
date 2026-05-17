using FapWeb.Models.Dtos.LoginDtos;

namespace FapWeb.Services.IServices
{
    public interface IAuthService
    {
        string LoginAsync(LoginRequestDto request);
    }
}
