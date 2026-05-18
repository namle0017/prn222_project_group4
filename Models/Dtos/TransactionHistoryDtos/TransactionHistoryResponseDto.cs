namespace FapWeb.Models.Dtos.TransactionHistoryDtos
{
    public class TransactionHistoryResponseDto
    {
        public Guid Id { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public string TransactionStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
