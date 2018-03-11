using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DAL.Interfaces;
using ProjectedEntities;

namespace DAL
{
    internal class GenericRepository<TEntity, TProjectedEntity, TId> :
        IGenericRepository<TEntity, TProjectedEntity, TId>,
        IDisposable
        where TEntity : class, IEntity<TId>, new()
        where TProjectedEntity : class, IProjectedEntity<TId>, new()
        where TId: IEquatable<TId>
    {
        private readonly DbSet<TEntity> _entityDbSet;

        public DbContext DbContext { get; set; }

        public GenericRepository(DbContext context)
        {
            DbContext = context;
            _entityDbSet = context.Set<TEntity>();
        }

        #region Public Methods

        /// <summary>
        /// Gets the entities collection asynchronous.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="orderBy">The order by.</param>
        /// <param name="numberOfElemetsToRetrieve">The number of elemets to retrieve.</param>
        /// <param name="includeRelatedProperties">The include related properties.</param>
        /// <param name="noTrackingChanges">if set to <c>true</c> [no tracking changes].</param>
        /// <returns></returns>
        public virtual async Task<IList<TEntity>> GetEntitiesCollectionAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? numberOfElemetsToRetrieve = null,
            string[] includeRelatedProperties = null,
            bool noTrackingChanges = true)
        {
            IQueryable<TEntity> query = GetProcessedQuery(
                filter,
                orderBy,
                numberOfElemetsToRetrieve,
                includeRelatedProperties,
                noTrackingChanges);

            if (query != null)
            {
                return await query.ToListAsync();
            }

            return null;
        }

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
        public virtual async Task<IList<TProjectedEntity>> GetProjectedEntitiesCollectionAsync(
            Expression<Func<TEntity, TProjectedEntity>> selectedFields,
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? numberOfElemetsToRetrieve = null,
            string[] includeRelatedProperties = null,
            bool noTrackingChanges = true)
        {
            IQueryable<TEntity> query = GetProcessedQuery(
                filter,
                orderBy,
                numberOfElemetsToRetrieve,
                includeRelatedProperties,
                noTrackingChanges);

            if (query != null)
            {
                return await query.Select(selectedFields).ToListAsync();
            }

            return null;
        }

        /// <summary>
        /// Gets the entity by identifier asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public virtual async Task<TEntity> GetEntityByIdAsync(TId id)
        {
            return await _entityDbSet.FindAsync(id);
        }

        public virtual async Task<TProjectedEntity> GetProjectedEntityByIdAsync(
            TId id,
            Expression<Func<TEntity, TProjectedEntity>> selectedFields)
        {
            var col = await GetProjectedEntitiesCollectionAsync(
                selectedFields,
                ent => ent.Id.Equals(id),
                //ent => ent.Id == id,
                //EqualsPredicate(id),
                numberOfElemetsToRetrieve: 1);

            if (col != null && col.Count > 0)
                return col[0];

            return null;
        }

        /// <summary>
        /// Inserts the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public virtual void Insert(TEntity entity)
        {
            _entityDbSet.Add(entity);
        }

        /// <summary>
        /// Deletes the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public virtual void Delete(TId id)
        {
            TEntity entityToDelete = new TEntity
            {
                Id = id
            };

            Delete(entityToDelete);
        }

        /// <summary>
        /// Deletes the specified entity to delete.
        /// </summary>
        /// <param name="entityToDelete">The entity to delete.</param>
        public virtual void Delete(TEntity entityToDelete)
        {
            if (DbContext.Entry(entityToDelete).State == EntityState.Detached)
            {
                _entityDbSet.Attach(entityToDelete);
            }

            _entityDbSet.Remove(entityToDelete);
        }

        /// <summary>
        /// Updates the full entity.
        /// </summary>
        /// <param name="entityToUpdate">The entity to update.</param>
        public virtual void UpdateFullEntity(TEntity entityToUpdate)
        {
            _entityDbSet.Attach(entityToUpdate);
            DbContext.Entry(entityToUpdate).State = EntityState.Modified;
        }

        /// <summary>
        /// Updates the fields in entity.
        /// </summary>
        /// <param name="entityToUpdate">The entity to update.</param>
        /// <param name="modifiedProperties">The modified properties.</param>
        public virtual void UpdateFieldsInEntity(TEntity entityToUpdate, IList<string> modifiedProperties)
        {
            _entityDbSet.Attach(entityToUpdate);
            foreach (var propItem in modifiedProperties)
            {
                DbContext.Entry(entityToUpdate).Property(propItem).IsModified = true;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets the processed query.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="orderBy">The order by.</param>
        /// <param name="numberOfElemetsToRetrieve">The number of elemets to retrieve.</param>
        /// <param name="includeRelatedProperties">The include related properties.</param>
        /// <param name="noTrackingChanges">if set to <c>true</c> [no tracking changes].</param>
        /// <returns></returns>
        private IQueryable<TEntity> GetProcessedQuery(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? numberOfElemetsToRetrieve = null,
            string[] includeRelatedProperties = null,
            bool noTrackingChanges = true)
        {
            IQueryable<TEntity> query = _entityDbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (numberOfElemetsToRetrieve != null && numberOfElemetsToRetrieve.Value > 0)
            {
                query = query.Take(numberOfElemetsToRetrieve.Value);
            }

            if (includeRelatedProperties != null && includeRelatedProperties.Length > 0)
            {
                foreach (var includeProperty in includeRelatedProperties)
                {
                    query = query.Include(includeProperty);
                }
            }

            if (noTrackingChanges)
            {
                query = query.AsNoTracking();
            }

            return query;
        }

        #endregion

        #region Dispose Pattern

        private bool _disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                DbContext?.Dispose();

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}