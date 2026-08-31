using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SiteInspect.Application.Common.Abstractions.Persistence;
using SiteInspect.Domain.Common.Entities;

namespace SiteInspect.Infrastructure.Persistence.Repositories;

internal sealed class Repository<TEntity>(ApplicationDbContext dbContext) : IRepository<TEntity>
    where TEntity : BaseEntity
{
    private readonly DbSet<TEntity> dbSet = dbContext.Set<TEntity>();

    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbSet.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

    public async Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken = default) =>
        await dbSet.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TEntity>> ListAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default) =>
        await dbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);

    public Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default) =>
        dbSet.AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken);

    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        dbSet.AddAsync(entity, cancellationToken).AsTask();

    public Task AddRangeAsync(IEnumerable<TEntity> entitiesToAdd, CancellationToken cancellationToken = default) =>
        dbSet.AddRangeAsync(entitiesToAdd, cancellationToken);

    public void Update(TEntity entity) => dbSet.Update(entity);

    public void Remove(TEntity entity) => dbSet.Remove(entity);
}
