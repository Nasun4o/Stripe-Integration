using Entities;
using Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess
{
    public class RepositoryBase<T> : IRepositoryBase<T>
        where T : class
    {
        private readonly StripeIntegrationContext _stripeIntegrationContext;
        private readonly DbSet<T> dbSet;
        public RepositoryBase(StripeIntegrationContext stripeIntegrationContext)
        {
            _stripeIntegrationContext = stripeIntegrationContext;
            dbSet = stripeIntegrationContext.Set<T>();
        }

        public async Task CreateAsync(T entity)
        {
            await dbSet.AddAsync(entity);
        }

        public async Task UpdateAsync(T entity)
        {
            await Task.FromResult(dbSet.Update(entity));
        }

        public async Task DeleteAsync(T entity)
        {
            await Task.FromResult(dbSet.Remove(entity));
        }

        public IQueryable<T> Query()
        {
            return dbSet;
        }

        public async Task<int> SaveAsync()
        {
            return await _stripeIntegrationContext.SaveChangesAsync();
        }

        public async Task<T> FindByIdAsync(object id)
        {
            return await dbSet.FindAsync(id);
        }
    }
}
