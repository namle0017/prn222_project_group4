namespace FapWeb.Models.Dtos.LoginDtos
{
    public class LoginResponseDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
    }
}
