using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ITransactionRepository
    {
        Task<Transaction?> GetByIdAsync(Guid transactionId, CancellationToken cancellationToken);
        Task AddAsync(Transaction transaction, CancellationToken cancellationToken);
        Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken);
        Task<List<Transaction>> GetTransactionsFilteredAsync(string merchantId, string status, CancellationToken cancellationToken);        
    }
}
