namespace FapWeb.Models.Dtos.DashboardDtos
{
    public class DashboardChildItemDto
    {
        public string ChildName { get; set; } = string.Empty;

        public string ClassName { get; set; } = string.Empty;

        public double AttendanceRate { get; set; }

        public decimal RemainingTuition { get; set; }
    }
}
