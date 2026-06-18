using System;
using System.Collections.Generic;

namespace Persistence.Entities;

public partial class ReconciliationQueue
{
    public long Id { get; set; }

    public Guid TransactionId { get; set; }

    public string? AcquirerReference { get; set; }

    public string? ResponsePayload { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public virtual Transaction Transaction { get; set; } = null!;
}
