using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Payments.Commands.CreatePayment
{
    public record CreatePaymentCommand
    {
        public string MerchantId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public CardCommand Card { get; set; }
        public string IdempotencyKey { get; set; }        
    }

    public sealed record CardCommand 
    {
        public string Number { get; set; }
        public string Expiry { get; set; }
        public string Cvv { get; set; }

    }
}
