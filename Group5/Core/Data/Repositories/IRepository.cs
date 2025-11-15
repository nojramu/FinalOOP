using System.Linq.Expressions;

namespace Group5.Data.Repositories
{
    /// <summary>
    /// Generic Repository Interface - Demonstrates ABSTRACTION
    /// Defines common CRUD operations for all entities
    /// UML Relationship: This will have AGGREGATION with concrete repositories
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Get entity by ID
        /// </summary>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Get all entities
        /// </summary>
        Task<List<T>> GetAllAsync();

        /// <summary>
        /// Find entities matching predicate
        /// </summary>
        Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Add new entity
        /// </summary>
        Task AddAsync(T entity);

        /// <summary>
        /// Update existing entity
        /// </summary>
        void Update(T entity);

        /// <summary>
        /// Delete entity
        /// </summary>
        void Delete(T entity);

        /// <summary>
        /// Save all changes to database
        /// </summary>
        Task<int> SaveChangesAsync();
    }
}

