using System.Collections.Concurrent;
using FapWeb.Models.Configurations;
using FapWeb.Services.IServices;
using Microsoft.Extensions.Options;

namespace FapWeb.Services.Service
{
    public class ChatRequestLimiter : IChatRequestLimiter
    {
        private sealed class SessionUsage
        {
            public object SyncRoot { get; } = new();
            public bool IsInFlight { get; set; }
            public List<DateTime> Requests { get; } = new();
        }

        private readonly ConcurrentDictionary<string, SessionUsage> _sessions = new();
        private readonly MiMoSettings _settings;

        public ChatRequestLimiter(IOptions<MiMoSettings> settings)
        {
            _settings = settings.Value;
        }

        public bool TryStart(string sessionId, out string? errorMessage)
        {
            var usage = _sessions.GetOrAdd(sessionId, _ => new SessionUsage());
            var now = DateTime.UtcNow;
            var windowStart = now.AddMinutes(-Math.Max(1, _settings.RateLimitWindowMinutes));

            lock (usage.SyncRoot)
            {
                usage.Requests.RemoveAll(requestedAt => requestedAt < windowStart);

                if (usage.IsInFlight)
                {
                    errorMessage = "Chatbot đang xử lý câu hỏi trước đó. Vui lòng chờ một chút.";
                    return false;
                }

                if (usage.Requests.Count >= Math.Max(1, _settings.MaxRequestsPerWindow))
                {
                    errorMessage = "Bạn đã gửi quá nhiều câu hỏi. Vui lòng thử lại sau vài phút.";
                    return false;
                }

                usage.IsInFlight = true;
                usage.Requests.Add(now);
                errorMessage = null;
                return true;
            }
        }

        public void Complete(string sessionId)
        {
            if (!_sessions.TryGetValue(sessionId, out var usage))
            {
                return;
            }

            lock (usage.SyncRoot)
            {
                usage.IsInFlight = false;
            }
        }
    }
}
