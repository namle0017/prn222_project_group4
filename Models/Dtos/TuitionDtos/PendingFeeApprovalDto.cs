namespace FapWeb.Models.Dtos.TuitionDtos
{
    public class PendingFeeApprovalDto
    {
        public Guid TuitionFeeId { get; set; }

        public string StudentName { get; set; } = string.Empty;

        public string? ClassName { get; set; }

        public string? Description { get; set; }

        public decimal Amount { get; set; }

        public DateOnly? DueDate { get; set; }

        public string? CreatedByName { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
