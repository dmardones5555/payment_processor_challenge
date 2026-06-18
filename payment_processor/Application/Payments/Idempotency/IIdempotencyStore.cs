using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Payments.Idempotency
{
    public interface IIdempotencyStore
    {
        Task<IdempotencyRecord?> GetActiveAsync(string merchantId, string idempotencyKey, CancellationToken cancellationToken);

        Task SaveAsync(IdempotencyRecord idempotencyRecord, CancellationToken cancellationToken);
    }
}
