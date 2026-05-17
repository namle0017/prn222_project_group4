using System;
using System.Collections.Generic;

namespace FapWeb.Models.Entities;

public partial class TuitionFeeStatus
{
    public int Id { get; set; }

    public string StatusName { get; set; } = null!;

    public virtual ICollection<TuitionFee> TuitionFees { get; set; } = new List<TuitionFee>();
}
