using System;
using System.Collections.Generic;

namespace Persistence.Entities;

public partial class TransactionEvent
{
    public long EventId { get; set; }

    public Guid TransactionId { get; set; }

    public string EventType { get; set; } = null!;

    public string? PreviousStatus { get; set; }

    public string? NewStatus { get; set; }

    public int? DurationMs { get; set; }

    public int? AttemptNumber { get; set; }

    public string? Payload { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Transaction Transaction { get; set; } = null!;
}
