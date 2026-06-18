using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Constants
{
    public static class TransactionEventTypes
    {
        public const string TransactionCreated = "transaction.created";
        public const string TransactionProcessingStarted = "transaction.processing";
        public const string TransactionApproved = "transaction.approved";
        public const string TransactionDeclined = "transaction.declined";
        public const string TransactionFailed = "transaction.failed";
        public const string TransactionRetryAttempted = "transaction.retry.attempted";
        public const string TransactionRetryScheduled = "transaction.retry.scheduled";
    }
}
