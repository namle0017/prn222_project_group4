namespace FapWeb.Models.Dtos.ClassManagementDtos
{
    public class ClassDetailsDto
    {
        public Guid ClassId { get; set; }

        public string ClassName { get; set; } = string.Empty;

        public string? TeacherName { get; set; }

        public Guid? TeacherId { get; set; }

        public string? RoomName { get; set; }

        public int? MaxStudents { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public int TotalSessions { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public int StudentCount { get; set; }

        public int ScheduleCount { get; set; }

        public int AttendanceRecordsCount { get; set; }

        public decimal TotalExpectedTuition { get; set; }

        public decimal TotalCollectedTuition { get; set; }

        public decimal TotalOutstandingTuition { get; set; }

        public List<ClassStudentItemDto> Students { get; set; } = new();

        public List<ClassScheduleItemDto> Schedules { get; set; } = new();
    }
}
