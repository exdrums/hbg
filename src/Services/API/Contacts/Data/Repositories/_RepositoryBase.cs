using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace API.Contacts.Data.Repositories;

/// <summary>
/// Base class for all repository implementations that extends DbSet
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
public abstract class RepositoryBase<TEntity> : DbSet<TEntity> where TEntity : class
{
    protected readonly DbContext Context;
    private readonly DbSet<TEntity> _dbSet;

    protected RepositoryBase(DbContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<TEntity>();
        // _dbSet = this;
    }

    // DbSet implementation delegations
    public override IEntityType EntityType => _dbSet.EntityType;
    
    public override LocalView<TEntity> Local => _dbSet.Local;

    public override IAsyncEnumerable<TEntity> AsAsyncEnumerable()
    {
        return _dbSet.AsAsyncEnumerable();
    }

    public override IQueryable<TEntity> AsQueryable()
    {
        return _dbSet.AsQueryable();
    }

    public override TEntity Find(params object[] keyValues)
    {
        return _dbSet.Find(keyValues);
    }

    public override ValueTask<TEntity> FindAsync(object[] keyValues, CancellationToken cancellationToken)
    {
        return _dbSet.FindAsync(keyValues, cancellationToken);
    }

    public override ValueTask<TEntity> FindAsync(params object[] keyValues)
    {
        return _dbSet.FindAsync(keyValues);
    }

    public override EntityEntry<TEntity> Add(TEntity entity)
    {
        return _dbSet.Add(entity);
    }

    public override ValueTask<EntityEntry<TEntity>> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        return _dbSet.AddAsync(entity, cancellationToken);
    }

    public override void AddRange(params TEntity[] entities)
    {
        _dbSet.AddRange(entities);
    }

    public override void AddRange(IEnumerable<TEntity> entities)
    {
        _dbSet.AddRange(entities);
    }

    public override Task AddRangeAsync(params TEntity[] entities)
    {
        return _dbSet.AddRangeAsync(entities);
    }

    public override Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        return _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    public override EntityEntry<TEntity> Attach(TEntity entity)
    {
        return _dbSet.Attach(entity);
    }

    public override void AttachRange(params TEntity[] entities)
    {
        _dbSet.AttachRange(entities);
    }

    public override void AttachRange(IEnumerable<TEntity> entities)
    {
        _dbSet.AttachRange(entities);
    }

    public override EntityEntry<TEntity> Remove(TEntity entity)
    {
        return _dbSet.Remove(entity);
    }

    public override void RemoveRange(params TEntity[] entities)
    {
        _dbSet.RemoveRange(entities);
    }

    public override void RemoveRange(IEnumerable<TEntity> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    public override EntityEntry<TEntity> Update(TEntity entity)
    {
        return _dbSet.Update(entity);
    }

    public override void UpdateRange(params TEntity[] entities)
    {
        _dbSet.UpdateRange(entities);
    }

    public override void UpdateRange(IEnumerable<TEntity> entities)
    {
        _dbSet.UpdateRange(entities);
    }

    // Utility methods for repositories
    protected async Task<bool> SaveChangesAsync()
    {
        return await Context.SaveChangesAsync() > 0;
    }

    protected async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    protected async Task<List<TEntity>> ToListAsync()
    {
        return await _dbSet.ToListAsync();
    }

    protected async Task<List<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }
}
