using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using DAL.Constants;
using DAL.Interfaces;

namespace DAL.Entities
{
    [Table(DataTablesNamesConstants.DiffParts)]
    public class DiffPartsEntity : IEntity<int>
    {
        [Key]
        public int Id { get; set; }

        public int KeyIdentifier { get; set; }

        public string LeftPartFilePath { get; set; }

        public string LeftPartTimestamp { get; set; }

        public string RightPartFilePath { get; set; }

        public string RightPartTimestamp { get; set; }

        public string CalculatedDiffsMetadata { get; set; }

        public string CalculatedDiffsMetadataTimestamp { get; set; }

        public string IsLeftPartUpdating { get; set; }

        public string IsRightPartUpdating { get; set; }
    }
}