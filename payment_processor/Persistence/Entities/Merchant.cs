using System;
using System.Collections.Generic;

namespace Persistence.Entities;

public partial class Merchant
{
    public string MerchantId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Status { get; set; } = null!;

    public decimal MaxAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<PaymentIdempotency> PaymentIdempotencies { get; set; } = new List<PaymentIdempotency>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
