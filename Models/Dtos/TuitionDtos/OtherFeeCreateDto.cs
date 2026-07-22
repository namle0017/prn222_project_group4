using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FapWeb.Models.Dtos.TuitionDtos
{
    // "Phí khác" (không phải học phí tháng). Teacher tạo -> chờ admin duyệt; admin tạo -> duyệt luôn.
    public class OtherFeeCreateDto
    {
        [Required(ErrorMessage = "Lớp học là bắt buộc.")]
        [Display(Name = "Lớp học")]
        public Guid? ClassId { get; set; }

        [Required(ErrorMessage = "Nội dung khoản phí là bắt buộc.")]
        [StringLength(255)]
        [Display(Name = "Nội dung khoản phí")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số tiền là bắt buộc.")]
        [Range(1000, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 1.000 VND.")]
        [Display(Name = "Số tiền mỗi học sinh (VND)")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Hạn đóng là bắt buộc.")]
        [DataType(DataType.Date)]
        [Display(Name = "Hạn đóng")]
        public DateOnly DueDate { get; set; } = DateOnly.FromDateTime(DateTime.Today).AddDays(30);

        public List<SelectListItem> ClassOptions { get; set; } = new();
    }
}
