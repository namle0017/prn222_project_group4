using FapWeb.Models.Dtos.UserManagementDtos;

namespace FapWeb.Services.IServices
{
    public interface IUserManagementService
    {
        Task<List<UserListItemDto>> GetUsersAsync(string? searchTerm, string? roleFilter);

        Task<UserFormDto> GetCreateModelAsync();

        Task<UserFormDto?> GetEditModelAsync(Guid id);

        Task<(bool Success, string? Error)> CreateAsync(UserFormDto request);

        Task<(bool Success, string? Error)> UpdateAsync(UserFormDto request);

        Task<List<GuardianLinkListItemDto>> GetGuardianLinksAsync();

        Task<GuardianLinkFormDto> GetGuardianLinkModelAsync();

        Task<(bool Success, string? Error)> CreateGuardianLinkAsync(GuardianLinkFormDto request);

        Task<bool> RemoveGuardianLinkAsync(Guid linkId);

        //Task EnsureAdminAccountAsync();
    }
}
