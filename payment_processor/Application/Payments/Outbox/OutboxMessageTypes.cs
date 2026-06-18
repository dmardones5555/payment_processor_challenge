using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Payments.Outbox
{
    public static class OutboxMessageTypes
    {
        public const string PaymentCreated = "payment.created";
        public const string PaymentProcessingStarted = "payment.processing";
        public const string PaymentApproved = "payment.approved";
        public const string PaymentDeclined = "payment.declined";
        public const string PaymentFailed = "payment.failed";
        public const string PaymentRetryRequested = "payment.retry.requested";
    }
}
