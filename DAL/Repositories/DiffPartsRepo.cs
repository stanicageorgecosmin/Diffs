using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using BusinessSupport;
using DAL.Entities;
using DAL.Interfaces;
using ProjectedEntities;

namespace DAL.Repositories
{
    public class DiffPartsRepo : IDiffPartsRepo, IDisposable
    {
        private readonly IGenericRepository<DiffPartsEntity, ProjectedDiffParts, int> _genericRepository;

        private string _rootFolder = "DiffsStorage"; // TODO: it should be configurable

        private string _storageRootFolder = null;
        /// <summary>
        /// Gets the storage root folder.
        /// </summary>
        /// <value>
        /// The storage root folder.
        /// </value>
        private string StorageRootFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_storageRootFolder))
                {
                    _storageRootFolder = Path.Combine("C:\\", _rootFolder);
                }

                return _storageRootFolder;
            }
        }

        public DiffPartsRepo()
        {
            if (!Directory.Exists(StorageRootFolder))
            {
                Directory.CreateDirectory(StorageRootFolder);
            }

            _genericRepository = new GenericRepository<DiffPartsEntity, ProjectedDiffParts, int>(new DiffDataContext());
        }

        public DiffPartsRepo(IGenericRepository<DiffPartsEntity, ProjectedDiffParts, int> genericRepository)
        {
            _genericRepository = genericRepository;
        }

        #region Public Interface

        /// <summary>
        /// Determines whether [is left part currently updating asynchronous] [the specified key identifier].
        /// </summary>
        /// <param name="keyIdentifier">The key identifier.</param>
        /// <returns></returns>
        public async Task<bool?> IsLeftPartCurrentlyUpdatingAsync(int keyIdentifier)
        {
            var projectedEntities = await _genericRepository.GetProjectedEntitiesCollectionAsync(
                ent => new ProjectedDiffParts
                {
                    IsLeftPartUpdating = ent.IsLeftPartUpdating
                },
                ent => ent.KeyIdentifier == keyIdentifier);

            if (projectedEntities == null || projectedEntities.Count == 0)
                return null;

            return projectedEntities[0].ParsedIsLeftPartUpdating ?? false;
        }

        /// <summary>
        /// Determines whether [is right part currently updating asynchronous] [the specified key identifier].
        /// </summary>
        /// <param name="keyIdentifier">The key identifier.</param>
        /// <returns></returns>
        public async Task<bool?> IsRightPartCurrentlyUpdatingAsync(int keyIdentifier)
        {
            var projectedEntities = await _genericRepository.GetProjectedEntitiesCollectionAsync(
                ent => new ProjectedDiffParts
                {
                    IsRightPartUpdating = ent.IsRightPartUpdating
                },
                ent => ent.KeyIdentifier == keyIdentifier);

            if (projectedEntities == null || projectedEntities.Count == 0)
                return null;

            return projectedEntities[0].ParsedIsRightPartUpdating ?? false;
        }

        /// <summary>
        /// Saves the left stream for key identifier asynchronous.
        /// </summary>
        /// <param name="keyIdentifier">The key identifier.</param>
        /// <param name="stream">The stream.</param>
        /// <param name="streamSavingType">Type of the stream saving.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">stream</exception>
        public async Task SaveLeftStreamForKeyIdentifierAsync(int keyIdentifier, Stream stream, StreamSavingTypeEnum streamSavingType)
        {
            if (stream == null)
                throw new ArgumentNullException($"'{nameof(stream)}' parameter must not be null");

            switch (streamSavingType)
            {
                case StreamSavingTypeEnum.InsertNew:
                    await InsertLeftStreamForKeyIdentifierAsync(keyIdentifier, stream);
                    break;
                case StreamSavingTypeEnum.UpdateExisting:
                default:
                    await UpdateLeftStreamForKeyIdentifierAsync(keyIdentifier, stream);
                    break;
            }
        }

        /// <summary>
        /// Saves the right stream for key identifier asynchronous.
        /// </summary>
        /// <param name="keyIdentifier">The key identifier.</param>
        /// <param name="stream">The stream.</param>
        /// <param name="streamSavingType">Type of the stream saving.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">stream</exception>
        public async Task SaveRightStreamForKeyIdentifierAsync(int keyIdentifier, Stream stream, StreamSavingTypeEnum streamSavingType)
        {
            if (stream == null)
                throw new ArgumentNullException($"'{nameof(stream)}' parameter must not be null");

            switch (streamSavingType)
            {
                case StreamSavingTypeEnum.InsertNew:
                    await InsertRightStreamForKeyIdentifierAsync(keyIdentifier, stream);
                    break;
                case StreamSavingTypeEnum.UpdateExisting:
                default:
                    await UpdateRightStreamForKeyIdentifierAsync(keyIdentifier, stream);
                    break;
            }
        }

        /// <summary>
        /// Saves the serialized calculated diffs for key identifier asynchronous.
        /// </summary>
        /// <param name="keyIdentifier">The key identifier.</param>
        /// <param name="calculatedDiffs">The calculated diffs.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">calculatedDiffs</exception>
        /// <exception cref="System.ApplicationException">keyIdentifier</exception>
        public async Task SaveSerializedCalculatedDiffsForKeyIdentifierAsync(int keyIdentifier, string calculatedDiffs)
        {
            if (calculatedDiffs == null)
                throw new ArgumentNullException($"'{nameof(calculatedDiffs)}' parameter must not be null");

            int? id = await GetIdForKeyIdentifier(keyIdentifier);
            if (id == null)
                throw new ApplicationException(
                    $"Could not find the {nameof(keyIdentifier)} = {keyIdentifier} in database");

            var diffPartsEntity = new DiffPartsEntity
            {
                Id = id.Value,
                CalculatedDiffsMetadata = calculatedDiffs,
                CalculatedDiffsMetadataTimestamp = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)
            };

            _genericRepository.UpdateFieldsInEntity(diffPartsEntity,
                new List<string>
                {
                    $"{nameof(diffPartsEntity.CalculatedDiffsMetadata)}",
                    $"{nameof(diffPartsEntity.CalculatedDiffsMetadataTimestamp)}"
                });

            await _genericRepository.DbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Gets the difference results fields for key identifier.
        /// </summary>
        /// <param name="idkeyIdentifier">The idkey identifier.</param>
        /// <returns></returns>
        public async Task<ProjectedDiffParts> GetDiffResultsFieldsForKeyIdentifier(int idkeyIdentifier)
        {
            var projectedEntitiesCol = await _genericRepository.GetProjectedEntitiesCollectionAsync(
               ent => new ProjectedDiffParts
               {
                   Id = ent.Id,
                   KeyIdentifier = ent.KeyIdentifier,
                   CalculatedDiffsMetadata = ent.CalculatedDiffsMetadata,
                   CalculatedDiffsMetadataTimestamp = ent.CalculatedDiffsMetadataTimestamp,
                   LeftPartTimestamp = ent.LeftPartTimestamp,
                   RightPartTimestamp = ent.RightPartTimestamp,
                   LeftPartFilePath = ent.LeftPartFilePath,
                   RightPartFilePath = ent.RightPartFilePath
               },
               ent => ent.KeyIdentifier == idkeyIdentifier
           );

            if (projectedEntitiesCol == null || projectedEntitiesCol.Count == 0)
                return null;

            return projectedEntitiesCol[0];
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets the file path for part.
        /// </summary>
        /// <param name="diffPart">The difference part.</param>
        /// <param name="keyIdentifier">The key identifier.</param>
        /// <param name="createFolderIfNotExists">if set to <c>true</c> [create folder if not exists].</param>
        /// <returns></returns>
        private string GetFilePathForPart(DiffPartEnum diffPart, int keyIdentifier, bool createFolderIfNotExists = true)
        {
            string fileName = null;
            switch (diffPart)
            {
                case DiffPartEnum.Left:
                    fileName = "left";
                    break;
                case DiffPartEnum.Right:
                default:
                    fileName = "right";
                    break;
            }

            string baseFolder = Path.Combine(StorageRootFolder, keyIdentifier.ToString());
            if (createFolderIfNotExists && !Directory.Exists(baseFolder))
                Directory.CreateDirectory(baseFolder);

            return Path.Combine(baseFolder, fileName);
        }

        /// <summary>
        /// Saves the file.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        private async Task SaveFile(Stream stream, string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                await stream.CopyToAsync(fileStream);
            }

            //if (filePath.Contains("left"))
            //    await Task.Delay(30000);
        }

        /// <summary>
        /// Inserts the left stream for key identifier asynchronous.
        /// </summary>
        /// <param name="keyIdentifier">The key identifier.</param>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        private async Task InsertLeftStreamForKeyIdentifierAsync(int keyIdentifier, Stream stream)
        {
            string leftPartFilePath = GetFilePathForPart(DiffPartEnum.Left, keyIdentifier);

            #region Set Db part

            var diffPartsEnt = new DiffPartsEntity
            {
                KeyIdentifier = keyIdentifier,
                LeftPartFilePath = leftPartFilePath,
                IsLeftPartUpdating = true.ToString(),
                LeftPartTimestamp = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)
            };

            _genericRepository.Insert(diffPartsEnt);

            await _genericRepository.DbContext.SaveChangesAsync();

            #endregion

            await SaveFile(stream, leftPartFilePath);

            #region Set Db part

            diffPartsEnt.IsLeftPartUpdating = false.ToString();

            _genericRepository.UpdateFieldsInEntity(diffPartsEnt,
                new List<string> { $"{nameof(diffPartsEnt.IsLeftPartUpdating)}" });

            await _genericRepository.DbContext.SaveChangesAsync();

            #endregion
        }

        /// <summary>
        /// Updates the left stream for key identifier asynchronous.
        /// </summary>
        /// <param name="keyIdentifier">The key identifier.</param>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        /// <exception cref="System.ApplicationException">keyIdentifier</exception>
        private async Task UpdateLeftStreamForKeyIdentifierAsync(int keyIdentifier, Stream stream)
        {
            int? id = await GetIdForKeyIdentifier(keyIdentifier);
            if (id == null)
                throw new ApplicationException(
                    $"Could not find the {nameof(keyIdentifier)} = {keyIdentifier} in database");

            string leftPartFilePath = GetFilePathForPart(DiffPartEnum.Left, keyIdentifier);

            #region Set Db part

            var diffPartsEntity = new DiffPartsEntity
            {
                Id = id.Value,
                LeftPartFilePath = leftPartFilePath,
                IsLeftPartUpdating = true.ToString(),
                LeftPartTimestamp = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)
            };

            _genericRepository.UpdateFieldsInEntity(diffPartsEntity,
                new List<string>
                {
                    $"{nameof(diffPartsEntity.LeftPartFilePath)}",
                    $"{nameof(diffPartsEntity.IsLeftPartUpdating)}",
                    $"{nameof(diffPartsEntity.LeftPartTimestamp)}"
                });

            await _genericRepository.DbContext.SaveChangesAsync();

            #endregion

            await SaveFile(stream, leftPartFilePath);

            #region Set Db part

            diffPartsEntity.IsLeftPartUpdating = false.ToString();

            _genericRepository.UpdateFieldsInEntity(diffPartsEntity,
                new List<string>
                {
                    $"{nameof(diffPartsEntity.IsLeftPartUpdating)}"
                });

            await _genericRepository.DbContext.SaveChangesAsync();

            #endregion
        }

        /// <summary>
        /// Inserts the right stream for key identifier asynchronous.
        /// </summary>
        /// <param name="keyIdentifier">The key identifier.</param>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        private async Task InsertRightStreamForKeyIdentifierAsync(int keyIdentifier, Stream stream)
        {
            string rightPartFilePath = GetFilePathForPart(DiffPartEnum.Right, keyIdentifier);

            #region Set Db part

            var diffPartsEnt = new DiffPartsEntity
            {
                KeyIdentifier = keyIdentifier,
                RightPartFilePath = rightPartFilePath,
                IsRightPartUpdating = true.ToString(),
                RightPartTimestamp = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)
            };

            _genericRepository.Insert(diffPartsEnt);

            await _genericRepository.DbContext.SaveChangesAsync();

            #endregion

            await SaveFile(stream, rightPartFilePath);

            #region Set Db part

            diffPartsEnt.IsRightPartUpdating = false.ToString();

            _genericRepository.UpdateFieldsInEntity(diffPartsEnt,
                new List<string> { $"{nameof(diffPartsEnt.IsRightPartUpdating)}" });

            await _genericRepository.DbContext.SaveChangesAsync();

            #endregion
        }

        /// <summary>
        /// Updates the right stream for key identifier asynchronous.
        /// </summary>
        /// <param name="keyIdentifier">The key identifier.</param>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        /// <exception cref="System.ApplicationException">keyIdentifier</exception>
        private async Task UpdateRightStreamForKeyIdentifierAsync(int keyIdentifier, Stream stream)
        {
            int? id = await GetIdForKeyIdentifier(keyIdentifier);
            if (id == null)
                throw new ApplicationException(
                    $"Could not find the {nameof(keyIdentifier)} = {keyIdentifier} in database");

            string rightPartFilePath = GetFilePathForPart(DiffPartEnum.Right, keyIdentifier);

            #region Set Db part

            var diffPartsEntity = new DiffPartsEntity
            {
                Id = id.Value,
                RightPartFilePath = rightPartFilePath,
                IsRightPartUpdating = true.ToString(),
                RightPartTimestamp = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)
            };

            _genericRepository.UpdateFieldsInEntity(diffPartsEntity,
                new List<string>
                {
                    $"{nameof(diffPartsEntity.RightPartFilePath)}",
                    $"{nameof(diffPartsEntity.IsRightPartUpdating)}",
                    $"{nameof(diffPartsEntity.RightPartTimestamp)}"
                });

            await _genericRepository.DbContext.SaveChangesAsync();

            #endregion

            await SaveFile(stream, rightPartFilePath);

            #region Set Db part

            diffPartsEntity.IsRightPartUpdating = false.ToString();

            _genericRepository.UpdateFieldsInEntity(diffPartsEntity,
                new List<string>
                {
                    $"{nameof(diffPartsEntity.IsRightPartUpdating)}"
                });

            await _genericRepository.DbContext.SaveChangesAsync();

            #endregion
        }

        /// <summary>
        /// Gets the identifier for key identifier.
        /// </summary>
        /// <param name="keyIdentifier">The key identifier.</param>
        /// <returns></returns>
        private async Task<int?> GetIdForKeyIdentifier(int keyIdentifier)
        {
            var projectedEntities = await _genericRepository.GetProjectedEntitiesCollectionAsync(
                ent => new ProjectedDiffParts
                {
                    Id = ent.Id
                },
                ent => ent.KeyIdentifier == keyIdentifier);

            if (projectedEntities == null || projectedEntities.Count == 0)
                return null;

            return projectedEntities[0].Id;
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
                _genericRepository.DbContext?.Dispose();
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
