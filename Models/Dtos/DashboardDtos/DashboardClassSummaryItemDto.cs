namespace FapWeb.Models.Dtos.DashboardDtos
{
    public class DashboardClassSummaryItemDto
    {
        public string ClassName { get; set; } = string.Empty;

        public int StudentCount { get; set; }

        public DateOnly? NextScheduleDate { get; set; }
    }
}
