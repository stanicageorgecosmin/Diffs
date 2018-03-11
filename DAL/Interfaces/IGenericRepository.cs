using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IGenericRepository<TEntity, TProjectedEntity, in TId> : IDisposable
    {
        DbContext DbContext { get; set; }

        /// <summary>
        /// Gets the entities collection asynchronous.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="orderBy">The order by.</param>
        /// <param name="numberOfElemetsToRetrieve">The number of elemets to retrieve.</param>
        /// <param name="includeRelatedProperties">The include related properties.</param>
        /// <param name="noTrackingChanges">if set to <c>true</c> [no tracking changes].</param>
        /// <returns></returns>
        Task<IList<TEntity>> GetEntitiesCollectionAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? numberOfElemetsToRetrieve = null,
            string[] includeRelatedProperties = null,
            bool noTrackingChanges = true);

        /// <summary>
        /// Gets the projected entities collection asynchronous.
        /// </summary>
        /// <param name="selectedFields">The selected fields.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="orderBy">The order by.</param>
        /// <param name="numberOfElemetsToRetrieve">The number of elemets to retrieve.</param>
        /// <param name="includeRelatedProperties">The include related properties.</param>
        /// <param name="noTrackingChanges">if set to <c>true</c> [no tracking changes].</param>
        /// <returns></returns>
        Task<IList<TProjectedEntity>> GetProjectedEntitiesCollectionAsync(
            Expression<Func<TEntity, TProjectedEntity>> selectedFields,
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? numberOfElemetsToRetrieve = null,
            string[] includeRelatedProperties = null,
            bool noTrackingChanges = true);

        /// <summary>
        /// Gets the entity by identifier asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        Task<TEntity> GetEntityByIdAsync(TId id);

        /// <summary>
        /// Gets the projected entity by identifier asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="selectedFields">The selected fields.</param>
        /// <returns></returns>
        Task<TProjectedEntity> GetProjectedEntityByIdAsync(
            TId id,
            Expression<Func<TEntity, TProjectedEntity>> selectedFields);

        /// <summary>
        /// Inserts the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        void Insert(TEntity entity);

        /// <summary>
        /// Deletes the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        void Delete(TId id);

        /// <summary>
        /// Deletes the specified entity to delete.
        /// </summary>
        /// <param name="entityToDelete">The entity to delete.</param>
        void Delete(TEntity entityToDelete);

        /// <summary>
        /// Updates the full entity.
        /// </summary>
        /// <param name="entityToUpdate">The entity to update.</param>
        void UpdateFullEntity(TEntity entityToUpdate);

        /// <summary>
        /// Updates the fields in entity.
        /// </summary>
        /// <param name="entityToUpdate">The entity to update.</param>
        /// <param name="modifiedProperties">The modified properties.</param>
        void UpdateFieldsInEntity(TEntity entityToUpdate, IList<string> modifiedProperties);
    }
}
