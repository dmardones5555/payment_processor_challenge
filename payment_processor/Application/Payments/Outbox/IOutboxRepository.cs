using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Payments.Outbox
{
    public interface IOutboxRepository
    {
        Task AddAsync(OutboxMessageData message, CancellationToken cancellationToken);

        Task<IReadOnlyCollection<OutboxMessageData>> GetPendingAsync(
        string eventType,
        int batchSize,
        CancellationToken cancellationToken);

        Task MarkProcessingAsync(Guid messageId, CancellationToken cancellationToken);

        Task MarkProcessedAsync(Guid messageId, DateTime processedAtUtc, CancellationToken cancellationToken);

        Task MarkFailedAsync(Guid messageId, string errorMessage, CancellationToken cancellationToken);

        Task RequeueAsync(
            Guid messageId,
            int attemptCount,
            string errorMessage,
            DateTime nextAttemptAtUtc,
            CancellationToken cancellationToken);
    }
}
