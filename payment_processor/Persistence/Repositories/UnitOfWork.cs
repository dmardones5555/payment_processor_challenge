using Application.Common.Interfaces.Persistence;
using Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly PaymentProcessorContext _dbContext;

        public UnitOfWork(PaymentProcessorContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void ClearTracking()
        {
            _dbContext.ChangeTracker.Clear();
        }

        public async Task CommitAsync(CancellationToken cancellationToken)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
