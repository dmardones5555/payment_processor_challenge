using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public sealed class TransactionEvent
    {
        public long Id { get; private set; }
        public Guid TransactionId { get; private set; }
        public string EventType { get; private set; } = default!;
        public DateTime OccurredAtUtc { get; private set; }
        public int? DurationMs { get; private set; }
        public string? Details { get; private set; }
        public int? AttemptNumber { get; private set; }

        private TransactionEvent()
        {
        }

        private TransactionEvent(
            long id,
            Guid transactionId,
            string eventType,
            DateTime occurredAtUtc,
            int? durationMs,
            string? details,
            int? attemptNumber)
        {
            Id = id;
            TransactionId = transactionId;
            EventType = eventType;
            OccurredAtUtc = occurredAtUtc;
            DurationMs = durationMs;
            Details = details;
            AttemptNumber = attemptNumber;
        }

        public static TransactionEvent Create(
            Guid transactionId,
            string eventType,
            DateTime occurredAtUtc,
            int? durationMs = null,
            string? details = null,
            int? attemptNumber = null)
        {
            return new TransactionEvent(
                0,
                transactionId,
                eventType,
                occurredAtUtc,
                durationMs,
                details,
                attemptNumber);
        }

        public static TransactionEvent Load(
            long id,
            Guid transactionId,
            string eventType,
            DateTime occurredAtUtc,
            int? durationMs,
            string? details,
            int? attemptNumber)
        {
            return new TransactionEvent(
                id,
                transactionId,
                eventType,
                occurredAtUtc,
                durationMs,
                details,
                attemptNumber);
        }
    }
}
