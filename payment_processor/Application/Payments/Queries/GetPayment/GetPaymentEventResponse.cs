using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Payments.Queries.GetPayment
{
    public sealed class GetPaymentEventResponse
    {
        public long EventId { get; init; }
        public Guid TransactionId { get; init; }
        public string EventType { get; init; } = default!;
        public int? DurationMs { get; init; }
        public int? AttemptNumber { get; init; }
        public string? Payload { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}
