using FapWeb.Models.Dtos.SharedDtos;
using System.ComponentModel.DataAnnotations;

namespace FapWeb.Models.Dtos.ClassManagementDtos
{
    public class ClassFormDto
    {
        public Guid? Id { get; set; }

        [Required]
        [StringLength(100)]
        public string ClassName { get; set; } = string.Empty;

        public Guid? TeacherId { get; set; }

        [StringLength(100)]
        public string? RoomName { get; set; }

        [Range(1, 200)]
        public int? MaxStudents { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? EndDate { get; set; }

        [Range(0, 500)]
        public int TotalSessions { get; set; }

        public List<SelectOptionDto> TeacherOptions { get; set; } = new();
    }
}
