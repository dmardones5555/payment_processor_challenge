using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Integrations.Acquirers
{
    public sealed record AcquirerAuthorizationResult
    {
        public AcquirerAuthorizationStatus Status { get; init; }
        public string? AuthorizationCode { get; init; }
        public string? AcquirerReference { get; init; }
        public string? ResponseCode { get; init; }
        public string? ResponseMessage { get; init; }
        public bool IsTransientFailure { get; init; }

        private AcquirerAuthorizationResult(
            AcquirerAuthorizationStatus status,
            string? authorizationCode,
            string? acquirerReference,
            string? responseCode,
            string? responseMessage,
            bool isTransientFailure)
        {
            Status = status;
            AuthorizationCode = authorizationCode;
            AcquirerReference = acquirerReference;
            ResponseCode = responseCode;
            ResponseMessage = responseMessage;
            IsTransientFailure = isTransientFailure;
        }

        public static AcquirerAuthorizationResult Approved(
            string authorizationCode,
            string acquirerReference,
            string? responseCode = null,
            string? responseMessage = null)
        {
            return new AcquirerAuthorizationResult(
                AcquirerAuthorizationStatus.Approved,
                authorizationCode,
                acquirerReference,
                responseCode,
                responseMessage,
                false);
        }

        public static AcquirerAuthorizationResult Declined(
            string? responseCode = null,
            string? responseMessage = null,
            string? acquirerReference = null)
        {
            return new AcquirerAuthorizationResult(
                AcquirerAuthorizationStatus.Declined,
                null,
                acquirerReference,
                responseCode,
                responseMessage,
                false);
        }

        public static AcquirerAuthorizationResult TemporaryFailure(
            string? responseCode = null,
            string? responseMessage = null,
            string? acquirerReference = null)
        {
            return new AcquirerAuthorizationResult(
                AcquirerAuthorizationStatus.Failed,
                null,
                acquirerReference,
                responseCode,
                responseMessage,
                true);
        }

        public static AcquirerAuthorizationResult PermanentFailure(
            string? responseCode = null,
            string? responseMessage = null,
            string? acquirerReference = null)
        {
            return new AcquirerAuthorizationResult(
                AcquirerAuthorizationStatus.Failed,
                null,
                acquirerReference,
                responseCode,
                responseMessage,
                false);
        }
    }

}
