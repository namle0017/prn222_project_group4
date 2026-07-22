using FapWeb.Infrastructure;
using FapWeb.Models.Dtos.ChatbotDtos;
using FapWeb.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace FapWeb.Controllers
{
    public class ChatController : Controller
    {
        private readonly IChatbotService _chatbotService;
        private readonly IChatRequestLimiter _requestLimiter;

        public ChatController(IChatbotService chatbotService, IChatRequestLimiter requestLimiter)
        {
            _chatbotService = chatbotService;
            _requestLimiter = requestLimiter;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ask([FromForm] ChatbotRequestDto request, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var roleName = HttpContext.Session.GetString(AppSessionKeys.RoleName);
            if (!userId.HasValue || string.IsNullOrWhiteSpace(roleName))
            {
                return Unauthorized(new { answer = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại." });
            }

            if (!ModelState.IsValid)
            {
                var error = ModelState.Values.SelectMany(value => value.Errors).FirstOrDefault()?.ErrorMessage
                            ?? "Câu hỏi không hợp lệ.";
                return BadRequest(new { answer = error });
            }

            var sessionId = HttpContext.Session.Id;
            if (!_requestLimiter.TryStart(sessionId, out var rateLimitError))
            {
                return StatusCode(StatusCodes.Status429TooManyRequests, new { answer = rateLimitError });
            }

            try
            {
                var response = await _chatbotService.AskAsync(request.Message, userId.Value, roleName, cancellationToken);
                return Json(response);
            }
            finally
            {
                _requestLimiter.Complete(sessionId);
            }
        }

        private Guid? GetCurrentUserId()
        {
            var userIdText = HttpContext.Session.GetString(AppSessionKeys.UserId);
            return Guid.TryParse(userIdText, out var userId) ? userId : null;
        }
    }
}
