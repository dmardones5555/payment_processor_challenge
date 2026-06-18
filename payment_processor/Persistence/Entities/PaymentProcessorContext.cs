using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Entities;

public partial class PaymentProcessorContext : DbContext
{
    public PaymentProcessorContext()
    {
    }

    public PaymentProcessorContext(DbContextOptions<PaymentProcessorContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Merchant> Merchants { get; set; }

    public virtual DbSet<OutboxMessage> OutboxMessages { get; set; }

    public virtual DbSet<PaymentIdempotency> PaymentIdempotencies { get; set; }

    public virtual DbSet<ReconciliationQueue> ReconciliationQueues { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<TransactionEvent> TransactionEvents { get; set; }    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Merchant>(entity =>
        {
            entity.HasKey(e => e.MerchantId).HasName("PRIMARY");

            entity.ToTable("merchants");

            entity.Property(e => e.MerchantId)
                .HasMaxLength(50)
                .HasColumnName("merchant_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.MaxAmount).HasColumnName("max_amount");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("outbox_messages");

            entity.HasIndex(e => e.CreatedAt, "idx_outbox_created_at");

            entity.HasIndex(e => e.Status, "idx_outbox_status");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AggregateId).HasColumnName("aggregate_id");
            entity.Property(e => e.AttemptCount).HasColumnName("attempt_count");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.ErrorMessage)
                .HasMaxLength(500)
                .HasColumnName("error_message");
            entity.Property(e => e.EventType)
                .HasMaxLength(100)
                .HasColumnName("event_type");
            entity.Property(e => e.NextAttemptAt)
                .HasColumnType("datetime")
                .HasColumnName("next_attempt_at");
            entity.Property(e => e.Payload)
                .HasColumnType("json")
                .HasColumnName("payload");
            entity.Property(e => e.ProcessedAt)
                .HasColumnType("datetime")
                .HasColumnName("processed_at");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasColumnName("status");
        });

        modelBuilder.Entity<PaymentIdempotency>(entity =>
        {
            entity.HasKey(e => new { e.MerchantId, e.IdempotencyKey }).HasName("PRIMARY");

            entity.ToTable("payment_idempotency");

            entity.HasIndex(e => e.ExpiresAt, "idx_payment_idempotency_expires_at");

            entity.HasIndex(e => e.TransactionId, "idx_payment_idempotency_transaction_id");

            entity.Property(e => e.MerchantId)
                .HasMaxLength(50)
                .HasColumnName("merchant_id");
            entity.Property(e => e.IdempotencyKey)
                .HasMaxLength(100)
                .HasColumnName("idempotency_key");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt)
                .HasColumnType("datetime")
                .HasColumnName("expires_at");
            entity.Property(e => e.RequestHash)
                .HasMaxLength(64)
                .IsFixedLength()
                .HasColumnName("request_hash");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");

            entity.HasOne(d => d.Merchant).WithMany(p => p.PaymentIdempotencies)
                .HasForeignKey(d => d.MerchantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_payment_idempotency_merchant");

            entity.HasOne(d => d.Transaction).WithMany(p => p.PaymentIdempotencies)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_payment_idempotency_transaction");
        });

        modelBuilder.Entity<ReconciliationQueue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("reconciliation_queue");

            entity.HasIndex(e => e.Status, "idx_reconciliation_status");

            entity.HasIndex(e => e.TransactionId, "idx_reconciliation_transaction");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AcquirerReference)
                .HasMaxLength(100)
                .HasColumnName("acquirer_reference");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.ProcessedAt)
                .HasColumnType("datetime")
                .HasColumnName("processed_at");
            entity.Property(e => e.ResponsePayload)
                .HasColumnType("json")
                .HasColumnName("response_payload");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasColumnName("status");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");

            entity.HasOne(d => d.Transaction).WithMany(p => p.ReconciliationQueues)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_reconciliation_transaction");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PRIMARY");

            entity.ToTable("transactions");

            entity.HasIndex(e => e.CreatedAt, "idx_transactions_created_at");

            entity.HasIndex(e => new { e.MerchantId, e.CreatedAt }, "idx_transactions_merchant_created_at");

            entity.HasIndex(e => new { e.MerchantId, e.Status }, "idx_transactions_merchant_status");

            entity.HasIndex(e => e.Status, "idx_transactions_status");

            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.AcquirerReference)
                .HasMaxLength(100)
                .HasColumnName("acquirer_reference");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.CardBrand)
                .HasMaxLength(20)
                .HasColumnName("card_brand");
            entity.Property(e => e.CardLast4)
                .HasMaxLength(4)
                .IsFixedLength()
                .HasColumnName("card_last4");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .IsFixedLength()
                .HasColumnName("currency");
            entity.Property(e => e.FailureReason)
                .HasMaxLength(255)
                .HasColumnName("failure_reason");
            entity.Property(e => e.IdempotencyKey)
                .HasMaxLength(100)
                .HasColumnName("idempotency_key");
            entity.Property(e => e.MerchantId)
                .HasMaxLength(50)
                .HasColumnName("merchant_id");
            entity.Property(e => e.RetryCount).HasColumnName("retry_count");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Merchant).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.MerchantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_transactions_merchant");
        });

        modelBuilder.Entity<TransactionEvent>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PRIMARY");

            entity.ToTable("transaction_events");

            entity.HasIndex(e => e.CreatedAt, "idx_events_created_at");

            entity.HasIndex(e => e.TransactionId, "idx_events_transaction");

            entity.HasIndex(e => e.EventType, "idx_events_type");

            entity.Property(e => e.EventId).HasColumnName("event_id");
            entity.Property(e => e.AttemptNumber).HasColumnName("attempt_number");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DurationMs).HasColumnName("duration_ms");
            entity.Property(e => e.EventType)
                .HasMaxLength(50)
                .HasColumnName("event_type");
            entity.Property(e => e.NewStatus)
                .HasMaxLength(20)
                .HasColumnName("new_status");
            entity.Property(e => e.Payload)
                .HasColumnType("json")
                .HasColumnName("payload");
            entity.Property(e => e.PreviousStatus)
                .HasMaxLength(20)
                .HasColumnName("previous_status");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");

            entity.HasOne(d => d.Transaction).WithMany(p => p.TransactionEvents)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_transaction_events_transaction");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
