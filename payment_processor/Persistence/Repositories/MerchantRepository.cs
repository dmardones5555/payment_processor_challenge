using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    public class MerchantRepository : IMerchantRepository
    {
        private PaymentProcessorContext _dbContext;

        public MerchantRepository(PaymentProcessorContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Domain.Entities.Merchant?> GetByIdAsync(string merchantId, CancellationToken cancellationToken)
        {
            //get the merchant from the database by id
            var entity = await _dbContext.Merchants
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MerchantId == merchantId, cancellationToken);

            if (entity is null)
            {
                throw new InvalidOperationException($"Merchant '{merchantId}' was not found.");
            }

            return new Domain.Entities.Merchant(
                entity.MerchantId,
                entity.Name,
                MapStatus(entity.Status),
                entity.MaxAmount,
                entity.CreatedAt
            );
        }

        private static MerchantStatus MapStatus(string status)
        {
            return status switch
            {
                "ACTIVE" => MerchantStatus.Active,
                "SUSPENDED" => MerchantStatus.Suspended,
                "BLOCKED" => MerchantStatus.Blocked,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported merchant status.")
            };
        }
    }
}
