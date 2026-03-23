using System.Linq.Expressions;
using EventsManagement.Domain.Common;
using EventsManagement.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace EventsManagement.Repository.Implementations;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> entites;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        entites = _context.Set<T>();
    }

    public async Task<T> InsertAsync(T entity)
    {
        _context.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<ICollection<T>> InsertManyAsync(ICollection<T> entity) {
        _context.AddRange(entity);
        await _context.SaveChangesAsync();
        return entity;
    }


    public async Task<T> UpdateAsync(T entity)
    { 
        _context.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<T> DeleteAsync(T entity)
    {
        _context.Remove(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<E?> GetAsync<E>(Expression<Func<T, E>> selector,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
    {
        IQueryable<T> query = entites;
        if (include != null)
        { 
            query = include(query);
        }

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        return await query.Select(selector).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<E>> GetAllAsync<E>(Expression<Func<T, E>> selector,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
        {
            IQueryable<T> query = entites;

            if (include != null)
            {
                query = include(query);
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (orderBy != null)
            {
                orderBy(query);
            }

            return await query.Select(selector).ToListAsync();
        }

}