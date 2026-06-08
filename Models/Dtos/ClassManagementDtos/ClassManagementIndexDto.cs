using FapWeb.Models.Dtos.SharedDtos;

namespace FapWeb.Models.Dtos.ClassManagementDtos
{
    public class ClassManagementIndexDto
    {
        public string? SearchTerm { get; set; }

        public Guid? TeacherId { get; set; }

        public string? StatusFilter { get; set; }

        public int TotalClasses { get; set; }

        public int ActiveClasses { get; set; }

        public int TotalStudentsEnrolled { get; set; }

        public int ClassesWithoutSchedule { get; set; }

        public List<SelectOptionDto> TeacherOptions { get; set; } = new();

        public List<ClassListItemDto> Classes { get; set; } = new();
    }
}
