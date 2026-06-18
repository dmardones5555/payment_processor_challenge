using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Payments.Queries.GetPayment
{
    public sealed class GetPaymentResponse
    {
        public Guid TransactionId { get; init; }
        public string MerchantId { get; init; } = default!;
        public decimal Amount { get; init; }
        public string Currency { get; init; } = default!;
        public string Status { get; init; } = default!;
        public string IdempotencyKey { get; init; } = default!;
        public string CardBrand { get; init; } = default!;
        public string CardLast4 { get; init; } = default!;
        public string? AcquirerReference { get; init; }
        public int RetryCount { get; init; }
        public string? FailureReason { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public IReadOnlyCollection<GetPaymentEventResponse> Events { get; init; } = Array.Empty<GetPaymentEventResponse>();
    }
}
