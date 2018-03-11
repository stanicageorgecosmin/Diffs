using System;
using System.Data.Entity;
using System.Diagnostics;
using DAL.Entities;
using DAL.Interfaces;

namespace DAL
{
    public class DiffDataContext : DbContext, IDisposable
    {
        public DiffDataContext() : base("DiffDataContext")
        {
#if DEBUG
            Database.Log = s => Debug.Write(s);
#endif
        }

        public DbSet<DiffPartsEntity> DiffParts { get; set; }
    }
}
