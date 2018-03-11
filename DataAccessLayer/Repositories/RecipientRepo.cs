using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;

namespace DataAccessLayer.Repositories
{
    public class RecipientRepo: IDisposable
    {
        private readonly PhotoboothContext _photoboothContext;
        private readonly GenericRepository<Recipient, int> _genericRepository;

        public RecipientRepo()
        {
            _photoboothContext = new PhotoboothContext();
            _genericRepository = new GenericRepository<Recipient, int>(_photoboothContext);
        }

        internal RecipientRepo(PhotoboothContext photoboothContext)
        {
            _photoboothContext = photoboothContext;
            _genericRepository = new GenericRepository<Recipient, int>(photoboothContext);
        }

        public async Task<Recipient> GetRecipientByEmailAddressAsync(string emailAddress)
        {
            IList<Recipient> recipientsEnum =
                await _genericRepository.GetCollectionAsync(recipient => recipient.EmailAddress == emailAddress
                    //,includeProperties: new []{ $"{nameof(Recipient.DummyInstance.RecipientVouchers)}.{nameof(RecipientVoucher.DummyInstance.Voucher)}" }
                );
            return recipientsEnum?.FirstOrDefault();
        }

        public void Insert(Recipient entity)
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
                _genericRepository?.Dispose();

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
