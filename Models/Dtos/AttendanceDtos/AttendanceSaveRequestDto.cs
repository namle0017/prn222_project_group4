namespace FapWeb.Models.Dtos.AttendanceDtos
{
    public class AttendanceSaveRequestDto
    {
        public Guid ClassId { get; set; }

        public DateTime AttendanceDate { get; set; }

        public Guid? ScheduleId { get; set; }

        public List<AttendanceSaveStudentDto> Students { get; set; } = new();
    }
}
