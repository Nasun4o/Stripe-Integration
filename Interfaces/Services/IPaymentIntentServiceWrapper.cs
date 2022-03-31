using Entities.DataTransferObjects.Stripe;
using Stripe;
using System.Threading;
using System.Threading.Tasks;

namespace Interfaces.Services
{
    public interface IPaymentIntentServiceWrapper
    {
        Task<PaymentIntentDTO> CreatePaymentAsync(PaymentIntentCreateOptions options, RequestOptions requestOptions = null, CancellationToken cancellationToken = default(CancellationToken));
        Task<PaymentIntentDTO> ConfirmPaymentAsync(string id, PaymentIntentConfirmOptions options = null, RequestOptions requestOptions = null, CancellationToken cancellationToken = default(CancellationToken));
        Task<PaymentIntentDTO> UpdatePaymentAsync(string id, PaymentIntentUpdateOptions options, RequestOptions requestOptions = null, CancellationToken cancellationToken = default(CancellationToken));
        Task<PaymentIntentDTO> CancelPaymentAsync(string id, PaymentIntentCancelOptions options = null, RequestOptions requestOptions = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}
