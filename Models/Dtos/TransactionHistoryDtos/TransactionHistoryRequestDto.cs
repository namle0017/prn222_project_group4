using FapWeb.Models.Dtos.PaginatedDtos;
using FapWeb.Models.Dtos.TransactionHistoryDtos.Childs;

namespace FapWeb.Models.Dtos.TransactionHistoryDtos
{
    public class TransactionHistoryRequestDto
    {
        public UserQueryRequestDto QueryUser { get; set; }
        public PaginatedDto Paginated { get; set; }
    }
}
