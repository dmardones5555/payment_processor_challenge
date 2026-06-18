using Application.Payments.Outbox;
using Microsoft.EntityFrameworkCore;
using Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    public class OutboxRepository : IOutboxRepository
    {
        private readonly PaymentProcessorContext _dbContext;

        public OutboxRepository(PaymentProcessorContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task AddAsync(OutboxMessageData message, CancellationToken cancellationToken)
        {
            var entity = new OutboxMessage
            {
                Id = message.Id,
                AggregateId = message.AggregateId,
                EventType = message.EventType,
                Payload = message.Payload,
                Status = message.Status,
                AttemptCount = 0,
                ErrorMessage = null,
                CreatedAt = message.CreatedAt,
                ProcessedAt = null,
                NextAttemptAt = null
            };

            await _dbContext.OutboxMessages.AddAsync(entity, cancellationToken);
        }

        public async Task<IReadOnlyCollection<OutboxMessageData>> GetPendingAsync(string eventType, int batchSize, CancellationToken cancellationToken)
        {
            var nowUtc = DateTime.UtcNow;

            var entities = await _dbContext.OutboxMessages
                .Where(x => x.EventType == eventType
                         && x.Status == "PENDING"
                         && (x.NextAttemptAt == null || x.NextAttemptAt <= nowUtc))
                .OrderBy(x => x.CreatedAt)
                .Take(batchSize)
                .ToListAsync(cancellationToken);

            return entities
                .Select(e => new OutboxMessageData
                {                    
                    Id = e.Id,
                    AggregateId = e.AggregateId,
                    EventType = e.EventType,
                    Payload = e.Payload,
                    Status = e.Status,
                    CreatedAt = e.CreatedAt
                })
                .ToList()
                .AsReadOnly();
        }

        public async Task MarkProcessingAsync(Guid messageId, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.OutboxMessages
                .FirstOrDefaultAsync(x => x.Id == messageId, cancellationToken);

            if (entity is not null)
            {
                entity.Status = "PROCESSING";
                entity.AttemptCount++;
                entity.ErrorMessage = null;
            }

            await Task.CompletedTask;
        }

        public async Task MarkProcessedAsync(Guid messageId, DateTime processedAtUtc, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.OutboxMessages
                .FirstOrDefaultAsync(x => x.Id == messageId, cancellationToken);

            if (entity is not null)
            {
                entity.Status = "PROCESSED";
                entity.ProcessedAt = processedAtUtc;
                entity.ErrorMessage = null;
                entity.NextAttemptAt = null;
            }

            await Task.CompletedTask;
        }

        public async Task MarkFailedAsync(Guid messageId, string errorMessage, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.OutboxMessages
                .FirstOrDefaultAsync(x => x.Id == messageId, cancellationToken);

            if (entity is not null)
            {
                entity.Status = "FAILED";
                entity.ErrorMessage = errorMessage;
                entity.NextAttemptAt = null;
            }

            await Task.CompletedTask;
        }

        public async Task RequeueAsync(Guid messageId, int attemptCount, string errorMessage, DateTime nextAttemptAtUtc, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.OutboxMessages
                .FirstOrDefaultAsync(x => x.Id == messageId, cancellationToken);

            if (entity is not null)
            {
                entity.Status = "PENDING";
                entity.AttemptCount = attemptCount;
                entity.ErrorMessage = errorMessage;
                entity.NextAttemptAt = nextAttemptAtUtc;
            }

            await Task.CompletedTask;
        }
    }
}
