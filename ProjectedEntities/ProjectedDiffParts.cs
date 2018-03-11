using System;

namespace ProjectedEntities
{
    public class ProjectedDiffParts : IProjectedEntity<int>
    {
        public int Id { get; set; }

        public int KeyIdentifier { get; set; }

        public string LeftPartFilePath { get; set; }

        public string LeftPartTimestamp { get; set; }

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

        public string RightPartFilePath { get; set; }

        public string RightPartTimestamp { get; set; }

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

        public string CalculatedDiffsMetadata { get; set; }

        public string CalculatedDiffsMetadataTimestamp { get; set; }

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

        public string IsLeftPartUpdating { get; set; }

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

        public string IsRightPartUpdating { get; set; }

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
