namespace FapWeb.Models.Dtos.ClassManagementDtos
{
    public class ClassListItemDto
    {
        public Guid ClassId { get; set; }

        public string ClassName { get; set; } = string.Empty;

        public string? TeacherName { get; set; }

        public Guid? TeacherId { get; set; }

        public string? RoomName { get; set; }

        public int StudentCount { get; set; }

        public int ScheduleCount { get; set; }

        public string StatusName { get; set; } = string.Empty;
    }
}
