using System;
using System.Collections.Generic;

namespace Persistence.Entities;

public partial class PaymentIdempotency
{
    public string MerchantId { get; set; } = null!;

    public string IdempotencyKey { get; set; } = null!;

    public Guid TransactionId { get; set; }

    public string RequestHash { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public virtual Merchant Merchant { get; set; } = null!;

    public virtual Transaction Transaction { get; set; } = null!;
}
