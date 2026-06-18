using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ITransactionEventRepository
    {
        Task AddAsync(TransactionEvent transactionEvent, CancellationToken cancellationToken);
        Task<List<TransactionEvent>> GetByTransactionIdAsync(Guid transactionId, CancellationToken cancellationToken);
    }
}
