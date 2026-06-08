namespace FapWeb.Models.Dtos.ClassManagementDtos
{
    public class ClassStudentsViewDto
    {
        public Guid ClassId { get; set; }

        public string ClassName { get; set; } = string.Empty;

        public List<ClassStudentItemDto> Students { get; set; } = new();
    }
}
