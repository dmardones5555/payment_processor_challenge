using Application.Integrations.Acquirers;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.Payments.Reconciliation
{
    public static class ReconciliationPayloadFactory
    {
        public static string Create(
            Transaction transaction,
            AcquirerAuthorizationResult result,
            string reason,
            DateTime enqueuedAtUtc)
        {
            var payload = new ReconciliationPayload
            {
                Reason = reason,
                TransactionId = transaction.Id,
                MerchantId = transaction.MerchantId,
                Amount = transaction.Money.Amount,
                Currency = transaction.Money.Currency,
                CurrentStatus = transaction.Status.ToString(),
                RetryCount = transaction.RetryCount,
                AcquirerReference = result.AcquirerReference,
                AcquirerStatus = result.Status.ToString(),
                ResponseMessage = result.ResponseMessage,
                IsTransientFailure = result.IsTransientFailure,
                EnqueuedAtUtc = enqueuedAtUtc
            };

            return JsonSerializer.Serialize(payload);
        }
    }
}
