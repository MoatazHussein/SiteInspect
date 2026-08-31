using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SiteInspect.Domain.Common.Entities;

namespace SiteInspect.Infrastructure.Persistence.Configurations.Common;

internal abstract class AuditableEntityConfiguration<TEntity> : BaseEntityConfiguration<TEntity>
    where TEntity : AuditableEntity
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        base.Configure(builder);

        builder.Property(entity => entity.CreatedAtUtc)
            .HasPrecision(7)
            .IsRequired();

        builder.Property(entity => entity.LastModifiedAtUtc)
            .HasPrecision(7);
    }
}
