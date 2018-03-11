using System;
using System.IO;
using System.Threading.Tasks;
using BusinessSupport;
using ProjectedEntities;

namespace DAL.Interfaces
{
    public interface IDiffPartsRepo: IDisposable
    {
        /// <summary>
        /// Determines whether [is left part currently updating asynchronous] [the specified idkey identifier].
        /// </summary>
        /// <param name="idkeyIdentifier">The idkey identifier.</param>
        /// <returns></returns>
        Task<bool?> IsLeftPartCurrentlyUpdatingAsync(int idkeyIdentifier);

        /// <summary>
        /// Determines whether [is right part currently updating asynchronous] [the specified idkey identifier].
        /// </summary>
        /// <param name="idkeyIdentifier">The idkey identifier.</param>
        /// <returns></returns>
        Task<bool?> IsRightPartCurrentlyUpdatingAsync(int idkeyIdentifier);

        /// <summary>
        /// Saves the left stream for key identifier asynchronous.
        /// </summary>
        /// <param name="idkeyIdentifier">The idkey identifier.</param>
        /// <param name="stream">The stream.</param>
        /// <param name="streamSavingType">Type of the stream saving.</param>
        /// <returns></returns>
        Task SaveLeftStreamForKeyIdentifierAsync(int idkeyIdentifier, Stream stream, StreamSavingTypeEnum streamSavingType);

        /// <summary>
        /// Saves the right stream for key identifier asynchronous.
        /// </summary>
        /// <param name="idkeyIdentifier">The idkey identifier.</param>
        /// <param name="stream">The stream.</param>
        /// <param name="streamSavingType">Type of the stream saving.</param>
        /// <returns></returns>
        Task SaveRightStreamForKeyIdentifierAsync(int idkeyIdentifier, Stream stream, StreamSavingTypeEnum streamSavingType);

        /// <summary>
        /// Gets the difference results fields for key identifier.
        /// </summary>
        /// <param name="idkeyIdentifier">The idkey identifier.</param>
        /// <returns></returns>
        Task<ProjectedDiffParts> GetDiffResultsFieldsForKeyIdentifier(int idkeyIdentifier);

        /// <summary>
        /// Saves the serialized calculated diffs for key identifier asynchronous.
        /// </summary>
        /// <param name="keyIdentifier">The key identifier.</param>
        /// <param name="calculatedDiffs">The calculated diffs.</param>
        /// <returns></returns>
        Task SaveSerializedCalculatedDiffsForKeyIdentifierAsync(int keyIdentifier, string calculatedDiffs);
    }
}
