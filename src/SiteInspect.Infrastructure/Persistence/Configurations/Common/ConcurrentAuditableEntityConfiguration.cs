using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SiteInspect.Domain.Common.Entities;

namespace SiteInspect.Infrastructure.Persistence.Configurations.Common;

internal abstract class ConcurrentAuditableEntityConfiguration<TEntity>
    : AuditableEntityConfiguration<TEntity>
    where TEntity : ConcurrentAuditableEntity
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        base.Configure(builder);

        builder.Property(entity => entity.RowVersion)
            .IsRowVersion();
    }
}
