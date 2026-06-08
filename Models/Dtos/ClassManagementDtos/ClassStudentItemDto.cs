namespace FapWeb.Models.Dtos.ClassManagementDtos
{
    public class ClassStudentItemDto
    {
        public Guid StudentId { get; set; }

        public string StudentName { get; set; } = string.Empty;

        public string? GuardianName { get; set; }

        public DateTime? EnrolledAt { get; set; }

        public string? Status { get; set; }

        public bool IsActive { get; set; }
    }
}
