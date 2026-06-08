using FapWeb.Models.Dtos.SharedDtos;
using System.ComponentModel.DataAnnotations;

namespace FapWeb.Models.Dtos.ClassManagementDtos
{
    public class AddStudentToClassDto
    {
        public Guid ClassId { get; set; }

        public string ClassName { get; set; } = string.Empty;

        [Required]
        public Guid StudentId { get; set; }

        public List<SelectOptionDto> StudentOptions { get; set; } = new();
    }
}
