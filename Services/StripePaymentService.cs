using AutoMapper;
using Common;
using Entities.EntityModels;
using Entities.Utils.Models;
using Entities.ViewModels.Transactions;
using Interfaces;
using Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Services
{
    public class StripePaymentService : BaseService, IStripePaymentService
    {
        private readonly IPaymentIntentServiceWrapper _paymentIntentServiceWrapper;
        private readonly StripeSettings stripeSettings;

        public StripePaymentService(
            IData data,
            IMapper mapper,
            IPaymentIntentServiceWrapper paymentIntentServiceWrapper,
            IOptions<StripeSettings> stripeSettingsOptions) : base(data, mapper)
        {
            _paymentIntentServiceWrapper = paymentIntentServiceWrapper;
            stripeSettings = stripeSettingsOptions.Value;
            StripeConfiguration.ApiKey = stripeSettings.SecretKey;
        }

        /// <summary>
        /// Create new paymentIntent object in Stripe and in local database.
        /// We have set only the few parameters, but the PaymentIntentCreateOptions it may content a lot of nested objects and different parameters.
        /// </summary>
        /// <param name="amount">The amount of the transaction</param>
        /// <param name="userId">The current logged in user id.</param>
        /// <returns>The paymentIntent object.</returns>
        public async Task<PaymentIntentViewModel> CreatePaymentIntentAsync(long amount, string userId)
        {
            if (userId is null)
            {
                throw new ArgumentNullException(userId, "User cannot be null!");
            }
            if (amount < Constants.MIN_AMOUNT_OF_TRANSACTION)
            {
                throw new ArgumentException("The amount of you're transaction is less than the minimum $0.50");
            }

            var options = new PaymentIntentCreateOptions()
            {
                Amount = amount,
                Currency = "usd",
                PaymentMethod = "pm_card_visa"
            };

            var paymentIntent = await _paymentIntentServiceWrapper.CreatePaymentAsync(options);

            StripeTransactionHistory stripeHistory = new StripeTransactionHistory()
            {
                Amount = amount,
                UserId = userId,
                PaymentItentId = paymentIntent.Id,
                Status = paymentIntent.Status,
                Created = paymentIntent.Created
            };

            await Data.StripeTransactionHistories.CreateAsync(stripeHistory);
            await this.Data.SaveAsync();

            var result = this.Mapper.Map<PaymentIntentViewModel>(paymentIntent);
            return result;
        }

        /// <summary>
        /// This service will change the status of the paymentIntent to "succeeded". 
        /// It's better to be set to automatic and to be handle in the front-end using the Stripe elements.
        /// "pm_card_visa" is a card for testing purposes(because we don't have front-end where to fill the visa card details).
        /// </summary>
        /// <param name="id">local id in database which holds paymentIntentId</param>
        /// <returns>The updated paymentIntent.</returns>
        public async Task<PaymentIntentViewModel> ConfirmPaymentIntentAsync(string id)
        {
            if (id is null)
            {
                throw new ArgumentException("Invalid Id!");
            }

            var options = new PaymentIntentConfirmOptions
            {
                PaymentMethod = "pm_card_visa",
            };

            StripeTransactionHistory stripeHistory = await Data.StripeTransactionHistories.FindByIdAsync(id);

            if (stripeHistory.Status.Equals("succeeded"))
            {
                throw new ArgumentException("The transaction is already confirmed!");
            }

            if (stripeHistory.PaymentItentId is null || !stripeHistory.PaymentItentId.StartsWith("pi_"))
            {
                throw new ArgumentException("Invalid PaymentItent Id!");
            }

            var paymentIntent = await _paymentIntentServiceWrapper.ConfirmPaymentAsync(stripeHistory.PaymentItentId, options);

            stripeHistory.Status = paymentIntent.Status;

            await Data.StripeTransactionHistories.UpdateAsync(stripeHistory);
            await this.Data.SaveAsync();

            return this.Mapper.Map<PaymentIntentViewModel>(paymentIntent);
        }

        /// <summary>
        /// Update the paymentIntent in both Stripe & local database, depends on the parameters in the PaymentIntentUpdateOptions. 
        /// For the example we change only the Amount of the transaction.
        /// </summary>
        /// <param name="id">local id in database which holds paymentIntentId</param>
        /// <param name="amount">The amount of the transaction.</param>
        /// <returns>The updated paymentIntent.</returns>
        public async Task<PaymentIntentViewModel> UpdatePaymentIntentAsync(string id, long amount)
        {
            if (amount < Constants.MIN_AMOUNT_OF_TRANSACTION)
            {
                throw new ArgumentException("The amount of you're transaction is less than the minimum $0.50");
            }

            var options = new PaymentIntentUpdateOptions
            {
                Amount = amount
            };

            StripeTransactionHistory stripeHistory = await Data.StripeTransactionHistories.FindByIdAsync(id);
            if (stripeHistory.PaymentItentId is null || !stripeHistory.PaymentItentId.StartsWith("pi_"))
            {
                throw new ArgumentException("Invalid PaymentIntentId!");
            }
            var paymentIntent = await _paymentIntentServiceWrapper.UpdatePaymentAsync(stripeHistory.PaymentItentId, options);

            stripeHistory.Amount = amount;

            await Data.StripeTransactionHistories.UpdateAsync(stripeHistory);
            await this.Data.SaveAsync();

            return this.Mapper.Map<PaymentIntentViewModel>(paymentIntent);
        }

        /// <summary>
        /// This service will change the status of the paymentIntent to "Cancel". In order to work the status of the paymentIntent must not be succeeded 
        /// </summary>
        /// <param name="paymentIntentId">id of the paymentIntent</param>
        public async Task CancelPaymentIntentAsync(string id)
        {
            if (id is null)
            {
                throw new ArgumentException("Invalid Id!");
            }

            StripeTransactionHistory stripeHistory = await Data.StripeTransactionHistories.FindByIdAsync(id);

            if (stripeHistory is null)
            {
                throw new ArgumentException("Payment which not exist cannot be canceled!");
            }
            if (stripeHistory.PaymentItentId is null || !stripeHistory.PaymentItentId.StartsWith("pi_"))
            {
                throw new ArgumentException("Invalid PaymentItent ID!");
            }

            else if (!stripeHistory.Status.Equals("requires_confirmation"))
            {
                throw new ArgumentException("The payment is already completed and cannot be canceled!");
            }

            await _paymentIntentServiceWrapper.CancelPaymentAsync(stripeHistory.PaymentItentId);

            await Data.StripeTransactionHistories.DeleteAsync(stripeHistory);
            await this.Data.SaveAsync();
        }

        /// <summary>
        /// Get all paymentIntents of the current logged in user from the database(not from Stripe).
        /// </summary>
        /// <param name="userId">The Id of the logged in User.</param>
        /// <returns>List with transactions.</returns>
        public async Task<List<StripeTransactionHistoryView>> GetMyPaymentsAsync(string userId)
        {
            if (userId is null)
            {
                throw new ArgumentNullException("Invalid User!");
            }

            List<StripeTransactionHistory> payments = await Data.StripeTransactionHistories.Query().Where(s => s.UserId.Equals(userId)).ToListAsync();

            if (payments.Count <= 0)
            {
                throw new ArgumentException("No payments issued!");
            }

            List<StripeTransactionHistoryView> paymentsViewModel = new List<StripeTransactionHistoryView>();

            foreach (var item in payments)
            {
                paymentsViewModel.Add(this.Mapper.Map<StripeTransactionHistoryView>(item));
            }

            return paymentsViewModel;
        }
    }
}
