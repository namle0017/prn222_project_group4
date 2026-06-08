namespace FapWeb.Models.Dtos.AttendanceDtos
{
    public class AttendanceSaveStudentDto
    {
        public Guid StudentId { get; set; }

        public string StatusName { get; set; } = "PRESENT";
    }
}
