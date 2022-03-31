using AutoMapper;
using Entities.DataTransferObjects.Stripe;
using Entities.EntityModels;
using Entities.ViewModels.Transactions;
using Stripe;

namespace StripeIntegration.AutoMapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<StripeTransactionHistory, StripeTransactionHistoryView>();
            CreateMap<PaymentIntent, PaymentIntentDTO>();
            CreateMap<PaymentIntentDTO, PaymentIntentViewModel>();
        }
    }
}
