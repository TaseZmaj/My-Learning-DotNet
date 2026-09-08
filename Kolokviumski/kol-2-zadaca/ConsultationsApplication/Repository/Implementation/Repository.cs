using System.Linq.Expressions;
using Domain.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Repository.Interface;

namespace Repository.Implementation;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _entities;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _entities = _context.Set<T>();
    }

    public async Task<T> DeleteAsync(T entity)
    {
        _context.Remove(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public Task<E?> GetAsync<E>(Expression<Func<T, E>> selector, Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
        bool asNoTracking = false)
    {
        IQueryable<T> query = _entities;
        if (predicate != null)
            query = query.Where(predicate);
        if (include != null)
            query = include(query);
        if (asNoTracking)
            query = query.AsNoTracking();

        var finalQuery = orderBy != null ? (IQueryable<T>)orderBy(query) : query;

        if (typeof(E) == typeof(T))
            return (Task<E?>)(object)finalQuery.FirstOrDefaultAsync();

        return finalQuery.Select(selector).FirstOrDefaultAsync();
    }

    public Task<List<E>> GetAllAsync<E>(Expression<Func<T, E>> selector,
        Expression<Func<T, bool>>? predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
        bool asNoTracking = false)
    {
        IQueryable<T> query = _entities;
        if (predicate != null)
            query = query.Where(predicate);

        if (include != null)
            query = include(query);

        if (orderBy != null)
            query = orderBy(query);

        if (asNoTracking)
            query = query.AsNoTracking();

        return query.Select(selector).ToListAsync();
    }

    public async Task<PaginatedResult<E>> GetAllPagedAsync<E>(
        Expression<Func<T, E>> selector,
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
        bool asNoTracking = false)
    {
        IQueryable<T> query = _entities;

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        if (include != null)
        {
            query = include(query);
        }

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        var totalCount = await query.CountAsync();

        IQueryable<E> pagedQuery;

        if (orderBy != null)
        {
            pagedQuery = orderBy(query)
                .Skip(pageNumber * pageSize)
                .Take(pageSize)
                .Select(selector);
        }
        else
        {
            pagedQuery = query
                .Skip(pageNumber * pageSize)
                .Take(pageSize)
                .Select(selector);
        }

        var items = await pagedQuery.ToListAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PaginatedResult<E>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages
        };
    }

    public async Task<List<TResult>> AggregateAsync<TKey, TResult>(
        Expression<Func<T, TKey>> groupBy,
        Expression<Func<IGrouping<TKey, T>, TResult>> selector,
        Expression<Func<T, bool>>? predicate = null,
        bool asNoTracking = false)
    {
        IQueryable<T> query = _entities;

        if (asNoTracking)
            query = query.AsNoTracking();

        if (predicate != null)
            query = query.Where(predicate);

        return await query
            .GroupBy(groupBy)
            .Select(selector)
            .ToListAsync();
    }

    async Task<T> IRepository<T>.InsertAsync(T entity)
    {
        _context.Add(entity);
        await _context.SaveChangesAsync();
        _context.Entry(entity).State = EntityState.Detached;
        return entity;
    }

    async Task<T> IRepository<T>.UpdateAsync(T entity)
    {
        _context.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return _entities.AnyAsync(predicate);
    }
}
