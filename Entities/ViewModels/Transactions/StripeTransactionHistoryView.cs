using System;

namespace Entities.ViewModels.Transactions
{
    public class StripeTransactionHistoryView
    {
        public string Id { get; set; }
        public string PaymentItentId { get; set; }
        public string Status { get; set; }
        public long Amount { get; set; }
        public DateTime Created { get; set; }
    }
}
