namespace FapWeb.Models.Dtos.DashboardDtos
{
    public class AdminDashboardDto
    {
        public int TotalStudents { get; set; }

        public int TotalTeachers { get; set; }

        public int TotalParents { get; set; }

        public int TotalClasses { get; set; }

        public int TotalActiveSchedules { get; set; }

        public decimal TotalExpectedTuition { get; set; }

        public decimal TotalCollectedTuition { get; set; }

        public decimal TotalOutstandingTuition { get; set; }

        public int StudentsWithUnpaidTuition { get; set; }

        public int PendingFeeApprovals { get; set; }

        public double OverallAttendanceRate { get; set; }

        public List<DashboardNotificationItemDto> LatestNotifications { get; set; } = new();
    }
}
