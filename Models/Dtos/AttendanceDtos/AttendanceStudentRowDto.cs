namespace FapWeb.Models.Dtos.AttendanceDtos
{
    public class AttendanceStudentRowDto
    {
        public Guid StudentId { get; set; }

        public string StudentName { get; set; } = string.Empty;

        public string StatusName { get; set; } = "PRESENT";
    }
}
