using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
using System.Data.Entity.Infrastructure;
using System.Data.SQLite;

namespace DataAccessLayer
{
    //TODO: DI
    //public interface IPhotoboothContext: IDisposable
    //{
    //    DbSet<PhotoboothAppSetting> PhotoboothAppSettings { get; set; }

    //    DbSet<Voucher> Vouchers { get; set; }

    //    DbSet<Recipient> Recipients { get; set; }

    //    DbSet<RecipientVoucher> RecipientVouchers { get; set; }

    //    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    //    DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
    //}

    internal class PhotoboothContext : DbContext//, IPhotoboothContext
    {

        public PhotoboothContext(): base("PhotoboothContext")
        {
            
        }

        /// <summary>
        /// Gets or sets the photobooth application settings.
        /// </summary>
        /// <value>
        /// The photobooth application settings.
        /// </value>
        public DbSet<PhotoboothAppSetting> PhotoboothAppSettings { get; set; }

        /// <summary>
        /// Gets or sets the vouchers.
        /// </summary>
        /// <value>
        /// The vouchers.
        /// </value>
        public DbSet<Voucher> Vouchers { get; set; }

        /// <summary>
        /// Gets or sets the recipients.
        /// </summary>
        /// <value>
        /// The recipients.
        /// </value>
        public DbSet<Recipient> Recipients { get; set; }

        /// <summary>
        /// Gets or sets the recipient vouchers.
        /// </summary>
        /// <value>
        /// The recipient vouchers.
        /// </value>
        public DbSet<RecipientVoucher> RecipientVouchers { get; set; }
    }
}
