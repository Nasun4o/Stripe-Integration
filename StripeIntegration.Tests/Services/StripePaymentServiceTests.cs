using AutoMapper;
using Entities.DataTransferObjects.Stripe;
using Entities.EntityModels;
using Entities.Utils.Models;
using Interfaces;
using Interfaces.Services;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Services;
using Stripe;
using StripeIntegration.AutoMapper;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StripeIntegration.Tests.Services
{

    [TestClass]
    public class StripePaymentServiceTests : TestBase
    {
        private Mock<IPaymentIntentServiceWrapper> mockPaymentServiceWrapper;
        private Mock<IData> mockData;
        private IOptions<StripeSettings> stripeOptions;
        private IMapper _mapper;
        private PaymentIntentDTO paymentIntentDTO;
        private const string Valid_User_Id = "validUserId";

        [TestInitialize]
        public override void TestInitialize()
        {
            base.TestInitialize();

            PaymentIntent paymentIntent = new PaymentIntent()
            {
                Id = "PI_TestId",
                Amount = 100
            };

            if (_mapper == null)
            {
                var mappingConfig = new MapperConfiguration(mc =>
                {
                    mc.AddProfile(new MappingProfile());
                });
                IMapper mapper = mappingConfig.CreateMapper();
                _mapper = mapper;
            }
            stripeOptions = Options.Create(new StripeSettings());
            mockPaymentServiceWrapper = new Mock<IPaymentIntentServiceWrapper>();

            mockData = new Mock<IData>();
            mockData.Setup(s => s.StripeTransactionHistories.CreateAsync(It.IsAny<StripeTransactionHistory>()));
            mockData.Setup(s => s.StripeTransactionHistories.UpdateAsync(It.IsAny<StripeTransactionHistory>()));
            mockData.Setup(s => s.SaveAsync()).ReturnsAsync(1);

        }

        [TestMethod]
        public async Task When_CreatePaymentIntent_Succesfull_ShouldReturnOk()
        {
            //Arrange
            paymentIntentDTO = new PaymentIntentDTO()
            {
                Id = "PI_TestId",
                Amount = 100,
                Status = "succeeded"
            };

            mockPaymentServiceWrapper.Setup(x => x.CreatePaymentAsync(It.IsAny<PaymentIntentCreateOptions>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync(paymentIntentDTO);

            var service = new StripePaymentService(mockData.Object, _mapper, mockPaymentServiceWrapper.Object, stripeOptions);
            //Act
            var actual = await service.CreatePaymentIntentAsync(100, Valid_User_Id);

            //Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(paymentIntentDTO.Amount, actual.Amount);
            Assert.AreEqual(paymentIntentDTO.Status, actual.Status);
            Assert.AreEqual(paymentIntentDTO.Id, actual.Id);
        }

        [TestMethod]
        public async Task When_CreatePaymentIntent_AmountIsLessThanMinimum_ShouldThrowError()
        {
            //Arrange
            string errorMessage = "The amount of you're transaction is less than the minimum $0.50";
            mockPaymentServiceWrapper.Setup(x => x.CreatePaymentAsync(It.IsAny<PaymentIntentCreateOptions>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>())).ThrowsAsync(new ArgumentException());

            var service = new StripePaymentService(mockData.Object, _mapper, mockPaymentServiceWrapper.Object, stripeOptions);

            //Act, Assert
            ArgumentException ex = await Assert.ThrowsExceptionAsync<ArgumentException>(() => service.CreatePaymentIntentAsync(49, Valid_User_Id));
            Assert.AreEqual(errorMessage, ex.Message);
        }

        [TestMethod]
        public async Task When_CreatePaymentIntent_UserIsNull_ShouldThrowError()
        {
            //Arrange
            string errorMessage = "User cannot be null!";
            mockPaymentServiceWrapper.Setup(x => x.CreatePaymentAsync(It.IsAny<PaymentIntentCreateOptions>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>())).ThrowsAsync(new ArgumentNullException());

            var service = new StripePaymentService(mockData.Object, _mapper, mockPaymentServiceWrapper.Object, stripeOptions);

            ArgumentNullException ex = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => service.CreatePaymentIntentAsync(550, null));
            Assert.AreEqual(errorMessage, ex.Message);
        }

        [TestMethod]
        public async Task When_ConfirmPaymentIntent_Succesfull_ShouldReturnOk()
        {
            //Arrange
            paymentIntentDTO = new PaymentIntentDTO()
            {
                Id = "pi_TestId",
                Amount = 100,
                Status = "succeeded"
            };
            var stripeTransactionHistory = new StripeTransactionHistory()
            {
                Id = "f7d3fa28-7339-457c-b4e4-065f2677ee09",
                PaymentItentId = "pi_TestId",
                Amount = 100,
                Status = "requires_confirmation"
            };

            mockPaymentServiceWrapper.Setup(x => x.ConfirmPaymentAsync(
                It.IsAny<string>(),
                It.IsAny<PaymentIntentConfirmOptions>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(paymentIntentDTO);

            mockData.Setup(s => s.StripeTransactionHistories.FindByIdAsync(It.IsAny<object>())).ReturnsAsync(stripeTransactionHistory);

            var service = new StripePaymentService(mockData.Object, _mapper, mockPaymentServiceWrapper.Object, stripeOptions);

            //Act
            var actual = await service.ConfirmPaymentIntentAsync(stripeTransactionHistory.Id);

            //Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(stripeTransactionHistory.Amount, actual.Amount);
            Assert.AreEqual(stripeTransactionHistory.Status, actual.Status);
            Assert.AreEqual(stripeTransactionHistory.PaymentItentId, actual.Id);
        }

        [TestMethod]
        public async Task When_ConfirmPaymentIntent_AlreadyConfirmed_ShouldThrowException()
        {
            //Arrange
            string errorMessage = "The transaction is already confirmed!";
            paymentIntentDTO = new PaymentIntentDTO()
            {
                Id = "pi_TestId",
                Amount = 100,
                Status = "succeeded"
            };
            var stripeTransactionHistory = new StripeTransactionHistory()
            {
                Id = "f7d3fa28-7339-457c-b4e4-065f2677ee09",
                PaymentItentId = "pi_TestId",
                Amount = 100,
                Status = "succeeded"
            };

            mockPaymentServiceWrapper.Setup(x => x.ConfirmPaymentAsync(
                It.IsAny<string>(),
                It.IsAny<PaymentIntentConfirmOptions>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>())).ThrowsAsync(new ArgumentException());

            mockData.Setup(s => s.StripeTransactionHistories.FindByIdAsync(It.IsAny<object>())).ReturnsAsync(stripeTransactionHistory);

            var service = new StripePaymentService(mockData.Object, _mapper, mockPaymentServiceWrapper.Object, stripeOptions);

            //Act, Assert
            ArgumentException ex = await Assert.ThrowsExceptionAsync<ArgumentException>(() => service.ConfirmPaymentIntentAsync(stripeTransactionHistory.Id));
            Assert.AreEqual(errorMessage, ex.Message);
        }

        [TestMethod]
        [DataRow(null, null)]
        [DataRow("f7d3fa28-7339-457c-b4e4-065f2677ee09", null)]
        [DataRow("f7d3fa28-7339-457c-b4e4-065f2677ee09", "invalid")]
        public async Task When_ConfirmPaymentIntent_IdIsInvalid_ShouldThrowError(string id, string paymentItentId)
        {
            //Arrange
            string[] errorMessage = new string[] { "Invalid PaymentItent Id!", "Invalid Id!" };

            var stripeTransactionHistory = new StripeTransactionHistory()
            {
                Id = "f7d3fa28-7339-457c-b4e4-065f2677ee09",
                PaymentItentId = $"{paymentItentId}",
                Amount = 100,
                Status = "requires_confirmation"
            };

            mockData.Setup(s => s.StripeTransactionHistories.FindByIdAsync(It.IsAny<object>())).ReturnsAsync(stripeTransactionHistory);
            mockPaymentServiceWrapper.Setup(x => x.ConfirmPaymentAsync(
             It.IsAny<string>(),
             It.IsAny<PaymentIntentConfirmOptions>(),
             It.IsAny<RequestOptions>(),
             It.IsAny<CancellationToken>()));

            var service = new StripePaymentService(mockData.Object, _mapper, mockPaymentServiceWrapper.Object, stripeOptions);

            //Act, Assert
            mockPaymentServiceWrapper.Verify(s => s.ConfirmPaymentAsync(It.IsAny<string>(), It.IsAny<PaymentIntentConfirmOptions>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()), Times.Never);
            ArgumentException ex = await Assert.ThrowsExceptionAsync<ArgumentException>(() => service.ConfirmPaymentIntentAsync(id));
            Assert.IsTrue(errorMessage.Any(s => s.Equals(ex.Message)));
        }

        [TestMethod]
        public async Task When_UpdatePaymentIntent_Succesfull_ShouldReturnOk()
        {
            //Arrange
            paymentIntentDTO = new PaymentIntentDTO()
            {
                Id = "pi_TestId",
                Amount = 500,
                Status = "requires_confirmation"
            };
            var stripeTransactionHistory = new StripeTransactionHistory()
            {
                Id = "f7d3fa28-7339-457c-b4e4-065f2677ee09",
                PaymentItentId = "pi_TestId",
                Amount = 100,
                Status = "requires_confirmation"
            };

            mockPaymentServiceWrapper.Setup(x => x.UpdatePaymentAsync(
                It.IsAny<string>(),
                It.IsAny<PaymentIntentUpdateOptions>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(paymentIntentDTO);

            mockData.Setup(s => s.StripeTransactionHistories.FindByIdAsync(It.IsAny<object>())).ReturnsAsync(stripeTransactionHistory);

            var service = new StripePaymentService(mockData.Object, _mapper, mockPaymentServiceWrapper.Object, stripeOptions);

            //Act
            var actual = await service.UpdatePaymentIntentAsync(stripeTransactionHistory.Id, paymentIntentDTO.Amount);

            //Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(paymentIntentDTO.Amount, actual.Amount);
        }

        [TestMethod]
        [DataRow("f7d3fa28-7339-457c-b4e4-065f2677ee09", null, 550)]
        [DataRow("f7d3fa28-7339-457c-b4e4-065f2677ee09", "pi_valid", 49)]
        [DataRow("f7d3fa28-7339-457c-b4e4-065f2677ee09", "invalid", 49)]
        public async Task When_UpdatePaymentIntent_InvalidParameters_ShouldThrowError(string id, string paymentIntentId, long amount)
        {
            //Arrange
            string[] errorMessages = new string[] { "Invalid PaymentIntentId!", "The amount of you're transaction is less than the minimum $0.50" };
       
            var stripeTransactionHistory = new StripeTransactionHistory()
            {
                Id = $"{id}",
                PaymentItentId = $"{paymentIntentId}",
                Amount = 100,
                Status = "requires_confirmation"
            };

            mockData.Setup(s => s.StripeTransactionHistories.FindByIdAsync(It.IsAny<object>())).ReturnsAsync(stripeTransactionHistory);

            mockPaymentServiceWrapper.Setup(x => x.UpdatePaymentAsync(
             It.IsAny<string>(),
             It.IsAny<PaymentIntentUpdateOptions>(),
             It.IsAny<RequestOptions>(),
             It.IsAny<CancellationToken>()));

            var service = new StripePaymentService(mockData.Object, _mapper, mockPaymentServiceWrapper.Object, stripeOptions);

            //Act, Assert
            mockPaymentServiceWrapper.Verify(s => s.UpdatePaymentAsync(It.IsAny<string>(), It.IsAny<PaymentIntentUpdateOptions>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()), Times.Never);
            ArgumentException ex = await Assert.ThrowsExceptionAsync<ArgumentException>(() => service.UpdatePaymentIntentAsync(id, amount));
            Assert.IsTrue(errorMessages.Any(s => s.Equals(ex.Message)));
        }

        [TestMethod]
        public async Task When_CancelPaymentIntent_Succesfull_ShouldReturnOk()
        {
            //Arrange
            var stripeTransactionHistory = new StripeTransactionHistory()
            {
                Id = "f7d3fa28-7339-457c-b4e4-065f2677ee09",
                PaymentItentId = "pi_TestId",
                Amount = 100,
                Status = "requires_confirmation"
            };

            mockPaymentServiceWrapper.Setup(x => x.CancelPaymentAsync(
                It.IsAny<string>(),
                It.IsAny<PaymentIntentCancelOptions>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()));

            mockData.Setup(s => s.StripeTransactionHistories.FindByIdAsync(It.IsAny<object>())).ReturnsAsync(stripeTransactionHistory);

            var service = new StripePaymentService(mockData.Object, _mapper, mockPaymentServiceWrapper.Object, stripeOptions);

            //Act
            await service.CancelPaymentIntentAsync(stripeTransactionHistory.Id);

            //Assert
            mockPaymentServiceWrapper.Verify(s => s.CancelPaymentAsync(It.IsAny<string>(), It.IsAny<PaymentIntentCancelOptions>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            mockData.Verify(s => s.StripeTransactionHistories.DeleteAsync(stripeTransactionHistory), Times.Once);
        }

        [TestMethod]
        [DataRow(null, "pi_valid", "requires_confirmation")]
        [DataRow("f7d3fa28-7339-457c-b4e4-065f2677ee09", null, "")]
        [DataRow("f7d3fa28-7339-457c-b4e4-065f2677ee09", "pi_valid", "succeeded")]
        [DataRow("f7d3fa28-7339-457c-b4e4-065f2677ee09", "invalid", "requires_confirmation")]
        public async Task When_CancelPaymentIntent_InvalidParams_ShouldThrowException(string id, string paymentIntentId, string status)
        {
            //Arrange
            string[] errorMessages = new string[] { "Invalid Id!", "Payment which not exist cannot be canceled!", "Invalid PaymentItent ID!", "The payment is already completed and cannot be canceled!" };

            var stripeTransactionHistory = new StripeTransactionHistory()
            {
                Id = $"{id}",
                PaymentItentId = $"{paymentIntentId}",
                Amount = 100,
                Status = $"{status}"
            };

            mockPaymentServiceWrapper.Setup(x => x.CancelPaymentAsync(
                It.IsAny<string>(),
                It.IsAny<PaymentIntentCancelOptions>(),
                It.IsAny<RequestOptions>(),
                It.IsAny<CancellationToken>()));

            mockData.Setup(s => s.StripeTransactionHistories.FindByIdAsync(It.IsAny<object>())).ReturnsAsync(stripeTransactionHistory);

            var service = new StripePaymentService(mockData.Object, _mapper, mockPaymentServiceWrapper.Object, stripeOptions);

            //Act
            //Assert
            ArgumentException ex = await Assert.ThrowsExceptionAsync<ArgumentException>(() => service.CancelPaymentIntentAsync(id));
            Assert.IsTrue(errorMessages.Any(s => s.Equals(ex.Message)));
            mockPaymentServiceWrapper.Verify(s => s.CancelPaymentAsync(It.IsAny<string>(), It.IsAny<PaymentIntentCancelOptions>(), It.IsAny<RequestOptions>(), It.IsAny<CancellationToken>()), Times.Never);
            mockData.Verify(s => s.StripeTransactionHistories.DeleteAsync(stripeTransactionHistory), Times.Never);
        }
    }
}
