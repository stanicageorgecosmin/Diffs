using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer.Repositories;
using Entities;

namespace DataAccessLayer.UnitsOfWork
{
    public class RecipientVoucherUsageUoW : IDisposable
    {
        private readonly RecipientRepo _recipientRepository;

        private readonly RecipientVoucherRepo _recipientVoucherRepository;

        private readonly VoucherRepo _voucherRepository;

        private readonly PhotoboothContext _photoboothContext;

        public RecipientVoucherUsageUoW()
        {
            _photoboothContext = new PhotoboothContext();
            _recipientVoucherRepository = new RecipientVoucherRepo(_photoboothContext);
            _voucherRepository = new VoucherRepo(_photoboothContext);
            _recipientRepository = new RecipientRepo(_photoboothContext);
        }

        public async Task<string> AddNewVoucherForRecipientAsync(Recipient recipient, RecipientVoucher recipientVoucherUsage)
        {
            if (recipient == null || recipientVoucherUsage == null)
                return null;

            Voucher voucher = await _voucherRepository.GetFirstAvailableVoucherAsync();

            if (voucher != null)
            {
                recipientVoucherUsage.Recipient = recipient;
                recipientVoucherUsage.Voucher = voucher;

                //Recipient test =await _recipientRepository.GetRecipientByEmailAddressAsync("aaa@aaa.com");
                _recipientRepository.Insert(recipient);
                _recipientVoucherRepository.Insert(recipientVoucherUsage);

                await _photoboothContext.SaveChangesAsync();

                return voucher.Code;
            }

            return null;
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
                _voucherRepository?.Dispose();
                _recipientRepository?.Dispose();
                _recipientVoucherRepository?.Dispose();
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
