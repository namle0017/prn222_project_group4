namespace FapWeb.Models.Dtos.TuitionDtos
{
    public class PaymentHistoryDto
    {
        public Guid TransactionId { get; set; }

        public Guid StudentId { get; set; }

        public string StudentName { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }

        public string RecordedByName { get; set; } = string.Empty;

        public string StatusName { get; set; } = string.Empty;

        public string? Note { get; set; }
    }
}
