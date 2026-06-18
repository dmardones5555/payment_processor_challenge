using Application.Payments.Idempotency;
using Microsoft.EntityFrameworkCore;
using Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    public class IdempotencyStore : IIdempotencyStore
    {
        private PaymentProcessorContext _dbContext;

        public IdempotencyStore(PaymentProcessorContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IdempotencyRecord?> GetActiveAsync(string merchantId, string idempotencyKey, CancellationToken cancellationToken)
        {
            var nowUtc = DateTime.UtcNow;

            var entity = await _dbContext.PaymentIdempotencies
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.MerchantId == merchantId
                      && x.IdempotencyKey == idempotencyKey
                      && x.ExpiresAt > nowUtc,
                    cancellationToken);

            if (entity is null)
            {
                return null;
            }

            return new IdempotencyRecord(
                entity.MerchantId,
                entity.IdempotencyKey,
                entity.TransactionId,
                entity.RequestHash,
                entity.CreatedAt,
                entity.ExpiresAt);
        }
        

        public async Task SaveAsync(IdempotencyRecord idempotencyRecord, CancellationToken cancellationToken)
        {
            var entity = new PaymentIdempotency
            {
                MerchantId = idempotencyRecord.MerchantId,
                IdempotencyKey = idempotencyRecord.IdempotencyKey,
                TransactionId = idempotencyRecord.TransactionId,
                RequestHash = idempotencyRecord.RequestHash,
                CreatedAt = idempotencyRecord.CreatedAt,
                ExpiresAt = idempotencyRecord.ExpiresAt
            };

            await _dbContext.PaymentIdempotencies.AddAsync(entity, cancellationToken);
        }
    }
}
