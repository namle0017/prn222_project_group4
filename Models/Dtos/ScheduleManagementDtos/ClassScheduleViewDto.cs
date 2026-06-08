namespace FapWeb.Models.Dtos.ScheduleManagementDtos
{
    public class ClassScheduleViewDto
    {
        public Guid ClassId { get; set; }

        public string ClassName { get; set; } = string.Empty;

        public string? TeacherName { get; set; }

        public List<ScheduleListItemDto> Schedules { get; set; } = new();
    }
}
