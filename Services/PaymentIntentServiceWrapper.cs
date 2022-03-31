using AutoMapper;
using Entities.DataTransferObjects.Stripe;
using Interfaces.Services;
using Stripe;
using System.Threading;
using System.Threading.Tasks;

namespace Services
{
    public class PaymentIntentServiceWrapper : BaseService, IPaymentIntentServiceWrapper
    {
        private readonly PaymentIntentService _paymentIntentService;

        public PaymentIntentServiceWrapper(PaymentIntentService paymentIntentServicer)
        {
            _paymentIntentService = paymentIntentServicer;
        }

        public async Task<PaymentIntentDTO> CreatePaymentAsync(PaymentIntentCreateOptions options, RequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            var result = await _paymentIntentService.CreateAsync(options, requestOptions, cancellationToken);
            PaymentIntentDTO paymentIntentDTO = new PaymentIntentDTO()
            {
                Id = result.Id,
                Status = result.Status,
                Amount = result.Amount,
                ClientSecret = result.ClientSecret,
                Created = result.Created,
            };

            return paymentIntentDTO;
        }

        public async Task<PaymentIntentDTO> ConfirmPaymentAsync(string id, PaymentIntentConfirmOptions options = null, RequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            var result = await _paymentIntentService.ConfirmAsync(id, options, requestOptions, cancellationToken);
            PaymentIntentDTO paymentIntentDTO = new PaymentIntentDTO()
            {
                Id = result.Id,
                Status = result.Status,
                Amount = result.Amount,
                ClientSecret = result.ClientSecret,
                Created = result.Created,
            };

            return paymentIntentDTO;
        }

        public async Task<PaymentIntentDTO> UpdatePaymentAsync(string id, PaymentIntentUpdateOptions options, RequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            var result = await _paymentIntentService.UpdateAsync(id, options, requestOptions, cancellationToken);
            PaymentIntentDTO paymentIntentDTO = new PaymentIntentDTO()
            {
                Id = result.Id,
                Status = result.Status,
                Amount = result.Amount,
                ClientSecret = result.ClientSecret,
                Created = result.Created,
            };

            return paymentIntentDTO;
        }

        public async Task<PaymentIntentDTO> CancelPaymentAsync(string id, PaymentIntentCancelOptions options = null, RequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            var result = await _paymentIntentService.CancelAsync(id, options, requestOptions, cancellationToken);
            PaymentIntentDTO paymentIntentDTO = new PaymentIntentDTO()
            {
                Id = result.Id,
                Status = result.Status,
                Amount = result.Amount,
                ClientSecret = result.ClientSecret,
                Created = result.Created,
            };

            return paymentIntentDTO;
        }
    }
}
