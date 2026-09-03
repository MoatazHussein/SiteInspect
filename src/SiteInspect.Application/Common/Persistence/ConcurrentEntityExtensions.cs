using SiteInspect.Application.Common.Exceptions;
using SiteInspect.Domain.Common.Entities;

namespace SiteInspect.Application.Common.Persistence;

public static class ConcurrentEntityExtensions
{
    public static void EnsureCurrentVersion(
        this ConcurrentAuditableEntity entity,
        string expectedRowVersion)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (!RowVersionToken.Matches(entity.RowVersion, expectedRowVersion))
        {
            throw new StaleRowVersionException();
        }
    }
}
