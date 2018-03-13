using DAL.Repositories;
using System;
using System.IO;
using System.Threading.Tasks;
using DAL.Interfaces;
using Diffs.Constants;
using BusinessSupport;

namespace BLL
{
    public class DataSetterProcessor: IDisposable
    {
        private readonly IDiffPartsRepo _diffPartRepo;
        private readonly DiffPartEnum _diffPartEnum;

        public DataSetterProcessor(DiffPartEnum diffPartEnum)
        {
            _diffPartRepo = new DiffPartsRepo();
            _diffPartEnum = diffPartEnum;
        }

        public DataSetterProcessor(IDiffPartsRepo diffPartRepo, DiffPartEnum diffPartEnum)
        {
            _diffPartRepo = diffPartRepo;
            _diffPartEnum = diffPartEnum;
        }

        /// <summary>
        /// Tries the set data for key identifier asynchronous.
        /// </summary>
        /// <param name="keyIdentifier">The key identifier.</param>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public async Task<Feedback> TrySetDataForKeyIdentifierAsync(int keyIdentifier, Stream stream)
        {
            try
            {
                bool? isDataPartSettable = await IsDataPartSettableForKeyIdentifierAsync(keyIdentifier);

                if (!isDataPartSettable.HasValue)
                {
                    await InsertDataForKeyIdentifierAsync(keyIdentifier, stream);
                }
                else if (isDataPartSettable.Value)
                {
                    await UpdateDataForKeyIdentifierAsync(keyIdentifier, stream);
                }
                else
                {
                    return new Feedback
                    {
                        OperationSucceeded = false,
                        ErrorMessage = MessagesConstants.IsCurrentlyBusy
                    };

                }
            }
            catch (Exception ex)
            {
                return new Feedback
                {
                    OperationSucceeded = false,
                    ErrorMessage = ex.ToString()
                };
            }

            return new Feedback
            {
                OperationSucceeded = true
            };
        }

        #region Helper Methods

        /// <summary>
        /// Determines whether [is data part settable for key identifier asynchronous] [the specified key identifier].
        /// </summary>
        /// <param name="keyIdentifier">The key identifier.</param>
        /// <returns></returns>
        private async Task<bool?> IsDataPartSettableForKeyIdentifierAsync(int keyIdentifier)
        {
            bool? isPartCurrentlyUpdating = null;
            switch (_diffPartEnum)
            {
                case DiffPartEnum.Left:
                    isPartCurrentlyUpdating = await _diffPartRepo.IsLeftPartCurrentlyUpdatingAsync(keyIdentifier);
                    break;

                case DiffPartEnum.Right:
                    isPartCurrentlyUpdating = await _diffPartRepo.IsRightPartCurrentlyUpdatingAsync(keyIdentifier);
                    break;
            }

            return !isPartCurrentlyUpdating;
        }

        /// <summary>
        /// Inserts the data for key identifier asynchronous.
        /// </summary>
        /// <param name="keyIdentifier">The key identifier.</param>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">stream</exception>
        private async Task InsertDataForKeyIdentifierAsync(int keyIdentifier, Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException($"'{nameof(stream)}' parameter must not be null");

            switch (_diffPartEnum)
            {
                case DiffPartEnum.Left:
                    await _diffPartRepo.SaveLeftStreamForKeyIdentifierAsync(keyIdentifier, stream,
                        StreamSavingTypeEnum.InsertNew);
                    break;

                case DiffPartEnum.Right:
                default:
                    await _diffPartRepo.SaveRightStreamForKeyIdentifierAsync(keyIdentifier, stream,
                        StreamSavingTypeEnum.InsertNew);
                    break;
            }
           
        }

        /// <summary>
        /// Updates the data for key identifier asynchronous.
        /// </summary>
        /// <param name="keyIdentifier">The key identifier.</param>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">stream</exception>
        private async Task UpdateDataForKeyIdentifierAsync(int keyIdentifier, Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException($"'{nameof(stream)}' parameter must not be null");

            switch (_diffPartEnum)
            {
                case DiffPartEnum.Left:
                    await _diffPartRepo.SaveLeftStreamForKeyIdentifierAsync(keyIdentifier, stream,
                        StreamSavingTypeEnum.UpdateExisting);
                    break;

                case DiffPartEnum.Right:
                default:
                    await _diffPartRepo.SaveRightStreamForKeyIdentifierAsync(keyIdentifier, stream,
                        StreamSavingTypeEnum.UpdateExisting);
                    break;
            }
        }

        #endregion

        #region Dispose Pattern

        private bool _disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _diffPartRepo?.Dispose();
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
