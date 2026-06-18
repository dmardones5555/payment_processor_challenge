using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Payments.Commands.CreatePayment
{
    public record CreatePaymentResponse
    {
        public Guid TransactionId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
