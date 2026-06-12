using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FapWeb.Models.Dtos.TuitionDtos
{
    public class TuitionFeeCreateDto
    {
        [Required(ErrorMessage = "Lớp học là bắt buộc.")]
        [Display(Name = "Lớp học")]
        public Guid? ClassId { get; set; }

        [Required(ErrorMessage = "Số tiền là bắt buộc.")]
        [Range(1000, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 1.000 VND.")]
        [Display(Name = "Học phí mỗi học sinh (VND)")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Tháng học phí là bắt buộc.")]
        [RegularExpression(@"^\d{4}-\d{2}$", ErrorMessage = "Tháng học phí không hợp lệ.")]
        [Display(Name = "Tháng học phí")]
        public string BillingMonth { get; set; } = DateTime.Today.ToString("yyyy-MM");

        public List<SelectListItem> ClassOptions { get; set; } = new();
    }
}
