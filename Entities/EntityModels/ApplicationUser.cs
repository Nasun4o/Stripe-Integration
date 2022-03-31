using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Entities.EntityModels
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<StripeTransactionHistory> StripeTransactionHistories { get; set; } = new HashSet<StripeTransactionHistory>();
    }
}
