using System;
using System.Collections.Generic;

namespace Persistence.Entities;

public partial class Transaction
{
    public Guid TransactionId { get; set; }

    public string MerchantId { get; set; } = null!;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string IdempotencyKey { get; set; } = null!;

    public string? CardBrand { get; set; }

    public string? CardLast4 { get; set; }

    public string? AcquirerReference { get; set; }

    public int RetryCount { get; set; }

    public string? FailureReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Merchant Merchant { get; set; } = null!;

    public virtual ICollection<PaymentIdempotency> PaymentIdempotencies { get; set; } = new List<PaymentIdempotency>();

    public virtual ICollection<ReconciliationQueue> ReconciliationQueues { get; set; } = new List<ReconciliationQueue>();

    public virtual ICollection<TransactionEvent> TransactionEvents { get; set; } = new List<TransactionEvent>();
}
