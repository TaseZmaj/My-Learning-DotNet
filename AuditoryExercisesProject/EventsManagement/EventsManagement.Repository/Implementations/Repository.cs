using System.Linq.Expressions;
using EventsManagement.Domain.Common;
using EventsManagement.Domain.Dto;
using EventsManagement.Domain.Entities;
using EventsManagement.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace EventsManagement.Repository.Implementations;

public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext _context;
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

        public async Task<ICollection<T>> InsertManyAsync(ICollection<T> entity)
        {
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
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
            int? take = null)
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
                // orderBy(query);
                query = orderBy(query);
            }
            
            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }
            
            return await query.Select(selector).ToListAsync();
        }
        
        public async Task<PaginatedResult<E>> GetAllPagedAsync<E>(
            Expression<Func<T, E>> selector,
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
            bool asNoTracking = false
        )
        {
            IQueryable<T> query = entites;

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

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            IQueryable<E> projectedQuery = query.Select(selector);
            
            var totalCount = await projectedQuery.CountAsync();
            

            var items = await projectedQuery
                .Skip(pageNumber * pageSize)
                .Take(pageSize)
                .ToListAsync();

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

    }