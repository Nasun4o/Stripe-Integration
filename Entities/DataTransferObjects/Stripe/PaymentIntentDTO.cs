using System;

namespace Entities.DataTransferObjects.Stripe
{
    public class PaymentIntentDTO
    {
        public string Id { get; set; }

        public string ClientSecret { get; set; }

        public string Status { get; set; }

        public long Amount { get; set; }

        public DateTime Created { get; set; }
    }
}
