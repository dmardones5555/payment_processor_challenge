using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Application.Integrations.Acquirers;
using Integrations.Acquirers.Contracts;

namespace Integrations.Acquirers
{
    public sealed class HttpAcquirerGateway : IAcquirerGateway
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HttpAcquirerGateway> _logger;

        public HttpAcquirerGateway(HttpClient httpClient, ILogger<HttpAcquirerGateway> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<AcquirerAuthorizationResult> AuthorizeAsync(AcquirerAuthorizationRequest request, CancellationToken cancellationToken)
        {
            var httpRequest = new AuthorizePaymentHttpRequest
            {
                TransactionId = request.TransactionId,
                MerchantId = request.MerchantId,
                Amount = request.Amount,
                Currency = request.Currency,
                CardBrand = request.CardBrand,
                CardLast4 = request.CardLast4
            };

            try
            {
                using var response = await _httpClient.PostAsJsonAsync(
                    "authorize",
                    httpRequest,
                    cancellationToken);

                if (response.StatusCode == HttpStatusCode.RequestTimeout || response.StatusCode == HttpStatusCode.GatewayTimeout)
                {
                    return AcquirerAuthorizationResult.TemporaryFailure(
                        responseCode: ((int)response.StatusCode).ToString(),
                        responseMessage: "Acquirer timeout.");
                }

                if ((int)response.StatusCode >= 500)
                {
                    return AcquirerAuthorizationResult.TemporaryFailure(
                        responseCode: ((int)response.StatusCode).ToString(),
                        responseMessage: "Acquirer temporary failure.");
                }

                if ((int)response.StatusCode >= 400)
                {
                    return AcquirerAuthorizationResult.PermanentFailure(
                        responseCode: ((int)response.StatusCode).ToString(),
                        responseMessage: "Acquirer rejected request due to client error.");
                }

                var body = await response.Content.ReadFromJsonAsync<AuthorizePaymentHttpResponse>(cancellationToken: cancellationToken);

                if (body is null)
                {
                    return AcquirerAuthorizationResult.TemporaryFailure(
                        responseMessage: "Acquirer returned an empty response.");
                }

                return MapResponse(body);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(
                    "Timeout while calling acquirer for transaction {TransactionId}.",
                    request.TransactionId);

                return AcquirerAuthorizationResult.TemporaryFailure(
                    responseMessage: "Acquirer request timed out.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(
                    ex,
                    "HTTP error while calling acquirer for transaction {TransactionId}.",
                    request.TransactionId);

                return AcquirerAuthorizationResult.TemporaryFailure(
                    responseMessage: "Acquirer communication error.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while calling acquirer for transaction {TransactionId}.",
                    request.TransactionId);

                return AcquirerAuthorizationResult.PermanentFailure(
                    responseMessage: "Unexpected acquirer integration error.");
            }
        }

        private static AcquirerAuthorizationResult MapResponse(AuthorizePaymentHttpResponse response)
        {
            return response.Status.ToUpperInvariant() switch
            {
                "APPROVED" => AcquirerAuthorizationResult.Approved(
                    authorizationCode: response.AuthorizationCode ?? string.Empty,
                    acquirerReference: response.AcquirerReference ?? string.Empty,
                    responseCode: response.ResponseCode,
                    responseMessage: response.ResponseMessage),

                "DECLINED" => AcquirerAuthorizationResult.Declined(
                    responseCode: response.ResponseCode,
                    responseMessage: response.ResponseMessage,
                    acquirerReference: response.AcquirerReference),

                "TEMPORARY_FAILURE" => AcquirerAuthorizationResult.TemporaryFailure(
                    responseCode: response.ResponseCode,
                    responseMessage: response.ResponseMessage,
                    acquirerReference: response.AcquirerReference),

                "FAILED" => AcquirerAuthorizationResult.PermanentFailure(
                    responseCode: response.ResponseCode,
                    responseMessage: response.ResponseMessage,
                    acquirerReference: response.AcquirerReference),

                _ => AcquirerAuthorizationResult.PermanentFailure(
                    responseMessage: $"Unknown acquirer status '{response.Status}'.")
            };
        }
    }
}
