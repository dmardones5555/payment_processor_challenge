using Application.Common.Interfaces.Persistence;
using Application.Integrations.Acquirers;
using Application.Payments.Outbox;
using Application.Payments.Reconciliation;
using Domain.Constants;
using Domain.Entities;
using Domain.Interfaces;
using Persistence.Repositories;

namespace PaymentWorker.Services
{
    public sealed class PaymentAuthorizationProcessor
    {
        private const int MaxRetries = 3;

        private readonly IOutboxRepository _outboxRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ITransactionEventRepository _transactionEventRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAcquirerGateway _acquirerGateway;
        private readonly IReconciliationQueueRepository _reconciliationQueueRepository;
        private readonly ILogger<PaymentAuthorizationProcessor> _logger;

        public PaymentAuthorizationProcessor(
            IOutboxRepository outboxRepository,
            ITransactionRepository transactionRepository,
            ITransactionEventRepository transactionEventRepository,
            IUnitOfWork unitOfWork,
            IAcquirerGateway acquirerGateway,
            IReconciliationQueueRepository reconciliationQueueRepository,
            ILogger<PaymentAuthorizationProcessor> logger)
        {
            _outboxRepository = outboxRepository;
            _transactionRepository = transactionRepository;
            _transactionEventRepository = transactionEventRepository;
            _unitOfWork = unitOfWork;
            _acquirerGateway = acquirerGateway;
            _reconciliationQueueRepository = reconciliationQueueRepository;
            _logger = logger;
        }

        public async Task ProcessPendingAsync(CancellationToken cancellationToken)
        {
            var pendingMessages = await _outboxRepository.GetPendingAsync(
                eventType: OutboxMessageTypes.PaymentCreated,
                batchSize: 10,
                cancellationToken: cancellationToken);

            foreach (var message in pendingMessages)
            {
                try
                {
                    await ProcessMessageAsync(message, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Unexpected error while processing outbox message {OutboxMessageId}.",
                        message.Id);

                    await _outboxRepository.MarkFailedAsync(
                        message.Id,
                        ex.Message,
                        cancellationToken);

                    await _unitOfWork.CommitAsync(cancellationToken);
                }
            }
        }

        private async Task ProcessMessageAsync(OutboxMessageData message, CancellationToken cancellationToken)
        {
            var nowUtc = DateTime.UtcNow;

            await _outboxRepository.MarkProcessingAsync(message.Id, cancellationToken);

            var transaction = await _transactionRepository.GetByIdAsync(
                message.AggregateId,
                cancellationToken);

            if (transaction is null)
            {
                await _outboxRepository.MarkFailedAsync(
                    message.Id,
                    $"Transaction '{message.AggregateId}' was not found.",
                    cancellationToken);

                await _unitOfWork.CommitAsync(cancellationToken);
                return;
            }

            if (transaction.Status == Domain.Enums.TransactionStatus.PENDING)
            {
                transaction.MarkProcessing(nowUtc);

                await _transactionRepository.UpdateAsync(transaction, cancellationToken);

                await _transactionEventRepository.AddAsync(
                    TransactionEvent.Create(
                        transaction.Id,
                        TransactionEventTypes.TransactionProcessingStarted,
                        occurredAtUtc: nowUtc),
                    cancellationToken);

                await _unitOfWork.CommitAsync(cancellationToken);
                _unitOfWork.ClearTracking();
            }

            var startedAt = DateTime.UtcNow;

            var result = await _acquirerGateway.AuthorizeAsync(
                new AcquirerAuthorizationRequest(
                    transaction.Id,
                    transaction.MerchantId,
                    transaction.Money.Amount,
                    transaction.Money.Currency,
                    transaction.CardBrand,
                    transaction.CardLast4),
                cancellationToken);

            var finishedAt = DateTime.UtcNow;
            var durationMs = (int?)(finishedAt - startedAt).TotalMilliseconds;

            try
            {
                if (result.Status == AcquirerAuthorizationStatus.Approved)
                {
                    transaction.Approve(
                        result.AcquirerReference ?? string.Empty,
                        finishedAt);

                    await _transactionRepository.UpdateAsync(transaction, cancellationToken);

                    await _transactionEventRepository.AddAsync(
                        TransactionEvent.Create(
                            transaction.Id,
                            TransactionEventTypes.TransactionApproved,
                            occurredAtUtc: finishedAt,
                            durationMs: durationMs),
                        cancellationToken);

                    await _outboxRepository.MarkProcessedAsync(
                        message.Id,
                        finishedAt,
                        cancellationToken);

                    await _unitOfWork.CommitAsync(cancellationToken);
                    return;
                }

                if (result.Status == AcquirerAuthorizationStatus.Declined)
                {
                    transaction.Decline(
                        result.ResponseMessage ?? "Declined by acquirer.",
                        finishedAt);

                    await _transactionRepository.UpdateAsync(transaction, cancellationToken);

                    await _transactionEventRepository.AddAsync(
                        TransactionEvent.Create(
                            transaction.Id,
                            TransactionEventTypes.TransactionDeclined,
                            occurredAtUtc: finishedAt,
                            durationMs: durationMs),
                        cancellationToken);

                    await _outboxRepository.MarkProcessedAsync(
                        message.Id,
                        finishedAt,
                        cancellationToken);

                    await _unitOfWork.CommitAsync(cancellationToken);
                    return;
                }

                await HandleFailureAsync(
                    transaction,
                    message,
                    result,
                    finishedAt,
                    durationMs,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                try
                {
                    await HandlePersistenceFailureAfterAcquirerResponseAsync(
                        transaction,
                        message,
                        result,
                        ex,
                        cancellationToken);

                    return;
                }
                catch (Exception reconciliationEx)
                {
                    _logger.LogCritical(
                        reconciliationEx,
                        "Critical failure while registering reconciliation for transaction {TransactionId} after persistence failure.",
                        transaction.Id);

                    throw;
                }
            }
        }

        private async Task HandleFailureAsync(Transaction transaction, OutboxMessageData message, AcquirerAuthorizationResult result, DateTime occurredAtUtc, int? durationMs, CancellationToken cancellationToken)
        {
            transaction.IncrementRetry(occurredAtUtc);

            if (result.IsTransientFailure && transaction.RetryCount < MaxRetries)
            {
                await _transactionRepository.UpdateAsync(transaction, cancellationToken);

                var delay = CalculateBackoff(transaction.RetryCount);

                await _transactionEventRepository.AddAsync(
                    TransactionEvent.Create(
                        transaction.Id,
                        TransactionEventTypes.TransactionRetryScheduled,
                        occurredAtUtc: occurredAtUtc,
                        durationMs: durationMs,
                        details: $"Retry scheduled in {delay.TotalSeconds} seconds."),
                        cancellationToken);

                await _outboxRepository.RequeueAsync(
                    message.Id,
                    transaction.RetryCount,
                    result.ResponseMessage ?? "Temporary acquirer failure.",
                    occurredAtUtc.Add(delay),
                    cancellationToken);

                await _unitOfWork.CommitAsync(cancellationToken);
                return;
            }

            transaction.Fail(result.ResponseMessage ?? "Authorization failed.", occurredAtUtc);

            await _transactionRepository.UpdateAsync(transaction, cancellationToken);

            await _transactionEventRepository.AddAsync(
                TransactionEvent.Create(
                    transaction.Id,
                    TransactionEventTypes.TransactionFailed,
                    occurredAtUtc: occurredAtUtc,
                    durationMs: durationMs),
                cancellationToken);

            await _outboxRepository.MarkFailedAsync(
                message.Id,
                result.ResponseMessage ?? "Authorization failed.",
                cancellationToken);

            var shouldEnqueue = ReconciliationPolicy.ShouldEnqueueAfterRetriesExhausted(transaction, result, MaxRetries);
            if (shouldEnqueue)
            {
                var payload = ReconciliationPayloadFactory.Create(
                    transaction,
                    result,
                    reason: "authorization_failed_after_max_retries",
                    enqueuedAtUtc: occurredAtUtc);

                await _reconciliationQueueRepository.AddAsync(
                    transaction.Id,
                    result.AcquirerReference,
                    payload,
                    occurredAtUtc,
                    cancellationToken);
            }

            await _unitOfWork.CommitAsync(cancellationToken);
        }

        private async Task HandlePersistenceFailureAfterAcquirerResponseAsync(Transaction transaction, OutboxMessageData message, AcquirerAuthorizationResult result, Exception exception, CancellationToken cancellationToken)
        {
            var nowUtc = DateTime.UtcNow;

            var payload = ReconciliationPayloadFactory.Create(
                transaction,
                result,
                reason: "persistence_failure_after_acquirer_response",
                enqueuedAtUtc: nowUtc);

            await _reconciliationQueueRepository.AddAsync(
                transaction.Id,
                result.AcquirerReference,
                payload,
                nowUtc,
                cancellationToken);

            await _outboxRepository.MarkFailedAsync(
                message.Id,
                $"Persistence failure after acquirer response: {exception.Message}",
                cancellationToken);

            _logger.LogError(
                exception,
                "Persistence failure after acquirer response for transaction {TransactionId}. Reconciliation item created.",
                transaction.Id);

            await _unitOfWork.CommitAsync(cancellationToken);
        }

        private static TimeSpan CalculateBackoff(int retryCount)
        {
            var seconds = Math.Pow(2, retryCount);
            return TimeSpan.FromSeconds(seconds);
        }
    }
}
