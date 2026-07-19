using System.ComponentModel.DataAnnotations;

namespace FapWeb.Models.Dtos.ChatbotDtos
{
    public class ChatbotRequestDto
    {
        [Required(ErrorMessage = "Vui lòng nhập câu hỏi.")]
        [StringLength(500, ErrorMessage = "Câu hỏi không được vượt quá 500 ký tự.")]
        public string Message { get; set; } = string.Empty;
    }
}
