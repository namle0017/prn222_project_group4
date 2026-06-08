namespace FapWeb.Models.Dtos.ScheduleManagementDtos
{
    public class ScheduleListItemDto
    {
        public Guid ScheduleId { get; set; }

        public Guid ClassId { get; set; }

        public string ClassName { get; set; } = string.Empty;

        public string? TeacherName { get; set; }

        public DateOnly ScheduleDate { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public string? Topic { get; set; }
    }
}
