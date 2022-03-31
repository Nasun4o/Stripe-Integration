using Entities.Utils.Models;
using Entities.ViewModels.Transactions;
using Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Stripe;
using StripeIntegration.Controllers;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StripeIntegration.Tests.Controllers
{
    [TestClass]
    public class StripeControllerTests : TestBase
    {
        private Mock<IStripePaymentService> mockStripePaymentService;
        private readonly Mock<HttpContext> mockHttpContext = new Mock<HttpContext>();
        private PaymentIntentViewModel paymentViewModel;
        private IOptions<StripeSettings> stripeOptions;

        [TestInitialize]
        public override void TestInitialize()
        {
            base.TestInitialize();
            StripeConfiguration.ApiKey = "sk_test_ZUDBxcTWr5hZ9K1SuNHu16Td00BRjVsZZ7";

            mockHttpContext.Setup(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                                        new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                                        new Claim(ClaimTypes.Name, "test@user.com"),
                                        new Claim(ClaimTypes.Email, "test@user.com")
                                   }, "TestAuthentication")));
            stripeOptions = Options.Create(new StripeSettings());
            mockStripePaymentService = new Mock<IStripePaymentService>();
        }

        [TestMethod]
        public void When_CreatePaymentSuccesfull_ShouldReturnOk()
        {
            //Arrange
            paymentViewModel = new PaymentIntentViewModel()
            {
                Amount = 900,
                ClientSecret = "valid secret",
                Id = "PI_testId123",
            };

            mockStripePaymentService.Setup(x => x.CreatePaymentIntentAsync(It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(paymentViewModel);

            var controller = new StripeController(mockStripePaymentService.Object, stripeOptions);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = mockHttpContext.Object;

            //Act
            var actionResult = controller.CreatePaymentIntent(900);
            var result = actionResult.Result as OkObjectResult;
            var actual = result.Value as PaymentIntentViewModel;

            //Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            Assert.AreEqual(paymentViewModel.Amount, actual.Amount);
        }

        [TestMethod]
        public async Task When_CreatePaymentUserIsNull_ShouldThrowExecption()
        {
            //Arrange
            var controller = new StripeController(mockStripePaymentService.Object, stripeOptions);

            //Act
            var actionResult = controller.CreatePaymentIntent(900);

            //Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => actionResult);
        }

        [TestMethod]
        public async Task When_CreatePaymentAmountIsLessThanMinimum_ShouldThrowExecption()
        {
            //Arrange
            long amount = 49;
            mockStripePaymentService.Setup(x => x.CreatePaymentIntentAsync(amount, It.IsAny<string>())).ThrowsAsync(new ArgumentException());

            var controller = new StripeController(mockStripePaymentService.Object, stripeOptions);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = mockHttpContext.Object;

            //Act
            var actionResult = controller.CreatePaymentIntent(amount);

            //Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => actionResult);
        }


        [TestMethod]
        public void When_ConfirmPaymentSuccesfull_ShouldReturnOk()
        {
            //Arrange
            paymentViewModel = new PaymentIntentViewModel()
            {
                Amount = 900,
                ClientSecret = "valid secret",
                Id = "pi_testId123",
                Status = "succeeded"
            };

            mockStripePaymentService.Setup(x => x.ConfirmPaymentIntentAsync(It.IsAny<string>())).ReturnsAsync(paymentViewModel);

            var controller = new StripeController(mockStripePaymentService.Object, stripeOptions);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = mockHttpContext.Object;

            //Act
            var actionResult = controller.ConfirmPaymentIntent(paymentViewModel.Id);
            var result = actionResult.Result as OkObjectResult;
            var actual = result.Value as PaymentIntentViewModel;

            //Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            Assert.AreEqual(paymentViewModel.Status, actual.Status);
        }


        [TestMethod]
        public async Task When_ConfirmPaymentInvalidId_ShouldThrowExecption()
        {
            //Arrange
            string invalidId = "notStartWithpi_";
            mockStripePaymentService.Setup(x => x.ConfirmPaymentIntentAsync(invalidId)).ThrowsAsync(new ArgumentException());

            var controller = new StripeController(mockStripePaymentService.Object, stripeOptions);

            //Act
            var actionResult = controller.ConfirmPaymentIntent(invalidId);

            //Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => actionResult);
        }


        [TestMethod]
        public void When_UpdatePaymentIsSuccesfull_ShouldReturnOk()
        {
            //Arrange
            paymentViewModel = new PaymentIntentViewModel()
            {
                Amount = 900,
                ClientSecret = "valid secret",
                Id = "pi_testId123",
                Status = "requires_confirmation"
            };

            mockStripePaymentService.Setup(x => x.UpdatePaymentIntentAsync(It.IsAny<string>(), It.IsAny<long>())).ReturnsAsync(paymentViewModel);

            var controller = new StripeController(mockStripePaymentService.Object, stripeOptions);

            //Act
            var actionResult = controller.UpdatePaymentIntent(paymentViewModel.Id, paymentViewModel.Amount);
            var result = actionResult.Result as OkObjectResult;
            var actual = result.Value as PaymentIntentViewModel;

            //Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            Assert.AreEqual(paymentViewModel.Amount, actual.Amount);
        }

        [TestMethod]
        public async Task When_UpdatePaymentInvalidAmount_ShouldThrowExecption()
        {
            //Arrange
            long invalidAmount = 49;

            mockStripePaymentService.Setup(x => x.UpdatePaymentIntentAsync(It.IsAny<string>(), invalidAmount)).ThrowsAsync(new ArgumentException());

            var controller = new StripeController(mockStripePaymentService.Object, stripeOptions);

            //Act
            var actionResult = controller.UpdatePaymentIntent(It.IsAny<string>(), invalidAmount);

            //Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => actionResult);
        }

        [TestMethod]
        public async Task When_UpdatePaymentInvalidId_ShouldThrowExecption()
        {
            //Arrange
            string invalidId = "testId123";

            mockStripePaymentService.Setup(x => x.UpdatePaymentIntentAsync(invalidId, It.IsAny<long>())).ThrowsAsync(new ArgumentException());

            var controller = new StripeController(mockStripePaymentService.Object, stripeOptions);

            //Act
            var actionResult = controller.UpdatePaymentIntent(invalidId, It.IsAny<long>());

            //Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => actionResult);
        }

        [TestMethod]
        public void When_CancelPaymentSuccesfull_ShouldReturnOk()
        {
            //Arrange
            string validId = "pi_testId123";

            mockStripePaymentService.Setup(x => x.CancelPaymentIntentAsync(It.IsAny<string>()));

            var controller = new StripeController(mockStripePaymentService.Object, stripeOptions);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = mockHttpContext.Object;

            //Act
            var actionResult = controller.CancelPaymentIntent(validId);
            var result = actionResult.Result as NoContentResult;

            //Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task When_CancelPaymentInvalidId_ShouldThrowExecption()
        {
            //Arrange
            string invalidId = "notvalid";
            mockStripePaymentService.Setup(x => x.CancelPaymentIntentAsync(invalidId)).ThrowsAsync(new ArgumentException());

            var controller = new StripeController(mockStripePaymentService.Object, stripeOptions);

            //Act
            var actionResult = controller.CancelPaymentIntent(invalidId);

            //Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => actionResult);
        }

        [TestMethod]
        public void When_GetMyPaymentsSuccesfull_ShouldRetunOk()
        {
            //Arrange
            List<StripeTransactionHistoryView> stripeTransactionHistoryViews = new List<StripeTransactionHistoryView>()
            {
                new StripeTransactionHistoryView()
                {
                     Amount = 1000,
                     PaymentItentId = "pi_1",
                     Status = "requires_confirmation"
                },
                new StripeTransactionHistoryView()
                {
                     Amount = 5430,
                     PaymentItentId = "pi_2",
                     Status = "succeeded"
                }
            };

            mockStripePaymentService.Setup(x => x.GetMyPaymentsAsync(It.IsAny<string>())).ReturnsAsync(stripeTransactionHistoryViews);

            var controller = new StripeController(mockStripePaymentService.Object, stripeOptions);
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = mockHttpContext.Object;

            //Act
            var actionResult = controller.GetMyPayments();
            var result = actionResult.Result as OkObjectResult;
            var actual = result.Value as List<StripeTransactionHistoryView>;

            //Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            Assert.AreEqual(stripeTransactionHistoryViews.Count, actual.Count);
            Assert.IsTrue(stripeTransactionHistoryViews.Count > 0);
        }

        [TestMethod]
        public async Task When_GetMyPaymentsUserIsNull_ShouldThrowException()
        {
            //Arrange
            mockStripePaymentService.Setup(x => x.GetMyPaymentsAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());

            var controller = new StripeController(mockStripePaymentService.Object, stripeOptions);

            //Act
            var actionResult = controller.GetMyPayments();

            //Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => actionResult);
        }
    }
}
