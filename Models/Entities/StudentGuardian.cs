using System;
using System.Collections.Generic;

namespace FapWeb.Models.Entities;

public partial class StudentGuardian
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    public Guid GuardianId { get; set; }

    public int? RelationshipId { get; set; }

    public bool? IsPrimary { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User Guardian { get; set; } = null!;

    public virtual FamilyRelationship? Relationship { get; set; }

    public virtual User Student { get; set; } = null!;
}
