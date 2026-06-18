using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Integrations.Acquirers
{
    public sealed record AcquirerAuthorizationRequest
    {
        public Guid TransactionId { get; init; }
        public string MerchantId { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; }
        public string CardBrand { get; init; }
        public string CardLast4 { get; init; }

        public AcquirerAuthorizationRequest(
            Guid transactionId,
            string merchantId,
            decimal amount,
            string currency,
            string cardBrand,
            string cardLast4)
        {
            TransactionId = transactionId;
            MerchantId = merchantId;
            Amount = amount;
            Currency = currency;
            CardBrand = cardBrand;
            CardLast4 = cardLast4;
        }
    }
}
