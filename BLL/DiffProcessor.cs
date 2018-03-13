using DAL.Repositories;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusinessSupport;
using DAL.Interfaces;
using Newtonsoft.Json;
using ProjectedEntities;

namespace BLL
{
    public class DiffProcessor : IDisposable
    {
        private const int MaxNumberOfCharsToBeProcessedAtOnce = 10; // TODO: it should be configurable
        private const int MaxNumberOfProducerConsumerItems = 1000; // TODO: it should be configurable

        private readonly IDiffPartsRepo _diffPartRepo;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiffProcessor"/> class.
        /// </summary>
        public DiffProcessor()
        {
            _diffPartRepo = new DiffPartsRepo();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiffProcessor"/> class.
        /// </summary>
        /// <param name="diffPartRepo">The difference part repo.</param>
        public DiffProcessor(IDiffPartsRepo diffPartRepo)
        {
            _diffPartRepo = diffPartRepo;
        }

        /// <summary>
        /// Tries to get diffs for key identifier asynchronous.
        /// </summary>
        /// <param name="keyIdentifier">The key identifier.</param>
        /// <returns></returns>
        public async Task<CalculatedDiffsFeedback> TryGetDiffsForKeyIdentifierAsync(int keyIdentifier)
        {
            try
            {
                var projectedEntity = await _diffPartRepo.GetDiffResultsFieldsForKeyIdentifier(keyIdentifier);

                if (IsCachedCalculatedDiffsValid(projectedEntity))
                {
                    // return cached value
                    string cachedCalculatedDiff = projectedEntity.CalculatedDiffsMetadata;
                    var calculatedDiffs = JsonConvert.DeserializeObject<CalculatedDiffsFeedback>(cachedCalculatedDiff);
                    return calculatedDiffs;
                }

                var calculatedDiffsFeedback = await CalculateDiffsAsync(projectedEntity);

                if (calculatedDiffsFeedback != null)
                {
                    string serializedData = JsonConvert.SerializeObject(calculatedDiffsFeedback);

                    await _diffPartRepo.SaveSerializedCalculatedDiffsForKeyIdentifierAsync(keyIdentifier,
                        serializedData);

                    return calculatedDiffsFeedback;
                }
            }
            catch (Exception ex)
            {
                return new CalculatedDiffsFeedback
                {
                    OperationSucceeded = false,
                    ErrorMessage = ex.ToString()
                };
            }

            return new CalculatedDiffsFeedback()
            {
                OperationSucceeded = false,
                ErrorMessage = "Unknown error occurred"
            };
        }

        #region Helper Methods

        /// <summary>
        /// Calculates the diffs asynchronous.
        /// </summary>
        /// <param name="projectedEntity">The projected entity.</param>
        /// <returns></returns>
        private async Task<CalculatedDiffsFeedback> CalculateDiffsAsync(ProjectedDiffParts projectedEntity)
        {
            CheckValidationForCalculatedDiffsParamter(projectedEntity);

            var leftFileInfo = new FileInfo(projectedEntity.LeftPartFilePath);
            leftFileInfo.Refresh();
            long leftPartLength = leftFileInfo.Length;

            var rightFileInfo = new FileInfo(projectedEntity.RightPartFilePath);
            rightFileInfo.Refresh();
            long rightFileLength = rightFileInfo.Length;

            if (leftPartLength != rightFileLength)
            {
                return new CalculatedDiffsFeedback
                {
                    OperationSucceeded = true,
                    LeftPartSize = leftPartLength.ToString(),
                    RightPartSize = rightFileLength.ToString(),
                    PartsAreEqual = false
                };
            }

            CalculatedDiffsFeedback calculatedDiffs =
                await CalculateDiffsForEqualSizeFilesContent(projectedEntity.LeftPartFilePath,
                    projectedEntity.RightPartFilePath, rightFileLength);

            return calculatedDiffs;
        }

        /// <summary>
        /// Calculates the content of the diffs for equal size files.
        /// </summary>
        /// <param name="leftPartFilePath">The left part file path.</param>
        /// <param name="rightPartFilePath">The right part file path.</param>
        /// <param name="sizeOfContentToBeProcessed">The size of content to be processed.</param>
        /// <returns></returns>
        private async Task<CalculatedDiffsFeedback> CalculateDiffsForEqualSizeFilesContent(string leftPartFilePath,
            string rightPartFilePath,
            long sizeOfContentToBeProcessed)
        {

            using (var blockingCollection = ProduceBufferBlocksCollection(leftPartFilePath, rightPartFilePath))
            {
                var differencesCountsByOffset =
                    await ProcessSegmentsOfDataByConsumingFromrCollectionAsync(blockingCollection);

                var normalizedDiffDetails = GetNormalizedDiffDetails(differencesCountsByOffset);
                CalculatedDiffsFeedback feedback = new CalculatedDiffsFeedback
                {
                    OperationSucceeded = true,
                    LeftPartSize = sizeOfContentToBeProcessed.ToString(),
                    RightPartSize = sizeOfContentToBeProcessed.ToString(),
                };

                if (normalizedDiffDetails != null && normalizedDiffDetails.Count > 0)
                {
                    feedback.PartsAreEqual = false;
                    feedback.DiffsMetadata = normalizedDiffDetails;
                }
                else
                {
                    feedback.PartsAreEqual = true;
                }

                return feedback;
            }
        }

        /// <summary>
        /// Processes the segments of data by consuming fromr collection asynchronous.
        /// </summary>
        /// <param name="blockingCollection">The blocking collection.</param>
        /// <returns></returns>
        private async Task<SortedDictionary<long, long>> ProcessSegmentsOfDataByConsumingFromrCollectionAsync(BlockingCollection<BufferBlock> blockingCollection)
        {
            var differencesCountsByOffset = new SortedDictionary<long, long>();
            var lockingSortedDictionary = new object();

            List<Task> listOfRunningTasks = new List<Task>();
            foreach (var producedItem in blockingCollection.GetConsumingEnumerable())
            {
                var localBufferBlock = producedItem;
                var task = Task.Run(() =>
                {
                    try
                    {
                        var listFoundDiffs = FindDiffsInSegment(localBufferBlock);
                        if (listFoundDiffs != null && listFoundDiffs.Count > 0)
                        {
                            lock (lockingSortedDictionary)
                            {
                                foreach (var foundDiffItem in listFoundDiffs)
                                {
                                    differencesCountsByOffset.Add(foundDiffItem.Offset, foundDiffItem.Length);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //TODO: log exception
                        // silent fail
                    }
                });

                listOfRunningTasks.Add(task);
            }

            await Task.WhenAll(listOfRunningTasks);

            return differencesCountsByOffset;
        }

        private BlockingCollection<BufferBlock> ProduceBufferBlocksCollection(string leftPartFilePath,
            string rightPartFilePath)
        {
            var blockingCollection = new BlockingCollection<BufferBlock>(MaxNumberOfProducerConsumerItems);
            Task.Run(async () =>
            {
                try
                {
                    using (var leftReader = new StreamReader(leftPartFilePath))
                    using (var rightReader = new StreamReader(rightPartFilePath))
                    {
                        int index = 0;
                        while (!leftReader.EndOfStream && !rightReader.EndOfStream)
                        {
                            var leftPartBuffer = new char[MaxNumberOfCharsToBeProcessedAtOnce];
                            var rightPartBuffer = new char[MaxNumberOfCharsToBeProcessedAtOnce];
                            int leftNumberOfChars =
                                await leftReader.ReadBlockAsync(leftPartBuffer, 0, MaxNumberOfCharsToBeProcessedAtOnce);

                            int rightNumberOfChars = await rightReader.ReadBlockAsync(rightPartBuffer, 0,
                                MaxNumberOfCharsToBeProcessedAtOnce);

                            blockingCollection.Add(new BufferBlock
                            {
                                OffsetInFile = index,
                                NumberOfReadChars = leftNumberOfChars,
                                LeftBuffer = leftPartBuffer,
                                RightBuffer = rightPartBuffer
                            });

                            index += MaxNumberOfCharsToBeProcessedAtOnce;
                        }
                    }
                }
                catch (Exception ex)
                {
                    //TODO: log exception if any
                    // silent fail
                }
                finally
                {
                    blockingCollection.CompleteAdding();
                }
            });

            return blockingCollection;
        }

        /// <summary>
        /// Gets the normalized difference details.
        /// </summary>
        /// <param name="differencesCountsByOffset">The differences counts by offset.</param>
        /// <returns></returns>
        private List<DiffsMetadata> GetNormalizedDiffDetails(SortedDictionary<long, long> differencesCountsByOffset)
        {
            if (differencesCountsByOffset == null || differencesCountsByOffset.Count == 0)
                return null;

            var normalizedDiffsList = new List<DiffsMetadata>();
            long previousOffset = -1, previousLength = -1;
            foreach (var kvpItem in differencesCountsByOffset)
            {
                var currentOffset = kvpItem.Key;
                var currentLength = kvpItem.Value;

                if (previousOffset >= 0)
                {
                    long endDiffIndex = previousOffset + previousLength;
                    if (endDiffIndex == currentOffset)
                    {
                        normalizedDiffsList[normalizedDiffsList.Count - 1].Length =
                            (previousLength + currentLength).ToString();

                        continue;
                    }
                }

                normalizedDiffsList.Add(new DiffsMetadata
                {
                    Offset = currentOffset.ToString(),
                    Length = currentLength.ToString()
                });

                previousOffset = currentOffset;
                previousLength = currentLength;
            }
            return normalizedDiffsList;
        }

        /// <summary>
        /// Finds the diffs in segment.
        /// </summary>
        /// <param name="producedItem">The produced item.</param>
        /// <returns></returns>
        private List<ProcessedDiffDetails> FindDiffsInSegment(BufferBlock producedItem)
        {
            if (producedItem == null)
                return null;

            int offset = producedItem.OffsetInFile;
            var leftSegment = producedItem.LeftBuffer;
            var rightSegment = producedItem.RightBuffer;
            int numberOfChars = producedItem.NumberOfReadChars;

            List<ProcessedDiffDetails> listProcessedDiffDetails = new List<ProcessedDiffDetails>();
            bool alreadyStartedProcessingDifferencesSegment = false;
            for (int index = 0; index < numberOfChars && index < leftSegment.Length; index++)
            {
                if (leftSegment[index] == rightSegment[index])
                {
                    alreadyStartedProcessingDifferencesSegment = false;
                }
                else if (alreadyStartedProcessingDifferencesSegment)
                {
                    listProcessedDiffDetails[listProcessedDiffDetails.Count - 1].Length++;
                }
                else
                {
                    var diffMetadata = new ProcessedDiffDetails
                    {
                        Offset = offset + index,
                        Length = 1
                    };

                    listProcessedDiffDetails.Add(diffMetadata);
                    alreadyStartedProcessingDifferencesSegment = true;
                }
            }

            return listProcessedDiffDetails;
        }

        /// <summary>
        /// Checks the validation for calculated diffs paramter.
        /// </summary>
        /// <param name="projectedEntity">The projected entity.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException"></exception>
        private void CheckValidationForCalculatedDiffsParamter(ProjectedDiffParts projectedEntity)
        {
            if (projectedEntity == null)
                throw new ArgumentNullException($"'{nameof(projectedEntity)}' parameter is null");

            if (string.IsNullOrEmpty(projectedEntity.LeftPartFilePath) ||
                string.IsNullOrEmpty(projectedEntity.RightPartFilePath))
                throw new ArgumentException($"'{nameof(projectedEntity)}' parameter has invalid properties");

            if (!File.Exists(projectedEntity.LeftPartFilePath))
                throw new ArgumentException($"File {projectedEntity.LeftPartFilePath} could not be found");

            if (!File.Exists(projectedEntity.RightPartFilePath))
                throw new ArgumentException($"File {projectedEntity.RightPartFilePath} could not be found");
        }

        /// <summary>
        /// Determines whether [is cached calculated diffs valid] [the specified projected entity].
        /// </summary>
        /// <param name="projectedEntity">The projected entity.</param>
        /// <returns>
        ///   <c>true</c> if [is cached calculated diffs valid] [the specified projected entity]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsCachedCalculatedDiffsValid(ProjectedDiffParts projectedEntity)
        {
            return
                !string.IsNullOrWhiteSpace(projectedEntity?.CalculatedDiffsMetadata) &&
                projectedEntity.ParsedCalculatedDiffsMetadataTimestamp.HasValue &&
                projectedEntity.ParsedLeftPartTimestamp.HasValue && projectedEntity.ParsedRightPartTimestamp.HasValue &&
                projectedEntity.ParsedCalculatedDiffsMetadataTimestamp.Value >
                projectedEntity.ParsedLeftPartTimestamp.Value &&
                projectedEntity.ParsedCalculatedDiffsMetadataTimestamp.Value >
                projectedEntity.ParsedRightPartTimestamp.Value;
        }

        #endregion

        #region Helper Types

        private class BufferBlock
        {
            /// <summary>
            /// Gets or sets the offset in file.
            /// </summary>
            /// <value>
            /// The offset in file.
            /// </value>
            public int OffsetInFile { get; set; }

            /// <summary>
            /// Gets or sets the number of read chars.
            /// </summary>
            /// <value>
            /// The number of read chars.
            /// </value>
            public int NumberOfReadChars { get; set; }

            /// <summary>
            /// Gets or sets the left buffer.
            /// </summary>
            /// <value>
            /// The left buffer.
            /// </value>
            public char[] LeftBuffer { get; set; }

            /// <summary>
            /// Gets or sets the right buffer.
            /// </summary>
            /// <value>
            /// The right buffer.
            /// </value>
            public char[] RightBuffer { get; set; }
        }

        private class ProcessedDiffDetails
        {
            /// <summary>
            /// Gets or sets the offset.
            /// </summary>
            /// <value>
            /// The offset.
            /// </value>
            public int Offset { get; set; }

            /// <summary>
            /// Gets or sets the length.
            /// </summary>
            /// <value>
            /// The length.
            /// </value>
            public int Length { get; set; }
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