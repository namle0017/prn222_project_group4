namespace FapWeb.Models.Dtos.UserManagementDtos
{
    public class GuardianLinkListItemDto
    {
        public Guid Id { get; set; }

        public string StudentName { get; set; } = string.Empty;

        public string GuardianName { get; set; } = string.Empty;

        public string? GuardianPhone { get; set; }

        public string? RelationshipName { get; set; }

        public bool IsPrimary { get; set; }
    }
}
