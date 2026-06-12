namespace FapWeb.Models.Dtos.SePayDtos
{
    public class SePayCheckoutOrderDto
    {
        public decimal Amount { get; set; }

        public string Description { get; set; } = string.Empty;

        public string InvoiceNumber { get; set; } = string.Empty;

        public string? CustomerId { get; set; }

        public string? PaymentMethod { get; set; }

        public string? SuccessUrl { get; set; }

        public string? ErrorUrl { get; set; }

        public string? CancelUrl { get; set; }
    }
}
