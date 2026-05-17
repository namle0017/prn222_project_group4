using System;
using System.Collections.Generic;

namespace FapWeb.Models.Entities;

public partial class FamilyRelationship
{
    public int Id { get; set; }

    public string RelationshipName { get; set; } = null!;

    public virtual ICollection<StudentGuardian> StudentGuardians { get; set; } = new List<StudentGuardian>();
}
