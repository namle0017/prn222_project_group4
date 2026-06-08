namespace FapWeb.Models.Dtos.ClassManagementDtos
{
    public class ClassScheduleItemDto
    {
        public Guid ScheduleId { get; set; }

        public DateOnly ScheduleDate { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public string? Topic { get; set; }

        public int AttendanceCount { get; set; }
    }
}
