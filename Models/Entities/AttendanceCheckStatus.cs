using System;
using System.Collections.Generic;

namespace FapWeb.Models.Entities;

public partial class AttendanceCheckStatus
{
    public int Id { get; set; }

    public string StatusName { get; set; } = null!;

    public virtual ICollection<AttendanceCheck> AttendanceChecks { get; set; } = new List<AttendanceCheck>();
}
