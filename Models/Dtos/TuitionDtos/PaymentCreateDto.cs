using System.ComponentModel.DataAnnotations;

namespace FapWeb.Models.Dtos.TuitionDtos
{
    public class PaymentCreateDto
    {
        public Guid TuitionFeeId { get; set; }

        public Guid StudentId { get; set; }

        public string StudentName { get; set; } = string.Empty;

        public decimal RemainingAmount { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; } = DateTime.Today;

        public string? Note { get; set; }
    }
}
