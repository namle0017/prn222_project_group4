namespace FapWeb.Models.Dtos.AttendanceDtos
{
    public class AttendanceHistoryDto
    {
        public Guid StudentId { get; set; }

        public string StudentName { get; set; } = string.Empty;

        public string ClassName { get; set; } = string.Empty;

        public DateTime AttendanceDate { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public string? TeacherName { get; set; }
    }
}
