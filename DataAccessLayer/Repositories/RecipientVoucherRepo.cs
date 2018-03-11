using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;

namespace DataAccessLayer.Repositories
{
    public class RecipientVoucherRepo: IDisposable
    {
        private readonly PhotoboothContext _photoboothContext;
        private readonly GenericRepository<RecipientVoucher, int> _genericRepository;

        public RecipientVoucherRepo()
        {
            _photoboothContext = new PhotoboothContext();
            _genericRepository = new GenericRepository<RecipientVoucher, int>(_photoboothContext);
        }

        internal RecipientVoucherRepo(PhotoboothContext photoboothContext)
        {
            _photoboothContext = photoboothContext;
            _genericRepository = new GenericRepository<RecipientVoucher, int>(photoboothContext);
        }

        public async Task<RecipientVoucher> GetLastVoucherForRecipientAsync(string emailAddress)
        {
            if (string.IsNullOrEmpty(emailAddress))
                return null;

            IList<RecipientVoucher> recipientVouchersList =
                await _genericRepository.GetCollectionAsync(
                    recipientVoucher => emailAddress.Equals(recipientVoucher.Recipient.EmailAddress, StringComparison.OrdinalIgnoreCase),
                    query => query.OrderByDescending(recipientVoucher => recipientVoucher.TimeStampUtc),
                    includeProperties: new[] {DatatablesConstants.Recipient, DatatablesConstants.Voucher});

            if (recipientVouchersList != null && recipientVouchersList.Count > 0)
                return recipientVouchersList[0];

            return null;
        }

        public void Insert(RecipientVoucher entity)
        {
            _genericRepository.Insert(entity);
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
