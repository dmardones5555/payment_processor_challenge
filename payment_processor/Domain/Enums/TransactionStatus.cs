using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum TransactionStatus
    {
        PENDING = 0,
        PROCESSING = 1,
        APPROVED = 2,
        DECLINED = 3,
        FAILED = 4
    }
}
