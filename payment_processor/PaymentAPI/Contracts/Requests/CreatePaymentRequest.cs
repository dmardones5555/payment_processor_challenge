using Microsoft.AspNetCore.Http.HttpResults;

namespace PaymentAPI.Contracts.Requests
{
    public class CreatePaymentRequest
    {
        public string MerchantId { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; }

        public CardRequest Card { get; set; }

        public string IdempotencyKey { get; set; }
    }

    public class CardRequest
    {
        public string Number { get; set; }

        public string Expiry { get; set; }

        public string Cvv { get; set; }
    }
}
