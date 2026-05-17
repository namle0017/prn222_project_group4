using System;
using System.Collections.Generic;

namespace FapWeb.Models.Entities;

public partial class TransactionStatus
{
    public int Id { get; set; }

    public string StatusName { get; set; } = null!;

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
