namespace FapWeb.Models.Dtos.ChatbotDtos
{
    public class ChatbotResponseDto
    {
        public string Answer { get; init; } = string.Empty;
        public string? SuggestedActionLabel { get; init; }
        public string? SuggestedActionUrl { get; init; }
        public bool IsAvailable { get; init; } = true;
    }
}
