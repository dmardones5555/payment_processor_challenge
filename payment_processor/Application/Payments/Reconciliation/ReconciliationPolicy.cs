using Application.Integrations.Acquirers;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Payments.Reconciliation
{
    public static class ReconciliationPolicy
    {
        public static bool ShouldEnqueueAfterRetriesExhausted(Transaction transaction, AcquirerAuthorizationResult result, int maxRetries)
        {
            return result.IsTransientFailure && transaction.RetryCount >= maxRetries;
        }

        public static bool ShouldEnqueueOnPersistenceFailure()
        {
            return true;
        }
    }
}
