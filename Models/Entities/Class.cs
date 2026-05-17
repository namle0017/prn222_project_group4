using System;
using System.Collections.Generic;

namespace FapWeb.Models.Entities;

public partial class Class
{
    public Guid Id { get; set; }

    public string ClassName { get; set; } = null!;

    public Guid? TeacherId { get; set; }

    public string? RoomName { get; set; }

    public int? MaxStudents { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public int TotalSessions { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<ClassStudent> ClassStudents { get; set; } = new List<ClassStudent>();

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public virtual User? Teacher { get; set; }

    public virtual ICollection<TuitionFee> TuitionFees { get; set; } = new List<TuitionFee>();
}
