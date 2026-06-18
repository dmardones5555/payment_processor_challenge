namespace PaymentAPI.Contracts.Responses
{
    public class CreatePaymentResponse
    {
        public string TransactionId { get; set; }
        public string Status { get; set; }
        public string CreatedAt { get; set; }
    }
}
