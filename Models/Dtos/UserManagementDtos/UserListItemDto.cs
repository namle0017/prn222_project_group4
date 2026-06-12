namespace FapWeb.Models.Dtos.UserManagementDtos
{
    public class UserListItemDto
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public string RoleName { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
