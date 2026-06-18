using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public sealed class Transaction
    {
        public Guid Id { get; private set; }
        public string MerchantId { get; private set; } = default!;
        public Money Money { get; private set; } = default!;
        public TransactionStatus Status { get; private set; }
        public string IdempotencyKey { get; private set; } = default!;
        public string CardBrand { get; private set; } = default!;
        public string CardLast4 { get; private set; } = default!;
        public string? AcquirerReference { get; private set; }
        public int RetryCount { get; private set; }
        public string? FailureReason { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private Transaction()
        {
        }

        public Transaction(Guid id, string merchantId, Money money, string idempotencyKey, string cardBrand, string cardLast4, DateTime createdAt)
        {
            if (string.IsNullOrWhiteSpace(merchantId))
                throw new DomainException(
                    "transaction.merchant_id.required",
                    "MerchantId is required.");

            if (string.IsNullOrWhiteSpace(idempotencyKey))
                throw new DomainException(
                    "transaction.idempotency_key.required",
                    "IdempotencyKey is required.");

            if (string.IsNullOrWhiteSpace(cardBrand))
                throw new DomainException(
                    "transaction.card_brand.required",
                    "CardBrand is required.");

            if (string.IsNullOrWhiteSpace(cardLast4))
                throw new DomainException(
                    "transaction.card_last4.required",
                    "CardLast4 is required.");

            if (cardLast4.Length != 4 || !cardLast4.All(char.IsDigit))
                throw new DomainException(
                    "transaction.card_last4.invalid",
                    "CardLast4 must contain exactly 4 digits.");

            Id = id;
            MerchantId = merchantId.Trim();
            Money = money;
            IdempotencyKey = idempotencyKey.Trim();
            CardBrand = cardBrand.Trim().ToUpperInvariant();
            CardLast4 = cardLast4;
            Status = TransactionStatus.PENDING;
            RetryCount = 0;
            CreatedAt = createdAt;
            UpdatedAt = createdAt;
        }

        public static Transaction Load(
            Guid id,
            string merchantId,
            Money money,
            TransactionStatus status,
            string idempotencyKey,
            string? cardBrand,
            string? cardLast4,
            string? acquirerReference,
            int retryCount,
            string? failureReason,
            DateTime createdAt,
            DateTime updatedAt)
        {
            return new Transaction
            {
                Id = id,
                MerchantId = merchantId,
                Money = money,
                Status = status,
                IdempotencyKey = idempotencyKey,
                CardBrand = cardBrand,
                CardLast4 = cardLast4,
                AcquirerReference = acquirerReference,
                RetryCount = retryCount,
                FailureReason = failureReason,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            };
        }

        public static Transaction Create(string merchantId, Money money, string idempotencyKey, string cardBrand, string cardLast4, DateTime createdAtUtc)
        {
            return new Transaction(
                Guid.NewGuid(),
                merchantId,
                money,
                idempotencyKey,
                cardBrand,
                cardLast4,
                createdAtUtc);
        }

        public void MarkProcessing(DateTime nowUtc)
        {
            EnsureCanTransitionTo(TransactionStatus.PROCESSING);

            Status = TransactionStatus.PROCESSING;
            UpdatedAt = nowUtc;
        }

        public void Approve(string? acquirerReference, DateTime finishedAt)
        {
            EnsureCanTransitionTo(TransactionStatus.APPROVED);

            Status = TransactionStatus.APPROVED;
            AcquirerReference = NormalizeOptional(acquirerReference);
            FailureReason = null;
            UpdatedAt = finishedAt;
        }

        public void Decline(string reason, DateTime finishedAt, string? acquirerReference = null)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new DomainException(
                    "transaction.decline_reason.required",
                    "Decline reason is required.");

            EnsureCanTransitionTo(TransactionStatus.DECLINED);

            Status = TransactionStatus.DECLINED;
            FailureReason = reason.Trim();
            AcquirerReference = NormalizeOptional(acquirerReference);
            UpdatedAt = finishedAt;
        }

        public void Fail(string reason, DateTime occurredAtUtc)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new DomainException(
                    "transaction.failure_reason.required",
                    "Failure reason is required.");

            EnsureCanTransitionTo(TransactionStatus.FAILED);

            Status = TransactionStatus.FAILED;
            FailureReason = reason.Trim();
            UpdatedAt = occurredAtUtc;
        }

        public void IncrementRetry(DateTime occurredAtUtc)
        {
            RetryCount++;
            UpdatedAt = occurredAtUtc;
        }

        private void EnsureCanTransitionTo(TransactionStatus newStatus)
        {
            var isValid = Status switch
            {
                TransactionStatus.PENDING
                    => newStatus == TransactionStatus.PROCESSING,

                TransactionStatus.PROCESSING
                    => newStatus is TransactionStatus.APPROVED
                        or TransactionStatus.DECLINED
                        or TransactionStatus.FAILED,

                TransactionStatus.APPROVED => false,
                TransactionStatus.DECLINED => false,
                TransactionStatus.FAILED => false,
                _ => false
            };

            if (!isValid)
            {
                throw new DomainException(
                    "transaction.invalid_status_transition",
                    $"Cannot transition transaction from {Status} to {newStatus}.");
            }
        }

        private static string? NormalizeOptional(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}
