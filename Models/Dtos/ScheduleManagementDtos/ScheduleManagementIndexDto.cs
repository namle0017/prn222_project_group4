using FapWeb.Models.Dtos.SharedDtos;

namespace FapWeb.Models.Dtos.ScheduleManagementDtos
{
    public class ScheduleManagementIndexDto
    {
        public Guid? ClassIdFilter { get; set; }

        public DateOnly? DateFilter { get; set; }

        public int TotalSchedules { get; set; }

        public int TodaySchedules { get; set; }

        public int UpcomingSchedules { get; set; }

        public int ClassesCovered { get; set; }

        public List<SelectOptionDto> ClassOptions { get; set; } = new();

        public List<ScheduleListItemDto> Schedules { get; set; } = new();
    }
}
