namespace FapWeb.Models.Dtos.AttendanceDtos
{
    public class AttendanceClassDto
    {
        public Guid ClassId { get; set; }

        public string ClassName { get; set; } = string.Empty;

        public string? RoomName { get; set; }

        public int StudentCount { get; set; }

        public string? TeacherName { get; set; }
    }
}
