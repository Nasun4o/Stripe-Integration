using Entities.ViewModels.Transactions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Interfaces.Services
{
    public interface IStripePaymentService
    {
        Task<PaymentIntentViewModel> CreatePaymentIntentAsync(long amount, string userId);
        Task<PaymentIntentViewModel> ConfirmPaymentIntentAsync(string paymentIntentId);
        Task<PaymentIntentViewModel> UpdatePaymentIntentAsync(string paymentIntentId, long amount);
        Task CancelPaymentIntentAsync(string paymentIntentId);
        Task<List<StripeTransactionHistoryView>> GetMyPaymentsAsync(string userId);
    }
}
