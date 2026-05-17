using System;
using System.Collections.Generic;

namespace FapWeb.Models.Entities;

public partial class AttendanceCheck
{
    public Guid Id { get; set; }

    public Guid ScheduleId { get; set; }

    public Guid StudentId { get; set; }

    public int StatusId { get; set; }

    public Guid? CheckedBy { get; set; }

    public string? Note { get; set; }

    public DateTime? CheckedAt { get; set; }

    public virtual User? CheckedByNavigation { get; set; }

    public virtual Schedule Schedule { get; set; } = null!;

    public virtual AttendanceCheckStatus Status { get; set; } = null!;

    public virtual User Student { get; set; } = null!;
}
