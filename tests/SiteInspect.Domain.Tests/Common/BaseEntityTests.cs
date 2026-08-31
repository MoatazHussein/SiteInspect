using FluentAssertions;
using SiteInspect.Domain.Common.Entities;

namespace SiteInspect.Domain.Tests.Common;

public sealed class BaseEntityTests
{
    [Fact]
    public void NewEntityHasNonEmptyIdentifier()
    {
        var entity = new TestEntity();

        entity.Id.Should().NotBeEmpty();
        entity.Id.Version.Should().Be(7);
    }

    [Fact]
    public void ConcurrentEntityStartsWithoutDatabaseRowVersion()
    {
        var entity = new TestConcurrentEntity();

        entity.RowVersion.Should().BeEmpty();
    }

    [Fact]
    public void AuditMetadataRecordsCreationAndModificationSeparately()
    {
        var entity = new TestAuditableEntity();
        var createdAt = new DateTimeOffset(2026, 8, 29, 8, 0, 0, TimeSpan.Zero);
        var modifiedAt = createdAt.AddMinutes(5);
        var creatorId = Guid.NewGuid();
        var modifierId = Guid.NewGuid();

        entity.SetCreatedAudit(createdAt, creatorId);
        entity.SetModifiedAudit(modifiedAt, modifierId);

        entity.CreatedAtUtc.Should().Be(createdAt);
        entity.CreatedBy.Should().Be(creatorId);
        entity.LastModifiedAtUtc.Should().Be(modifiedAt);
        entity.LastModifiedBy.Should().Be(modifierId);
    }

    private sealed class TestEntity : BaseEntity;

    private sealed class TestAuditableEntity : AuditableEntity;

    private sealed class TestConcurrentEntity : ConcurrentAuditableEntity;
}
