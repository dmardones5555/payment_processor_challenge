using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Integrations.Acquirers
{
    public interface IAcquirerGateway
    {
        Task<AcquirerAuthorizationResult> AuthorizeAsync(AcquirerAuthorizationRequest request, CancellationToken cancellationToken);
    }
}
