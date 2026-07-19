using FapWeb.Models.Dtos.ChatbotDtos;

namespace FapWeb.Services.IServices
{
    public interface IChatbotService
    {
        Task<ChatbotResponseDto> AskAsync(string message, Guid currentUserId, string roleName, CancellationToken cancellationToken = default);
    }
}
