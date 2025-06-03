using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace NLightning.Infrastructure.Repositories.Database;

using Persistence.Contexts;
using Helpers;

public class BaseDbRepository<TEntity> where TEntity : class
{
    private readonly NLightningDbContext _context;
    protected readonly DbSet<TEntity> DbSet;
    
    protected BaseDbRepository(NLightningDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        
        _context = context;
        DbSet = context.Set<TEntity>();
    }
    
    protected IQueryable<TEntity> Get(Expression<Func<TEntity, bool>>? predicate = null,
                                              Expression<Func<TEntity, object>>? include = null,
                                              Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, 
                                              bool asNoTracking = true, int perPage = 0, int pageNumber = 1)
    {
        var query = asNoTracking ? DbSet.AsNoTracking() : DbSet;

        if (predicate is not null)
            query = query.Where(predicate);

        if (include is not null)
            query = query.Include(include);
        
        if (perPage > 0)
            query = query.Skip((pageNumber - 1) * perPage).Take(perPage);

        return orderBy is not null ? orderBy(query) : query;
    }
    
    protected async Task<TEntity?> GetByIdAsync(object id, bool asNoTracking = true,
                                                        Expression<Func<TEntity, object>>? include = null)
    {
        var query = asNoTracking ? DbSet.AsNoTracking() : DbSet;
        
        if (include is not null)
            query = query.Include(include);

        var lambdaPredicate = PrimaryKeyHelper.GetPrimaryKeyExpression<TEntity>(id, _context)
            ?? throw new InvalidOperationException(
                $"Entity {typeof(TEntity).Name} does not have a primary key defined.");
        
        query = query.Where(lambdaPredicate);

        return await query.FirstOrDefaultAsync();
    }
    
    protected void Insert(TEntity entity)
    {
        DbSet.Add(entity);
    }
    
    protected void Delete(TEntity entityToDelete)
    {
        if (_context.Entry(entityToDelete).State == EntityState.Detached)
            DbSet.Attach(entityToDelete);

        DbSet.Remove(entityToDelete);
    }
    
    protected async Task DeleteByIdAsync(object id)
    {
        var entityToDelete = await GetByIdAsync(id, false)
            ?? throw new InvalidOperationException($"Entity with id {id} not found.");
        
        Delete(entityToDelete);
    }
    
    protected void DeleteRange(IEnumerable<TEntity> entitiesToDelete)
    {
        var iEnumerable = entitiesToDelete as TEntity[] ?? entitiesToDelete.ToArray();
        if (iEnumerable.Length == 0)
            return;

        foreach (var entity in iEnumerable)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
                DbSet.Attach(entity);
        }

        DbSet.RemoveRange(iEnumerable);
    }
    
    protected void DeleteWhere(Expression<Func<TEntity, bool>> predicate)
    {
        var entitiesToDelete = DbSet.Where(predicate).ToArray();
        if (entitiesToDelete.Length == 0)
            return;

        DeleteRange(entitiesToDelete);
    }
    
    protected void Update(TEntity entityToUpdate)
    {
        DbSet.Update(entityToUpdate);
    }
}