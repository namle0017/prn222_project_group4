using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FapWeb.Models.Dtos.UserManagementDtos
{
    public class UserFormDto
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc.")]
        [StringLength(150)]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        // Bắt buộc khi tạo mới; khi sửa, để trống nghĩa là giữ mật khẩu cũ.
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Vai trò là bắt buộc.")]
        [Display(Name = "Vai trò")]
        public int RoleId { get; set; }

        [Display(Name = "Giới tính")]
        public string? Gender { get; set; }

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateOnly? DateOfBirth { get; set; }

        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; } = true;

        public List<SelectListItem> RoleOptions { get; set; } = new();
    }
}
