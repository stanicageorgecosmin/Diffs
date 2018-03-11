using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Entities;

namespace DataAccessLayer.Repositories
{
    public class AppSettingRepo: IDisposable
    {
        private readonly PhotoboothContext _photoboothContext;
        private readonly GenericRepository<PhotoboothAppSetting, int> _genericRepository;

        public AppSettingRepo()
        {
            _photoboothContext = new PhotoboothContext();
            _genericRepository = new GenericRepository<PhotoboothAppSetting, int>(_photoboothContext);
        }

        internal AppSettingRepo(PhotoboothContext photoboothContext)
        {
            _photoboothContext = photoboothContext;
            _genericRepository = new GenericRepository<PhotoboothAppSetting, int>(photoboothContext);
        }

        public async Task<IList<PhotoboothAppSetting>> GetAllAppSettings()
        {
            IList<PhotoboothAppSetting> appSettings = await _genericRepository.GetCollectionAsync();
            return appSettings;
        }

        public async Task<IList<PhotoboothAppSetting>> GetAppSettings(IList<string> settings)
        {
            IList<PhotoboothAppSetting> appSettings =
                await _genericRepository.GetCollectionAsync(setting => settings.Contains(setting.Key));
            return appSettings;
        }

        public async Task<PhotoboothAppSetting> GetAppSettingByKey(string key)
        {
            IList<PhotoboothAppSetting> appSettings =
                await _genericRepository.GetCollectionAsync(setting => setting.Key == key);
            return appSettings?.FirstOrDefault();
        }

        public async Task<int> SaveExistingSettingsAsync(IEnumerable<PhotoboothAppSetting> settingsToBeSaved)
        {
            if (settingsToBeSaved == null)
                return 0;

            foreach (PhotoboothAppSetting settingItem in settingsToBeSaved)
            {
                _genericRepository.Update(settingItem);
            }

            return await _photoboothContext.SaveChangesAsync();
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
