using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Payments.Outbox
{
    public sealed record OutboxMessageData
    {
        public Guid Id { get; init; }
        public Guid AggregateId { get; init; }
        public string EventType { get; init; }
        public string Payload { get; init; }
        public string Status { get; init; }
        public DateTime CreatedAt { get; init; }

        public OutboxMessageData(
            Guid aggregateId,
            string eventType,
            string payload,
            DateTime createdAt)
        {
            Id = Guid.NewGuid();
            AggregateId = aggregateId;
            EventType = eventType;
            Payload = payload;
            Status = "PENDING";
            CreatedAt = createdAt;
        }

        public OutboxMessageData(
            Guid id,
            Guid aggregateId,
            string eventType,
            string status,
            string payload,
            DateTime createdAt)
        {
            Id = id;
            AggregateId = aggregateId;
            EventType = eventType;
            Payload = payload;
            Status = status;
            CreatedAt = createdAt;
        }

        public OutboxMessageData() { }
    }
}
