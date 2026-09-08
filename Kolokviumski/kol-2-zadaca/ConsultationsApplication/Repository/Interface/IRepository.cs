using System.Linq.Expressions;
using Domain.Dto;
using Microsoft.EntityFrameworkCore.Query;

namespace Repository.Interface;

public interface IRepository<T> where T : class
{
    Task<T> InsertAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<T> DeleteAsync(T entity);

    Task<E?> GetAsync<E>(Expression<Func<T, E>> selector,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
        bool asNoTracking = false
    );

    Task<List<E>> GetAllAsync<E>(Expression<Func<T, E>> selector,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
        bool asNoTracking = false);

    Task<PaginatedResult<E>> GetAllPagedAsync<E>(
        Expression<Func<T, E>> selector,
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
        bool asNoTracking = false);

    Task<List<TResult>> AggregateAsync<TKey, TResult>(
        Expression<Func<T, TKey>> groupBy,
        Expression<Func<IGrouping<TKey, T>, TResult>> selector,
        Expression<Func<T, bool>>? predicate = null,
        bool asNoTracking = false);

    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
}