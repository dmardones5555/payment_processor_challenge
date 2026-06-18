using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Payments.Reconciliation
{
    public sealed class ReconciliationPayload
    {
        public string Reason { get; init; } = default!;
        public Guid TransactionId { get; init; }
        public string MerchantId { get; init; } = default!;
        public decimal Amount { get; init; }
        public string Currency { get; init; } = default!;
        public string CurrentStatus { get; init; } = default!;
        public int RetryCount { get; init; }
        public string? AcquirerReference { get; init; }
        public string AcquirerStatus { get; init; } = default!;
        public string? ResponseMessage { get; init; }
        public bool IsTransientFailure { get; init; }
        public DateTime EnqueuedAtUtc { get; init; }
    }
}
