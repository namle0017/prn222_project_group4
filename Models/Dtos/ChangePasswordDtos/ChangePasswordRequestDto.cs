namespace FapWeb.Models.Dtos.ChangePasswordDtos
{
    public class ChangePasswordRequestDto
    {
        public Guid UserId { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
