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
    public sealed class Merchant
    {
        public string Id { get; private set; } = default!;
        public string Name { get; private set; } = default!;
        public MerchantStatus Status { get; private set; }
        public decimal MaxAmount { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private Merchant()
        {
        }

        public Merchant(string id, string name, MerchantStatus status, decimal maxAmount, DateTime createdAt)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new DomainException(
                    "merchant.id.required",
                    "MerchantId is required.");

            if (id.Trim().Length > 50)
                throw new DomainException(
                    "merchant.id.invalid_length",
                    "MerchantId length must be 50 characters or less.");

            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException(
                    "merchant.name.required",
                    "Merchant name is required.");

            if (name.Trim().Length > 100)
                throw new DomainException(
                    "merchant.name.invalid_length",
                    "Merchant name length must be 100 characters or less.");

            if (maxAmount <= 0)
                throw new DomainException(
                    "merchant.max_amount.invalid",
                    "Merchant max amount must be greater than zero.");

            Id = id.Trim();
            Name = name.Trim();
            Status = status;
            MaxAmount = maxAmount;
            CreatedAt = createdAt;
        }

        public static Merchant Create(string id, string name, MerchantStatus status, decimal maxAmount)
        {
            return new Merchant(
                id,
                name,
                status,
                maxAmount,
                DateTime.UtcNow);
        }

        public void EnsureIsActive()
        {
            if (Status == MerchantStatus.Suspended)
            {
                throw new DomainException(
                    "merchant.suspended",
                    $"Merchant '{Id}' is suspended.");
            }

            if (Status == MerchantStatus.Blocked)
            {
                throw new DomainException(
                    "merchant.blocked",
                    $"Merchant '{Id}' is blocked.");
            }
        }

        public void EnsureCanProcess(Money money)
        {
            ArgumentNullException.ThrowIfNull(money);

            EnsureIsActive();

            if (money.Amount > MaxAmount)
            {
                throw new DomainException(
                    "merchant.amount_exceeds_limit",
                    $"Amount exceeds the maximum allowed for merchant '{Id}'.");
            }
        }

        public void Suspend()
        {
            if (Status == MerchantStatus.Blocked)
            {
                throw new DomainException(
                    "merchant.invalid_status_transition",
                    "Blocked merchant cannot be suspended.");
            }

            Status = MerchantStatus.Suspended;
        }

        public void Block()
        {
            Status = MerchantStatus.Blocked;
        }

        public void Activate()
        {
            if (Status == MerchantStatus.Blocked)
            {
                throw new DomainException(
                    "merchant.invalid_status_transition",
                    "Blocked merchant cannot be activated directly.");
            }

            Status = MerchantStatus.Active;
        }
    }
}
