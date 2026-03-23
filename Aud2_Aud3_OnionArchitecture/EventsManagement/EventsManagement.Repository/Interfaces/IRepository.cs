using System.Linq.Expressions;
using EventsManagement.Domain.Common;
using Microsoft.EntityFrameworkCore.Query;

namespace EventsManagement.Repository.Interfaces;

//moze da bide: IRepository<Event>, IRepository<Seat>, IRepository<Section>,...
public interface IRepository<T> where T : BaseEntity
{
    Task<T> InsertAsync(T entity);
    Task<ICollection<T>> InsertManyAsync(ICollection<T> entity);
    Task<T> UpdateAsync(T entity);
    Task<T> DeleteAsync(T entity);

    Task<E?> GetAsync<E>(Expression<Func<T, E>> selector,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null);

    Task<IEnumerable<E>> GetAllAsync<E>(Expression<Func<T, E>> selector,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null);
}