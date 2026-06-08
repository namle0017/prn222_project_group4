namespace FapWeb.Models.Dtos.DashboardDtos
{
    public class ParentDashboardDto
    {
        public int MyChildren { get; set; }

        public int TotalAbsentCount { get; set; }

        public decimal TotalTuitionPaid { get; set; }

        public decimal TotalTuitionRemaining { get; set; }

        public double OverallAttendanceRate { get; set; }

        public List<DashboardChildItemDto> Children { get; set; } = new();

        public List<DashboardNotificationItemDto> LatestNotifications { get; set; } = new();
    }
}
