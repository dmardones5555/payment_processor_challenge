using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Persistence.Entities;

namespace Persistence.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private PaymentProcessorContext _dbContext;

        public TransactionRepository(PaymentProcessorContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Domain.Entities.Transaction transaction, CancellationToken cancellationToken)
        {
            var entity = MapToPersistence(transaction);

            await _dbContext.Transactions.AddAsync(entity, cancellationToken);
        }

        public async Task<Domain.Entities.Transaction?> GetByIdAsync(Guid transactionId, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Transactions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TransactionId == transactionId, cancellationToken);

            return entity is null ? null : MapToDomain(entity);
        }

        public async Task UpdateAsync(Domain.Entities.Transaction transaction, CancellationToken cancellationToken)
        {
            var entity = _dbContext.Transactions.Local
                .FirstOrDefault(x => x.TransactionId == transaction.Id);

            if (entity is null)
            {
                entity = await _dbContext.Transactions
                    .FirstOrDefaultAsync(x => x.TransactionId == transaction.Id, cancellationToken);
            }

            if (entity is null)
            {
                throw new InvalidOperationException($"Transaction '{transaction.Id}' was not found.");
            }

            entity.MerchantId = transaction.MerchantId;
            entity.Amount = transaction.Money.Amount;
            entity.Currency = transaction.Money.Currency;
            entity.Status = MapStatus(transaction.Status);
            entity.IdempotencyKey = transaction.IdempotencyKey;
            entity.CardBrand = transaction.CardBrand;
            entity.CardLast4 = transaction.CardLast4;
            entity.AcquirerReference = transaction.AcquirerReference;
            entity.RetryCount = transaction.RetryCount;
            entity.FailureReason = transaction.FailureReason;
            entity.CreatedAt = transaction.CreatedAt;
            entity.UpdatedAt = transaction.UpdatedAt;
        }

        public async Task<List<Domain.Entities.Transaction>> GetTransactionsFilteredAsync(string? merchantId, string? status, CancellationToken cancellationToken)
        {
            IQueryable<Entities.Transaction> query = _dbContext.Transactions
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(merchantId))
            {
                var normalizedMerchantId = merchantId.Trim();
                query = query.Where(t => t.MerchantId == normalizedMerchantId);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                var normalizedStatus = status.Trim().ToUpperInvariant();
                query = query.Where(t => t.Status == normalizedStatus);
            }

            var entities = await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(cancellationToken);

            return entities
                .Select(MapToDomain)
                .ToList();
        }


        private static Domain.Entities.Transaction MapToDomain(Entities.Transaction entity)
        {

            var money = new Money(entity.Amount, entity.Currency);

            return Domain.Entities.Transaction.Load(
                id: entity.TransactionId,
                merchantId: entity.MerchantId,
                money: money,
                status: MapStatus(entity.Status),
                idempotencyKey: entity.IdempotencyKey,
                cardBrand: entity.CardBrand,
                cardLast4: entity.CardLast4,
                acquirerReference: entity.AcquirerReference,
                retryCount: entity.RetryCount,
                failureReason: entity.FailureReason,
                createdAt: entity.CreatedAt,
                updatedAt: entity.UpdatedAt);
        }

        private static Entities.Transaction MapToPersistence(Domain.Entities.Transaction transaction)
        {
            return new Entities.Transaction
            {
                TransactionId = transaction.Id,
                MerchantId = transaction.MerchantId,
                Amount = transaction.Money.Amount,
                Currency = transaction.Money.Currency,
                Status = MapStatus(transaction.Status),
                IdempotencyKey = transaction.IdempotencyKey,
                CardBrand = transaction.CardBrand,
                CardLast4 = transaction.CardLast4,
                AcquirerReference = transaction.AcquirerReference,
                RetryCount = transaction.RetryCount,
                FailureReason = transaction.FailureReason,
                CreatedAt = transaction.CreatedAt,
                UpdatedAt = transaction.UpdatedAt
            };
        }

        private static string MapStatus(TransactionStatus status)
        {
            return status switch
            {
                TransactionStatus.PENDING => "PENDING",
                TransactionStatus.PROCESSING => "PROCESSING",
                TransactionStatus.APPROVED => "APPROVED",
                TransactionStatus.DECLINED => "DECLINED",
                TransactionStatus.FAILED => "FAILED",
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported transaction status.")
            };
        }

        private static TransactionStatus MapStatus(string status)
        {
            return status switch
            {
                "PENDING" => TransactionStatus.PENDING,
                "PROCESSING" => TransactionStatus.PROCESSING,
                "APPROVED" => TransactionStatus.APPROVED,
                "DECLINED" => TransactionStatus.DECLINED,
                "FAILED" => TransactionStatus.FAILED,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported transaction status.")
            };
        }       
    }
}
