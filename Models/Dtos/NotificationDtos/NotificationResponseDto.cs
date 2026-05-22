namespace FapWeb.Models.Dtos.NotificationDtos
{
    public class NotificationResponseDto
    {
        public Guid Id { get; set; }

        public Guid? SenderId { get; set; }

        public string? SenderName { get; set; }

        public Guid? ReceiverId { get; set; }

        public string? ReceiverName { get; set; }

        public string Title { get; set; } = null!;

        public string Content { get; set; } = null!;

        public bool IsRead { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
