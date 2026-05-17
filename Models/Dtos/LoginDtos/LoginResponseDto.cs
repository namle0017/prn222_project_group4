namespace FapWeb.Models.Dtos.LoginDtos
{
    public class LoginResponseDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string UserRole { get; set; }
    }
}
