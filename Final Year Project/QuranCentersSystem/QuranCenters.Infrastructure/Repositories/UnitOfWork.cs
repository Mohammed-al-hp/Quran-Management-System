using QuranCenters.Core.Interfaces;
using QuranCenters.Infrastructure.Data;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace QuranCenters.Infrastructure.Repositories
{
    /// <summary>
    /// تنفيذ وحدة العمل - تدير المعاملات وتضمن تناسق البيانات
    /// تستخدم ConcurrentDictionary لتخزين المستودعات وإعادة استخدامها
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly ConcurrentDictionary<Type, object> _repositories = new();
        private bool _disposed = false;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IRepository<T> Repository<T>() where T : class
        {
            return (IRepository<T>)_repositories.GetOrAdd(typeof(T), _ => new Repository<T>(_context));
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
