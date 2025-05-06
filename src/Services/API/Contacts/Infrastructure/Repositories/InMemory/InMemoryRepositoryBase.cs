using API.Contacts.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace API.Contacts.Infrastructure.Repositories.InMemory
{
    /// <summary>
    /// Base class for in-memory repository implementations
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public abstract class InMemoryRepositoryBase<T> : IRepository<T> where T : class
    {
        protected readonly Dictionary<string, T> _entities = new Dictionary<string, T>();
        protected readonly object _lock = new object();

        /// <summary>
        /// Gets the ID for an entity
        /// </summary>
        protected abstract string GetId(T entity);

        /// <summary>
        /// Gets all entities
        /// </summary>
        public Task<IEnumerable<T>> GetAllAsync()
        {
            lock (_lock)
            {
                return Task.FromResult(_entities.Values.AsEnumerable());
            }
        }

        /// <summary>
        /// Gets entities by filter
        /// </summary>
        public Task<IEnumerable<T>> GetByFilterAsync(Expression<Func<T, bool>> filter)
        {
            lock (_lock)
            {
                var compiledFilter = filter.Compile();
                return Task.FromResult(_entities.Values.Where(compiledFilter).AsEnumerable());
            }
        }

        /// <summary>
        /// Gets entity by ID
        /// </summary>
        public Task<T> GetByIdAsync(string id)
        {
            lock (_lock)
            {
                _entities.TryGetValue(id, out var entity);
                return Task.FromResult(entity);
            }
        }

        /// <summary>
        /// Adds a new entity
        /// </summary>
        public Task<bool> AddAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            lock (_lock)
            {
                var id = GetId(entity);
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentException("Entity ID cannot be null or empty");
                }

                if (_entities.ContainsKey(id))
                {
                    return Task.FromResult(false);
                }

                _entities[id] = entity;
                return Task.FromResult(true);
            }
        }

        /// <summary>
        /// Updates an existing entity
        /// </summary>
        public Task<bool> UpdateAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            lock (_lock)
            {
                var id = GetId(entity);
                if (string.IsNullOrEmpty(id))
                {
                    throw new ArgumentException("Entity ID cannot be null or empty");
                }

                if (!_entities.ContainsKey(id))
                {
                    return Task.FromResult(false);
                }

                _entities[id] = entity;
                return Task.FromResult(true);
            }
        }

        /// <summary>
        /// Deletes an entity
        /// </summary>
        public Task<bool> DeleteAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            lock (_lock)
            {
                var id = GetId(entity);
                return Task.FromResult(_entities.Remove(id));
            }
        }

        /// <summary>
        /// Deletes an entity by ID
        /// </summary>
        public Task<bool> DeleteByIdAsync(string id)
        {
            lock (_lock)
            {
                return Task.FromResult(_entities.Remove(id));
            }
        }

        /// <summary>
        /// Checks if entity exists by ID
        /// </summary>
        public Task<bool> ExistsAsync(string id)
        {
            lock (_lock)
            {
                return Task.FromResult(_entities.ContainsKey(id));
            }
        }

        /// <summary>
        /// Gets entity by filter
        /// </summary>
        public Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> filter)
        {
            lock (_lock)
            {
                var compiledFilter = filter.Compile();
                return Task.FromResult(_entities.Values.FirstOrDefault(compiledFilter));
            }
        }
    }
}
