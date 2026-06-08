namespace FapWeb.Models.Dtos.DashboardDtos
{
    public class StudentDashboardDto
    {
        public int MyClasses { get; set; }

        public int AbsentCount { get; set; }

        public decimal TuitionRemaining { get; set; }

        public double AttendanceRate { get; set; }

        public List<DashboardScheduleItemDto> UpcomingSchedules { get; set; } = new();

        public List<DashboardNotificationItemDto> LatestNotifications { get; set; } = new();
    }
}
