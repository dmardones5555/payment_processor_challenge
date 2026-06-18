using System;
using System.Collections.Generic;

namespace Persistence.Entities;

public partial class OutboxMessage
{
    public Guid Id { get; set; }

    public Guid AggregateId { get; set; }

    public string EventType { get; set; } = null!;

    public string Payload { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int AttemptCount { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public DateTime? NextAttemptAt { get; set; }
}
