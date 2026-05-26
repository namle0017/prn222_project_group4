namespace FapWeb.Models.Dtos.TransactionHistoryDtos.Childs
{
    public class UserQueryRequestDto
    {
        public Guid? UserId { get; set; }
        public string? SearchTerm { get; set; }
    }
}
