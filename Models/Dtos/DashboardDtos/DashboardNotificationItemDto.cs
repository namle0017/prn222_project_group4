namespace FapWeb.Models.Dtos.DashboardDtos
{
    public class DashboardNotificationItemDto
    {
        public string Title { get; set; } = string.Empty;

        public DateTime? CreatedAt { get; set; }

        public bool IsRead { get; set; }
    }
}
