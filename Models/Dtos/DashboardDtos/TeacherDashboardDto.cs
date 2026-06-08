namespace FapWeb.Models.Dtos.DashboardDtos
{
    public class TeacherDashboardDto
    {
        public int MyClasses { get; set; }

        public int StudentsInMyClasses { get; set; }

        public int AbsentStudentsToday { get; set; }

        public int ClassesNeedingAttendance { get; set; }

        public decimal OutstandingTuitionInMyClasses { get; set; }

        public List<DashboardScheduleItemDto> TodaySchedules { get; set; } = new();

        public List<DashboardScheduleItemDto> UpcomingSchedules { get; set; } = new();

        public List<DashboardNotificationItemDto> LatestNotifications { get; set; } = new();

        public List<DashboardClassSummaryItemDto> ClassSummaries { get; set; } = new();
    }
}
