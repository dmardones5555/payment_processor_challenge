using Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ValueObjects
{
    public record Money
    {
        private static readonly IReadOnlyDictionary<string, int> CurrencyScale = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["CLP"] = 0,
            ["USD"] = 2,
            ["EUR"] = 2,
        };
        public decimal Amount { get; set; }
        public string Currency { get; set; }

        public Money(decimal amount, string currency)
        {
            if (amount <= 0)
                throw new DomainException("Amount must be greater than zero.");

            if (string.IsNullOrWhiteSpace(currency))
                throw new DomainException("Currency is required.");

            currency = currency.Trim().ToUpperInvariant();

            if (!CurrencyScale.TryGetValue(currency, out var scale))
                throw new DomainException($"Currency '{currency}' is not supported.");

            //if (GetScale(amount) > scale)
            //    throw new DomainException(
            //        $"Currency '{currency}' supports up to {scale} decimal places.");

            if (decimal.Round(amount, scale, MidpointRounding.AwayFromZero) != amount)            
                throw new DomainException(
                    $"Currency '{currency}' supports up to {scale} decimal places.");
            



            Amount = amount;
            Currency = currency;
        }

        private static int GetScale(decimal value)
        {
            var bits = decimal.GetBits(value);
            return (bits[3] >> 16) & 0xFF;
        }
    }
}
