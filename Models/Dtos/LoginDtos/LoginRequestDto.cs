using System.ComponentModel.DataAnnotations;

namespace FapWeb.Models.Dtos.LoginDtos
{
    /// <summary>
    /// Đối tượng DTO chứa yêu cầu thông tin đăng nhập của người dùng vào hệ thống FAP Portal.
    /// </summary>
    /// <remarks>
    /// Được sử dụng bởi AuthController và AuthService để nhận và kiểm tra dữ liệu từ Form Đăng nhập.
    /// </remarks>
    public class LoginRequestDto
    {
        /// <summary>
        /// Số điện thoại dùng làm tài khoản đăng nhập của người dùng.
        /// </summary>
        /// <example>0901234567</example>
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [Display(Name = "Số điện thoại")]
        public string UserPhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// Mật khẩu dạng văn bản thô do người dùng nhập vào.
        /// </summary>
        /// <example>password123</example>
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string UserPassword { get; set; } = string.Empty;
    }
}
