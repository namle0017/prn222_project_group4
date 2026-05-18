namespace FapWeb.Models.Dtos.TransactionHistoryDtos
{
    public class TransactionHistoryRequestDto
    {
        public Guid UserId { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
