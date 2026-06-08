namespace FapWeb.Models.Dtos.AttendanceDtos
{
    public class AttendanceTakeViewModel
    {
        public Guid ClassId { get; set; }

        public string ClassName { get; set; } = string.Empty;

        public DateTime AttendanceDate { get; set; }

        public Guid? ScheduleId { get; set; }

        public string? ScheduleLabel { get; set; }

        public string? ErrorMessage { get; set; }

        public List<FapWeb.Models.Dtos.SharedDtos.SelectOptionDto> ScheduleOptions { get; set; } = new();

        public List<AttendanceStudentRowDto> Students { get; set; } = new();
    }
}
