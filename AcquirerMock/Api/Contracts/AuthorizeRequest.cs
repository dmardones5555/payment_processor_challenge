namespace Api.Contracts
{
    public sealed class AuthorizeRequest
    {
        public Guid TransactionId { get; set; }
        public string MerchantId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string CardBrand { get; set; } = string.Empty;
        public string CardLast4 { get; set; } = string.Empty;
    }
}
