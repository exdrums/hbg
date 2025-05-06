using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace API.Contacts.Domain.Repositories
{
    /// <summary>
    /// Generic repository interface for database operations
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Gets all entities
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Gets entities by filter
        /// </summary>
        Task<IEnumerable<T>> GetByFilterAsync(Expression<Func<T, bool>> filter);

        /// <summary>
        /// Gets entity by ID
        /// </summary>
        Task<T> GetByIdAsync(string id);

        /// <summary>
        /// Adds a new entity
        /// </summary>
        Task<bool> AddAsync(T entity);

        /// <summary>
        /// Updates an existing entity
        /// </summary>
        Task<bool> UpdateAsync(T entity);

        /// <summary>
        /// Deletes an entity
        /// </summary>
        Task<bool> DeleteAsync(T entity);

        /// <summary>
        /// Deletes an entity by ID
        /// </summary>
        Task<bool> DeleteByIdAsync(string id);

        /// <summary>
        /// Checks if entity exists by ID
        /// </summary>
        Task<bool> ExistsAsync(string id);

        /// <summary>
        /// Gets entity by filter
        /// </summary>
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> filter);
    }
}
