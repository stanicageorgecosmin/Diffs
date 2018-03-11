using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    internal interface IDiffDataContext : IDisposable
    {
        Task<int> SaveChangesAsync();
    }
}
