using System.Linq;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IRepositoryBase<T>
    {
        IQueryable<T> Query();
        Task CreateAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<T> FindByIdAsync(object id);
        Task<int> SaveAsync();
    }
}
