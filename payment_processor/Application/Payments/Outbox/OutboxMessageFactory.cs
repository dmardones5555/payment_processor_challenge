using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.Payments.Outbox
{
    public class OutboxMessageFactory
    {
        public static OutboxMessageData ForPaymentCreated(Transaction transaction)
        {
            var payload = JsonSerializer.Serialize(new
            {
                transactionId = transaction.Id,
                merchantId = transaction.MerchantId,
                amount = transaction.Money.Amount,
                currency = transaction.Money.Currency,
                status = transaction.Status.ToString(),
                createdAt = transaction.CreatedAt
            });

            return new OutboxMessageData(
                transaction.Id,
                OutboxMessageTypes.PaymentCreated,
                payload,
                transaction.CreatedAt);
        }
    }
}
