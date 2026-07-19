namespace FapWeb.Services.IServices
{
    public interface IChatRequestLimiter
    {
        bool TryStart(string sessionId, out string? errorMessage);
        void Complete(string sessionId);
    }
}
