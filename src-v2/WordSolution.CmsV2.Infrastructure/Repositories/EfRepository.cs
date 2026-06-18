using Microsoft.EntityFrameworkCore;
using WordSolution.CmsV2.Domain.Repositories;
using WordSolution.CmsV2.Infrastructure.Persistence;

namespace WordSolution.CmsV2.Infrastructure.Repositories;

public abstract class EfRepository<TEntity> : IRepository<TEntity>
    where TEntity : class
{
    protected EfRepository(CmsV2DbContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Set = context.Set<TEntity>();
    }

    protected CmsV2DbContext Context { get; }

    protected DbSet<TEntity> Set { get; }

    public async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Set
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => EF.Property<int>(entity, "Id") == id, cancellationToken);
    }

    public Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken = default)
    {
        return ToReadOnlyListAsync(
            Set.AsNoTracking().OrderBy(entity => EF.Property<int>(entity, "Id")),
            cancellationToken);
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await Set.AddAsync(entity, cancellationToken);
    }

    public void Update(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        DetachAlreadyTrackedEntityWithSameKey(entity);
        Set.Update(entity);
    }

    public void Remove(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        DetachAlreadyTrackedEntityWithSameKey(entity);
        Set.Remove(entity);
    }

    protected static async Task<IReadOnlyList<TItem>> ToReadOnlyListAsync<TItem>(
        IQueryable<TItem> query,
        CancellationToken cancellationToken)
    {
        return await query.ToListAsync(cancellationToken);
    }

    private void DetachAlreadyTrackedEntityWithSameKey(TEntity entity)
    {
        var primaryKey = Context.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey();
        var keyProperty = primaryKey?.Properties.SingleOrDefault();
        var propertyInfo = keyProperty?.PropertyInfo;
        if (keyProperty is null || propertyInfo is null)
        {
            return;
        }

        var keyValue = propertyInfo.GetValue(entity);
        if (keyValue is null || Equals(keyValue, 0))
        {
            return;
        }

        var trackedEntry = Context.ChangeTracker
            .Entries<TEntity>()
            .FirstOrDefault(entry =>
                !ReferenceEquals(entry.Entity, entity)
                && Equals(entry.Property(keyProperty.Name).CurrentValue, keyValue));

        if (trackedEntry is not null)
        {
            trackedEntry.State = EntityState.Detached;
        }
    }
}
