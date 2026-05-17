using System;
using System.Collections.Generic;

namespace FapWeb.Models.Entities;

public partial class TuitionFee
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    public Guid? ClassId { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal? PaidAmount { get; set; }

    public DateOnly? DueDate { get; set; }

    public int? StatusId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Class? Class { get; set; }

    public virtual TuitionFeeStatus? Status { get; set; }

    public virtual User Student { get; set; } = null!;

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
