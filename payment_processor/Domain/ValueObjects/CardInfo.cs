using Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ValueObjects
{
    public sealed record CardInfo
    {
        public string Pan { get; }
        public string ExpiryMonth { get; }
        public string ExpiryYear { get; }
        public string Cvv { get; }
        public string Brand { get; }
        public string Last4 { get; }

        public CardInfo(string pan, string expiry, string cvv)
        {
            if (string.IsNullOrWhiteSpace(pan))
                throw new DomainException("card.pan.required", "Card PAN is required.");

            pan = NormalizeDigits(pan);

            if (!pan.All(char.IsDigit))
                throw new DomainException("card.pan.invalid", "Card PAN must contain only digits.");

            if (pan.Length < 13 || pan.Length > 19)
                throw new DomainException("card.pan.invalid_length", "Card PAN length is invalid.");

            if (!IsValidLuhn(pan))
                throw new DomainException("card.pan.invalid_luhn", "Card PAN is invalid.");

            if (string.IsNullOrWhiteSpace(expiry))
                throw new DomainException("card.expiry.required", "Card expiry is required.");

            var (month, year) = ParseExpiry(expiry);

            if (IsExpired(month, year))
                throw new DomainException("card.expired", "Card is expired.");

            if (string.IsNullOrWhiteSpace(cvv))
                throw new DomainException("card.cvv.required", "Card CVV is required.");

            cvv = cvv.Trim();

            if (!cvv.All(char.IsDigit))
                throw new DomainException("card.cvv.invalid", "Card CVV must contain only digits.");

            if (cvv.Length is < 3 or > 4)
                throw new DomainException("card.cvv.invalid_length", "Card CVV length is invalid.");

            Pan = pan;
            ExpiryMonth = month.ToString("00");
            ExpiryYear = year.ToString();
            Cvv = cvv;
            Brand = DetectBrand(pan);
            Last4 = pan[^4..];
        }

        private static string NormalizeDigits(string value)
        {
            return new string(value.Where(char.IsDigit).ToArray());
        }

        private static bool IsValidLuhn(string pan)
        {
            var sum = 0;
            var alternate = false;

            for (var i = pan.Length - 1; i >= 0; i--)
            {
                var n = pan[i] - '0';

                if (alternate)
                {
                    n *= 2;
                    if (n > 9)
                        n -= 9;
                }

                sum += n;
                alternate = !alternate;
            }

            return sum % 10 == 0;
        }

        private static (int Month, int Year) ParseExpiry(string expiry)
        {
            var parts = expiry.Split('/');

            if (parts.Length != 2)
                throw new DomainException("card.expiry.invalid_format", "Card expiry must have format MM/YY.");

            if (!int.TryParse(parts[0], out var month))
                throw new DomainException("card.expiry.invalid_month", "Card expiry month is invalid.");

            if (!int.TryParse(parts[1], out var shortYear))
                throw new DomainException("card.expiry.invalid_year", "Card expiry year is invalid.");

            if (month < 1 || month > 12)
                throw new DomainException("card.expiry.invalid_month", "Card expiry month must be between 01 and 12.");

            var year = 2000 + shortYear;

            return (month, year);
        }

        private static bool IsExpired(int month, int year)
        {
            var now = DateTime.UtcNow;
            var expiryDate = new DateTime(year, month, DateTime.DaysInMonth(year, month), 23, 59, 59, DateTimeKind.Utc);

            return expiryDate < now;
        }

        private static string DetectBrand(string pan)
        {
            if (pan.StartsWith("4"))
                return "VISA";

            if (pan.Length >= 2 &&
                int.TryParse(pan[..2], out var firstTwo) &&
                firstTwo is >= 51 and <= 55)
                return "MASTERCARD";

            if (pan.StartsWith("34") || pan.StartsWith("37"))
                return "AMEX";

            return "UNKNOWN";
        }
    }
}
