using Entities.EntityModels;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IData
    {
        Task<int> SaveAsync();
        IRepositoryBase<ApplicationUser> ApplicationUser { get; }
        IRepositoryBase<StripeTransactionHistory> StripeTransactionHistories { get; }
    }
}
