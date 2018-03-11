using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;

namespace DataAccessLayer.Repositories
{
    public class VoucherRepo: IDisposable
    {
        private readonly PhotoboothContext _photoboothContext;
        private readonly GenericRepository<Voucher, int> _genericRepository;

        public VoucherRepo()
        {
            _photoboothContext = new PhotoboothContext();
            _genericRepository = new GenericRepository<Voucher, int>(_photoboothContext);
        }

        internal VoucherRepo(PhotoboothContext photoboothContext)
        {
            _photoboothContext = photoboothContext;
            _genericRepository = new GenericRepository<Voucher, int>(photoboothContext);
        }

        public async Task<Voucher> GetFirstAvailableVoucherAsync()
        {
            // select top 1 v.* from Voucher v
            // left join RecipientVoucher rv
            // on v.Id = rv.VoucherId
            // where rv.Id = null

            IQueryable<Voucher> query = (from v in _photoboothContext.Vouchers
                from rv in v.RecipientVouchers.DefaultIfEmpty()
                where rv == null
                select v).Take(1);

            Voucher voucher = await query.FirstAsync();
            return voucher;
        }

        #region Dispose Pattern

        private bool _disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _photoboothContext?.Dispose();
                _genericRepository?.Dispose();
            }

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
