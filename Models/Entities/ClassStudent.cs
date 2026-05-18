using System;
using System.Collections.Generic;

namespace FapWeb.Models.Entities;

public partial class ClassStudent
{
    public Guid Id { get; set; }

    public Guid ClassId { get; set; }

    public Guid StudentId { get; set; }

    public DateTime? EnrolledAt { get; set; }

    public string? Status { get; set; }

    public bool? IsEnable { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual User Student { get; set; } = null!;
}
