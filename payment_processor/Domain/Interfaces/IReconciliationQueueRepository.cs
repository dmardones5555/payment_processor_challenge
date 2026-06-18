using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IReconciliationQueueRepository
    {
        Task AddAsync(Guid transactionId, string? acquirerReference, string responsePayload, DateTime createdAtUtc, CancellationToken cancellationToken);
    }
}
