using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Entities;

namespace DataAccessLayer
{
    internal class GenericRepository<TEntity, TId>: IDisposable 
        where TEntity : class, IEntity<TId>, new()
    {
        private readonly DbContext _dbContext;
        private readonly DbSet<TEntity> _entityDbSet;

        public GenericRepository(DbContext context)
        {
            this._dbContext = context;
            this._entityDbSet = context.Set<TEntity>();
        }

        /// <summary>
        /// Gets the processed query.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="orderBy">The order by.</param>
        /// <param name="numberOfElemetsToRetrieve">The number of elemets to retrieve.</param>
        /// <param name="includeProperties">includeProperties: new []{ $"{nameof(Recipient.DummyInstance.RecipientVouchers)}.{nameof(RecipientVoucher.DummyInstance.Voucher)}"
        /// </param>
        /// <param name="noTrackingChanges">if set to <c>true</c> [no tracking changes].</param>
        /// <returns></returns>
        public IQueryable<TEntity> GetProcessedQuery(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? numberOfElemetsToRetrieve = null,
            string[] includeProperties = null,
            bool noTrackingChanges = true)
        {
            IQueryable<TEntity> query = _entityDbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (includeProperties != null && includeProperties.Length > 0)
            {
                foreach (var includeProperty in includeProperties)
                {
                    query = query.Include(includeProperty);
                }
            }

            if (numberOfElemetsToRetrieve != null && numberOfElemetsToRetrieve.Value > 0)
            {
                query = query.Take(numberOfElemetsToRetrieve.Value);
            }

            query = orderBy != null ? orderBy(query) : query;

            if (noTrackingChanges)
            {
                query = query.AsNoTracking();
            }

            return query;
        }

        /// <summary>
        /// Gets the collection.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="orderBy">The order by.</param>
        /// <param name="numberOfElemetsToRetrieve">The number of elemets to retrieve.</param>
        /// <param name="includeProperties">includeProperties: new []{ $"{nameof(Recipient.DummyInstance.RecipientVouchers)}.{nameof(RecipientVoucher.DummyInstance.Voucher)}"
        /// </param>
        /// <param name="noTrackingChanges">if set to <c>true</c> [no tracking changes].</param>
        /// <returns></returns>
        public virtual IList<TEntity> GetCollection(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? numberOfElemetsToRetrieve = null, 
            string[] includeProperties = null,
            bool noTrackingChanges = true)
        {
            IQueryable<TEntity> query = GetProcessedQuery(filter,
                orderBy,
                numberOfElemetsToRetrieve,
                includeProperties,
                noTrackingChanges);

            return query?.ToList();
        }

        /// <summary>
        /// Gets the collection asynchronous.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="orderBy">The order by.</param>
        /// <param name="numberOfElemetsToRetrieve">The number of elemets to retrieve.</param>
        /// <param name="includeProperties">includeProperties: new []{ $"{nameof(Recipient.DummyInstance.RecipientVouchers)}.{nameof(RecipientVoucher.DummyInstance.Voucher)}"
        /// </param>
        /// <param name="noTrackingChanges">if set to <c>true</c> [no tracking changes].</param>
        /// <returns></returns>
        public virtual async Task<IList<TEntity>> GetCollectionAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            int? numberOfElemetsToRetrieve = null, 
            string[] includeProperties = null,
            bool noTrackingChanges = true)
        {
            IQueryable<TEntity> query = GetProcessedQuery(filter,
                orderBy,
                numberOfElemetsToRetrieve,
                includeProperties,
                noTrackingChanges);

            if (query != null)
            {
                return await query.ToListAsync();
            }

            return new List<TEntity>();
        }

        public virtual TEntity GetById(object id)
        {
            return _entityDbSet.Find(id);
        }

        public virtual Task<TEntity> GetByIdAsync(object id)
        {
            return _entityDbSet.FindAsync(id);
        }

        public virtual void Insert(TEntity entity)
        {
            _entityDbSet.Add(entity);
        }

        public virtual void Delete(TId id)
        {
            TEntity entityToDelete = new TEntity
            {
                Id = id
            };

            Delete(entityToDelete);
        }

        public virtual void Delete(TEntity entityToDelete)
        {
            if (_dbContext.Entry(entityToDelete).State == EntityState.Detached)
            {
                _entityDbSet.Attach(entityToDelete);
            }

            _entityDbSet.Remove(entityToDelete);
        }

        public virtual void Update(TEntity entityToUpdate)
        {
            _entityDbSet.Attach(entityToUpdate);
            _dbContext.Entry(entityToUpdate).State = EntityState.Modified;
        }

        #region Dispose Pattern

        private bool _disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                _dbContext?.Dispose();

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