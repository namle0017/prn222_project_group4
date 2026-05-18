using System;
using System.Collections.Generic;

namespace FapWeb.Models.Entities;

public partial class Schedule
{
    public Guid Id { get; set; }

    public Guid ClassId { get; set; }

    public DateOnly ScheduleDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string? Topic { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AttendanceCheck> AttendanceChecks { get; set; } = new List<AttendanceCheck>();

    public virtual Class Class { get; set; } = null!;
}
