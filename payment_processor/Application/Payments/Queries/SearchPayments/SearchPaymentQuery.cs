using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Payments.Queries.SearchPayments
{
    public sealed record SearchPaymentQuery
    {
        public string? MerchantId { get; set; }
        public string? Status { get; set; }
    }
}
