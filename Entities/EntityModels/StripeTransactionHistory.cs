using System;

namespace Entities.EntityModels
{
    public class StripeTransactionHistory
    {
        public string Id { get; set; }
        public string PaymentItentId { get; set; }
        public string Status { get; set; }
        public long Amount { get; set; }
        public DateTime Created { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
