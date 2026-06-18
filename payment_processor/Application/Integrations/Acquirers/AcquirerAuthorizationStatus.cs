using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Integrations.Acquirers
{
    public enum AcquirerAuthorizationStatus
    {
        Approved = 1,
        Declined = 2,
        Failed = 3
    }
}
