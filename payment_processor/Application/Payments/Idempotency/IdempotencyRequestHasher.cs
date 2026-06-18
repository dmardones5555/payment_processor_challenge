using Application.Payments.Commands.CreatePayment;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Application.Payments.Idempotency
{
    public static class IdempotencyRequestHasher
    {
        public static string Build(CreatePaymentCommand command)
        {
            var merchantId = NormalizeText(command.MerchantId);
            var amount = command.Amount.ToString("0.############################", CultureInfo.InvariantCulture);
            var currency = NormalizeText(command.Currency);
            var pan = NormalizeDigits(command.Card.Number);
            var expiry = NormalizeText(command.Card.Expiry);

            var raw = string.Join("|",
                merchantId,
                amount,
                currency,
                pan,
                expiry);

            var bytes = Encoding.UTF8.GetBytes(raw);
            var hashBytes = SHA256.HashData(bytes);

            return Convert.ToHexString(hashBytes);
        }

        private static string NormalizeText(string value)
        {
            return value.Trim().ToUpperInvariant();
        }

        private static string NormalizeDigits(string value)
        {
            return new string(value.Where(char.IsDigit).ToArray());
        }
    }
}
