using NLightning.Domain.Bitcoin.Interfaces;
using NLightning.Infrastructure.Persistence.Contexts;
using NLightning.Infrastructure.Persistence.Entities.Bitcoin;

namespace NLightning.Infrastructure.Repositories.Database.Bitcoin;

public class RevocationWatchDbRepository(NLightningDbContext context)
    : BaseDbRepository<RevocationWatchEntity>(context), IRevocationWatchDbRepository
{
}