using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Payments.Queries.SearchPayments
{
    public record SearchPaymentResponse
    {
        public Guid TransactionId { get; set; }
        public string MerchantId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public string CardBrand { get; set; }
        public string CardLast4 { get; set; }
        public string? AcquirerReference { get; set; }
        public int RetryCount { get; set; }
        public string FailureReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
