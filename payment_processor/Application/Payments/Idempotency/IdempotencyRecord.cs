using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Payments.Idempotency
{
    public record IdempotencyRecord
    {
        public string MerchantId { get; set; } = default!;
        public string IdempotencyKey { get; set; } = default!;
        public Guid TransactionId { get; set; }
        public string RequestHash { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }

        public IdempotencyRecord(string merchantId, string idempotencyKey, Guid transactionId, string requestHash, DateTime createdAt, DateTime expiresAt)
        {
            MerchantId = merchantId;
            IdempotencyKey = idempotencyKey;
            TransactionId = transactionId;
            RequestHash = requestHash;
            CreatedAt = createdAt;
            ExpiresAt = expiresAt;
        }
    }
}
