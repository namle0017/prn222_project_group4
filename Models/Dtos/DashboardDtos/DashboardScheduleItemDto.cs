namespace FapWeb.Models.Dtos.DashboardDtos
{
    public class DashboardScheduleItemDto
    {
        public string ClassName { get; set; } = string.Empty;

        public DateOnly ScheduleDate { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public string? Topic { get; set; }
    }
}
