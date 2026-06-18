using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integrations.Acquirers.Contracts
{
    public sealed class AuthorizePaymentHttpResponse
    {
        public string Status { get; set; } = string.Empty;
        public string? AuthorizationCode { get; set; }
        public string? AcquirerReference { get; set; }
        public string? ResponseCode { get; set; }
        public string? ResponseMessage { get; set; }
    }
}
