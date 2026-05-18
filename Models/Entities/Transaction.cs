using System;
using System.Collections.Generic;

namespace FapWeb.Models.Entities;

public partial class Transaction
{
    public Guid Id { get; set; }

    public Guid TuitionFeeId { get; set; }

    public decimal Amount { get; set; }

    public string? PaymentMethod { get; set; }

    public string? TransactionCode { get; set; }

    public int? StatusId { get; set; }

    public Guid? PaidBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User? PaidByNavigation { get; set; }

    public virtual TransactionStatus? Status { get; set; }

    public virtual TuitionFee TuitionFee { get; set; } = null!;
}
