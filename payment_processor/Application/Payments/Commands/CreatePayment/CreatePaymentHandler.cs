using Application.Common.Interfaces.Persistence;
using Application.Common.Logging;
using Application.Payments.Idempotency;
using Application.Payments.Outbox;
using Application.Payments.Queries.GetPayment;
using Domain.Constants;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Application.Payments.Commands.CreatePayment
{
    public class CreatePaymentHandler
    {
        private readonly IIdempotencyStore _idempotencyStore;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IMerchantRepository _merchantRepository;
        private readonly IOutboxRepository _outboxRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITransactionEventRepository _transactionEventRepository;
        private readonly ILogger<GetPaymentHandler> _logger;

        public CreatePaymentHandler(
            IIdempotencyStore idempotencyStore, 
            ITransactionRepository transactionRepository, 
            IMerchantRepository merchantRepository,
            IOutboxRepository outboxRepository,
            IUnitOfWork unitOfWork,
            ITransactionEventRepository transactionEventRepository,
            ILogger<GetPaymentHandler> logger)
        {            
            _idempotencyStore = idempotencyStore;  
            _transactionRepository = transactionRepository;
            _merchantRepository = merchantRepository;
            _outboxRepository = outboxRepository;
            _unitOfWork = unitOfWork;
            _transactionEventRepository = transactionEventRepository;
            _logger = logger;
        }

        public async Task<CreatePaymentResponse> Handle(CreatePaymentCommand command, CancellationToken cancellationToken)
        {
            var nowUtc = DateTime.UtcNow;
            var stopwatch = Stopwatch.StartNew();

            var merchantId = new MerchantId(command.MerchantId);
            var idempotencyKey = command.IdempotencyKey.Trim();

            var requestHash = IdempotencyRequestHasher.Build(command);

            var existingIdempotency = await _idempotencyStore.GetActiveAsync(
                merchantId.Value,
                idempotencyKey,
                cancellationToken);

            if (existingIdempotency is not null)
            {
                if (existingIdempotency.RequestHash != requestHash)
                {
                    throw new DomainException(
                        "idempotency_key.conflict",
                        "The same idempotency key was already used with a different request.",
                        StatusCodes.Status409Conflict);
                }

                var existingTransaction = await _transactionRepository.GetByIdAsync(
                    existingIdempotency.TransactionId,
                    cancellationToken);

                if (existingTransaction is null)
                {
                    throw new DomainException(
                        "transaction.not_found",
                        $"Transaction '{existingIdempotency.TransactionId}' was not found.");
                }

                return new CreatePaymentResponse
                {
                    TransactionId = existingTransaction.Id,
                    Status = existingTransaction.Status.ToString(),
                    CreatedAt = existingTransaction.CreatedAt
                };
            }

            var money = new Money(command.Amount, command.Currency);

            var cardInfo = new CardInfo(
                command.Card.Number,
                command.Card.Expiry,
                command.Card.Cvv);

            var merchant = await _merchantRepository.GetByIdAsync(
                merchantId.Value,
                cancellationToken);

            if (merchant is null)
            {
                throw new DomainException(
                    "merchant.not_found",
                    $"Merchant '{merchantId.Value}' was not found.");
            }

            merchant.EnsureCanProcess(money);

            var transaction = Transaction.Create(
                merchantId.Value,
                money,
                idempotencyKey,
                cardInfo.Brand,
                cardInfo.Last4,
                nowUtc);

            await _transactionRepository.AddAsync(transaction, cancellationToken);

            var transactionCreatedEvent = TransactionEvent.Create(
                transaction.Id,
                TransactionEventTypes.TransactionCreated,
                occurredAtUtc: nowUtc);

            await _transactionEventRepository.AddAsync(
                transactionCreatedEvent,
                cancellationToken);

            var idempotencyRecord = new IdempotencyRecord(
                merchant.Id,
                idempotencyKey,
                transaction.Id,
                requestHash,
                nowUtc,
                nowUtc.AddHours(24));

            await _idempotencyStore.SaveAsync(idempotencyRecord, cancellationToken);

            var outboxMessage = OutboxMessageFactory.ForPaymentCreated(transaction);
            await _outboxRepository.AddAsync(outboxMessage, cancellationToken);

            await _unitOfWork.CommitAsync(cancellationToken);

            stopwatch.Stop();
            _logger.LogInformation(
                "payment.create.completed {@LogData}",
                LogDataFactory.Create(
                    evento: "payment.create.completed",
                    timestamp: DateTime.UtcNow,
                    transactionId: transaction.Id,
                    durationMs: (int)stopwatch.ElapsedMilliseconds
                )
            );

            return new CreatePaymentResponse
            {
                TransactionId = transaction.Id,
                Status = transaction.Status.ToString(),
                CreatedAt = transaction.CreatedAt
            };
        }
    }
}
