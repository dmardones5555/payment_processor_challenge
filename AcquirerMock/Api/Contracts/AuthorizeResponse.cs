namespace Api.Contracts
{
    public sealed class AuthorizeResponse
    {
        public string Status { get; set; } = string.Empty;
        public string? AuthorizationCode { get; set; }
        public string? AcquirerReference { get; set; }
        public string? ResponseCode { get; set; }
        public string? ResponseMessage { get; set; }
    }
}
