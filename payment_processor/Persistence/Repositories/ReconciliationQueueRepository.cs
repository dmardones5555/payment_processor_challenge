using Domain.Interfaces;
using Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    public class ReconciliationQueueRepository : IReconciliationQueueRepository
    {
        private readonly PaymentProcessorContext _dbContext;

        public ReconciliationQueueRepository(PaymentProcessorContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Guid transactionId, string? acquirerReference, string responsePayload, DateTime createdAtUtc, CancellationToken cancellationToken)
        {
            var entity = new ReconciliationQueue
            {
                TransactionId = transactionId,
                AcquirerReference = acquirerReference,
                ResponsePayload = responsePayload,
                Status = "PENDING",
                CreatedAt = createdAtUtc,
                ProcessedAt = null
            };

            await _dbContext.ReconciliationQueues.AddAsync(entity, cancellationToken);
        }
    }
}
