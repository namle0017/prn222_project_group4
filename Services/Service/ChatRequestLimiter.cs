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

            RemoveExpiredSessions();
        }

        /// <summary>
        /// ChatRequestLimiter la Singleton nen _sessions se phinh mai neu khong don.
        /// Moi session sau khi het cua so gioi han va khong con request nao dang chay
        /// thi khong con gia tri, co the bo di.
        /// </summary>
        private void RemoveExpiredSessions()
        {
            var windowMinutes = Math.Max(1, _settings.RateLimitWindowMinutes);

            // Chi don session da im lang gap doi cua so gioi han. De han rong nhu vay
            // de mot session vua duoc GetOrAdd o luong khac khong bi xoa nham,
            // vi bi xoa nham dong nghia voi viec session do thoat khoi gioi han.
            var staleBefore = DateTime.UtcNow.AddMinutes(-windowMinutes * 2);

            foreach (var entry in _sessions)
            {
                var usage = entry.Value;
                bool isStale;

                lock (usage.SyncRoot)
                {
                    // Khong dung dieu kien Requests rong: mot session vua duoc tao o
                    // luong khac cung dang rong trong khoanh khac truoc khi ghi request.
                    isStale = !usage.IsInFlight
                              && usage.Requests.Count > 0
                              && usage.Requests.Max() < staleBefore;
                }

                if (isStale)
                {
                    _sessions.TryRemove(entry.Key, out _);
                }
            }
        }
    }
}
