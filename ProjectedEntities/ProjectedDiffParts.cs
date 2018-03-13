using System;

namespace ProjectedEntities
{
    public class ProjectedDiffParts : IProjectedEntity<int>
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the key identifier.
        /// </summary>
        /// <value>
        /// The key identifier.
        /// </value>
        public int KeyIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the left part file path.
        /// </summary>
        /// <value>
        /// The left part file path.
        /// </value>
        public string LeftPartFilePath { get; set; }

        /// <summary>
        /// Gets or sets the left part timestamp.
        /// </summary>
        /// <value>
        /// The left part timestamp.
        /// </value>
        public string LeftPartTimestamp { get; set; }

        /// <summary>
        /// Gets the parsed left part timestamp.
        /// </summary>
        /// <value>
        /// The parsed left part timestamp.
        /// </value>
        public DateTime? ParsedLeftPartTimestamp
        {
            get
            {
                if (string.IsNullOrEmpty(LeftPartTimestamp))
                    return null;

                DateTime parsedTimestamp;
                if (DateTime.TryParse(LeftPartTimestamp, out parsedTimestamp))
                    return parsedTimestamp;

                return null;
            }
        }

        /// <summary>
        /// Gets or sets the right part file path.
        /// </summary>
        /// <value>
        /// The right part file path.
        /// </value>
        public string RightPartFilePath { get; set; }

        /// <summary>
        /// Gets or sets the right part timestamp.
        /// </summary>
        /// <value>
        /// The right part timestamp.
        /// </value>
        public string RightPartTimestamp { get; set; }

        /// <summary>
        /// Gets the parsed right part timestamp.
        /// </summary>
        /// <value>
        /// The parsed right part timestamp.
        /// </value>
        public DateTime? ParsedRightPartTimestamp
        {
            get
            {
                if (string.IsNullOrEmpty(RightPartTimestamp))
                    return null;

                DateTime parsedTimestamp;
                if (DateTime.TryParse(RightPartTimestamp, out parsedTimestamp))
                    return parsedTimestamp;

                return null;
            }
        }

        /// <summary>
        /// Gets or sets the calculated diffs metadata.
        /// </summary>
        /// <value>
        /// The calculated diffs metadata.
        /// </value>
        public string CalculatedDiffsMetadata { get; set; }

        /// <summary>
        /// Gets or sets the calculated diffs metadata timestamp.
        /// </summary>
        /// <value>
        /// The calculated diffs metadata timestamp.
        /// </value>
        public string CalculatedDiffsMetadataTimestamp { get; set; }

        /// <summary>
        /// Gets the parsed calculated diffs metadata timestamp.
        /// </summary>
        /// <value>
        /// The parsed calculated diffs metadata timestamp.
        /// </value>
        public DateTime? ParsedCalculatedDiffsMetadataTimestamp
        {
            get
            {
                if (string.IsNullOrEmpty(CalculatedDiffsMetadataTimestamp))
                    return null;

                DateTime parsedTimestamp;
                if (DateTime.TryParse(CalculatedDiffsMetadataTimestamp, out parsedTimestamp))
                    return parsedTimestamp;

                return null;
            }
        }

        /// <summary>
        /// Gets or sets the is left part updating.
        /// </summary>
        /// <value>
        /// The is left part updating.
        /// </value>
        public string IsLeftPartUpdating { get; set; }

        /// <summary>
        /// Gets the parsed is left part updating.
        /// </summary>
        /// <value>
        /// The parsed is left part updating.
        /// </value>
        public bool? ParsedIsLeftPartUpdating
        {
            get
            {
                if (string.IsNullOrEmpty(IsLeftPartUpdating))
                    return null;

                bool parsedBoolean;
                if (bool.TryParse(IsLeftPartUpdating.Trim().ToLower(), out parsedBoolean))
                    return parsedBoolean;

                return null;
            }
        }

        /// <summary>
        /// Gets or sets the is right part updating.
        /// </summary>
        /// <value>
        /// The is right part updating.
        /// </value>
        public string IsRightPartUpdating { get; set; }

        /// <summary>
        /// Gets the parsed is right part updating.
        /// </summary>
        /// <value>
        /// The parsed is right part updating.
        /// </value>
        public bool? ParsedIsRightPartUpdating
        {
            get
            {
                if (string.IsNullOrEmpty(IsRightPartUpdating))
                    return null;

                bool parsedBoolean;
                if (bool.TryParse(IsRightPartUpdating.Trim().ToLower(), out parsedBoolean))
                    return parsedBoolean;

                return null;
            }
        }
    }
}
