using FapWeb.Models.Dtos.SharedDtos;
using System.ComponentModel.DataAnnotations;

namespace FapWeb.Models.Dtos.ScheduleManagementDtos
{
    public class ScheduleFormDto
    {
        public Guid? Id { get; set; }

        [Required]
        public Guid ClassId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateOnly ScheduleDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Required]
        public TimeOnly StartTime { get; set; } = new(8, 0);

        [Required]
        public TimeOnly EndTime { get; set; } = new(10, 0);

        [StringLength(255)]
        public string? Topic { get; set; }

        public List<SelectOptionDto> ClassOptions { get; set; } = new();

        // Số lần còn được đổi lịch trong tháng này (chỉ áp dụng cho teacher). null = không giới hạn.
        public int? RemainingEditsThisMonth { get; set; }
    }
}
