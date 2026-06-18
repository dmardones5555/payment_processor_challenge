using Application.Common.Logging;
using Application.Payments.Queries.GetPayment;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Application.Payments.Queries.SearchPayments
{
    public class SearchPaymentHandler
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ILogger<GetPaymentHandler> _logger;

        public SearchPaymentHandler(ITransactionRepository transactionRepository, ILogger<GetPaymentHandler> logger)
        {
            _transactionRepository = transactionRepository;
            _logger = logger;
        }

        public async Task<List<SearchPaymentResponse>> Handle(SearchPaymentQuery query, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            string? normalizedMerchantId = null;
            string? normalizedStatus = null;

            if (!string.IsNullOrWhiteSpace(query.MerchantId))
            {
                normalizedMerchantId = query.MerchantId.Trim();
            }

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                var rawStatus = query.Status.Trim();

                if (!Enum.TryParse<TransactionStatus>(rawStatus, ignoreCase: true, out var parsedStatus))
                {
                    throw new DomainException(
                        "transaction.status.invalid",
                        $"Status '{query.Status}' is not valid. Allowed values: PENDING, PROCESSING, APPROVED, DECLINED, FAILED.",
                        StatusCodes.Status400BadRequest);
                }

                normalizedStatus = parsedStatus.ToString().ToUpperInvariant();
            }

            var transactions = await _transactionRepository.GetTransactionsFilteredAsync(
                normalizedMerchantId,
                normalizedStatus,
                cancellationToken);

            stopwatch.Stop();
            _logger.LogInformation(
                "paymentDetail.get.completed {@LogData}",
                LogDataFactory.Create(
                    evento: "paymentDetail.get.completed",
                    timestamp: DateTime.UtcNow,
                    transactionId: null,
                    durationMs: (int)stopwatch.ElapsedMilliseconds
                )
            );

            return transactions
                .Select(t => new SearchPaymentResponse
                {
                    TransactionId = t.Id,
                    MerchantId = t.MerchantId,
                    Amount = t.Money.Amount,
                    Currency = t.Money.Currency,
                    Status = t.Status.ToString(),
                    AcquirerReference = t.AcquirerReference,
                    RetryCount = t.RetryCount,
                    FailureReason = t.FailureReason,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                })
                .ToList();
        }


    }
}
