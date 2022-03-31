using Entities.Utils.Models;
using Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using System.IO;
using System.Threading.Tasks;

namespace StripeIntegration.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StripeController : BaseController
    {
        private readonly IStripePaymentService _stripePaymentService;

        //Get from Stripe CLI
        private readonly StripeSettings stripeSettings;
        private string _webHookCliKey;

        public StripeController(IStripePaymentService stripePaymentService, IOptions<StripeSettings> stripeSettingsOptions)
        {
            _stripePaymentService = stripePaymentService;
            stripeSettings = stripeSettingsOptions.Value;
            _webHookCliKey = stripeSettings.WebHookKey;
        }


        [HttpPost("payment-intent-create")]
        public async Task<IActionResult> CreatePaymentIntent(long amount)
        {
            return this.Ok(await this._stripePaymentService.CreatePaymentIntentAsync(amount, UserId));
        }

        [HttpPost("payment-intent-confirm")]
        public async Task<IActionResult> ConfirmPaymentIntent(string id) => this.Ok(await _stripePaymentService.ConfirmPaymentIntentAsync(id));

        [HttpPut("payment-intent-update")]
        public async Task<IActionResult> UpdatePaymentIntent(string id, long amount) => this.Ok(await _stripePaymentService.UpdatePaymentIntentAsync(id, amount));

        [HttpDelete("payment-intent-cancel")]
        public async Task<IActionResult> CancelPaymentIntent(string id)
        {
            await _stripePaymentService.CancelPaymentIntentAsync(id);

            return NoContent();
        }

        [HttpGet("my-payments")]
        public async Task<IActionResult> GetMyPayments()
        {
            var paymentIntentDtos = await _stripePaymentService.GetMyPaymentsAsync(UserId);
            return Ok(paymentIntentDtos);
        }

        /// <summary>
        /// This endpoint is used to listen for changes from Stripe, it's best practice to set some stuffs from here once the desired event is triggered.
        /// For example payment_Intent, it's better to update our database from here instead of doing in another endpoint (Confirm etc.)
        /// In order to start the webhook, you need to create Webhook key in Stripe and start Stripe CLI by stripe listen --forward-to https://localhost:44336/webhook
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("webhook")]
        public async Task<ActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], _webHookCliKey);

            switch (stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                    break;
                case "payment_intent.payment_failed":
                    break;
            }
            return new EmptyResult();
        }
    }
}
