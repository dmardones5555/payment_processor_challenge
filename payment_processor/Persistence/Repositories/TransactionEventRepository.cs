using Application.Payments.Queries.GetPayment;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    public class TransactionEventRepository : ITransactionEventRepository
    {
        private readonly PaymentProcessorContext _dbContext;

        public TransactionEventRepository(PaymentProcessorContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Domain.Entities.TransactionEvent transactionEvent, CancellationToken cancellationToken)
        {
            var entity = MapToPersistence(transactionEvent);

            await _dbContext.TransactionEvents.AddAsync(entity, cancellationToken);
        }

        public async Task<List<Domain.Entities.TransactionEvent>> GetByTransactionIdAsync(Guid transactionId, CancellationToken cancellationToken)
        {
            List<Domain.Entities.TransactionEvent> transactionEvents = new List<Domain.Entities.TransactionEvent>();

            var events =  await _dbContext.TransactionEvents
                .AsNoTracking()
                .Where(e => e.TransactionId == transactionId)
                .OrderBy(e => e.CreatedAt)
                .ToListAsync(cancellationToken);

            if (events != null && events.Count > 0)
            {
                foreach (var e in events)                
                    transactionEvents.Add(MapToDomain(e));                                    
            }

            return transactionEvents;
        }

        private static Entities.TransactionEvent MapToPersistence(Domain.Entities.TransactionEvent transactionEvent)
        {
            return new Entities.TransactionEvent
            {
                TransactionId = transactionEvent.TransactionId,
                EventType = transactionEvent.EventType,
                DurationMs = transactionEvent.DurationMs,
                AttemptNumber = transactionEvent.AttemptNumber,
                Payload = transactionEvent.Details is null ? null
                : JsonSerializer.Serialize(new { message = transactionEvent.Details }),
                CreatedAt = transactionEvent.OccurredAtUtc
            };
        }

        private static Domain.Entities.TransactionEvent MapToDomain(Entities.TransactionEvent entity)
        {
            string? details = null;

            if (!string.IsNullOrWhiteSpace(entity.Payload))
            {
                try
                {
                    var payload = JsonSerializer.Deserialize<TransactionEventPayload>(entity.Payload);
                    details = payload?.Message;
                }
                catch (JsonException)
                {
                    details = null;
                }
            }

            return Domain.Entities.TransactionEvent.Load(
                id: entity.EventId,
                transactionId: entity.TransactionId,
                eventType: entity.EventType,
                occurredAtUtc: entity.CreatedAt,
                durationMs: entity.DurationMs,
                details: details,
                attemptNumber: entity.AttemptNumber);

        }

        private sealed class TransactionEventPayload
        {
            public string? Message { get; init; }
        }

    }
}
