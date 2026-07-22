namespace FapWeb.Models.Dtos.TuitionDtos
{
    public class TuitionStudentStatusDto
    {
        public Guid TuitionFeeId { get; set; }

        public Guid StudentId { get; set; }

        public string StudentName { get; set; } = string.Empty;

        public string? ClassName { get; set; }

        public string? Description { get; set; }

        public string FeeType { get; set; } = "TUITION";

        public decimal RequiredFee { get; set; }

        public decimal PaidAmount { get; set; }

        public decimal RemainingAmount { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public DateOnly? DueDate { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
