using Application.Common.Logging;
using Domain.Exceptions;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Application.Payments.Queries.GetPayment
{    
    public class GetPaymentHandler
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ITransactionEventRepository _transactionEventRepository;
        private readonly ILogger<GetPaymentHandler> _logger;

        public GetPaymentHandler(
            ITransactionRepository transactionRepository,
            ITransactionEventRepository transactionEventRepository,
            ILogger<GetPaymentHandler> logger)
        {
            _transactionRepository = transactionRepository;
            _transactionEventRepository = transactionEventRepository;
            _logger = logger;
        }

        public async Task<GetPaymentResponse> Handle(Guid transactionId, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            var transaction = await _transactionRepository.GetByIdAsync(transactionId, cancellationToken);
            if (transaction == null)
            {
                throw new DomainException(
                        "transaction.not_found",
                        $"Transaction '{transactionId}' was not found.");
            }

            var events = await _transactionEventRepository.GetByTransactionIdAsync(transactionId, cancellationToken);

            var paymentResponse = new GetPaymentResponse
            {
                TransactionId = transaction.Id,
                MerchantId = transaction.MerchantId,
                Amount = transaction.Money.Amount,
                Currency = transaction.Money.Currency,
                Status = transaction.Status.ToString(),
                IdempotencyKey = transaction.IdempotencyKey,
                CardBrand = transaction.CardBrand,
                CardLast4 = transaction.CardLast4,
                AcquirerReference = transaction.AcquirerReference,
                RetryCount = transaction.RetryCount,
                FailureReason = transaction.FailureReason,
                CreatedAt = transaction.CreatedAt,
                UpdatedAt = transaction.UpdatedAt,
                Events = events.Select(e => new GetPaymentEventResponse
                {
                    EventId = e.Id,
                    TransactionId = transaction.Id,
                    EventType = e.EventType.ToString(),
                    DurationMs = e.DurationMs,
                    AttemptNumber = e.AttemptNumber,
                    Payload = e.Details,
                    CreatedAt = e.OccurredAtUtc,
                }).ToList()
            };

            stopwatch.Stop();
            _logger.LogInformation(
                "payment.get.completed {@LogData}",
                LogDataFactory.Create(
                    evento: "payment.get.completed",
                    timestamp: DateTime.UtcNow,
                    transactionId: transaction.Id,
                    durationMs: (int)stopwatch.ElapsedMilliseconds
                )
            );

            return paymentResponse;
        }
    }
}
