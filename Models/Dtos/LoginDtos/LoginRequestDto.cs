using System.ComponentModel.DataAnnotations;

namespace FapWeb.Models.Dtos.LoginDtos
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [Display(Name = "Số điện thoại")]
        public string UserPhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string UserPassword { get; set; } = string.Empty;
    }
}
