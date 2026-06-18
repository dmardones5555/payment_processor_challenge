using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Logging
{
    public static class LogDataFactory
    {
        public static object Create(
            string evento,
            DateTime timestamp,
            Guid? transactionId = null,
            int? durationMs = null)
        {
            return new
            {
                transaction_id = transactionId,
                evento,
                timestamp,
                duration_ms = durationMs
            };
        }
    }
}
