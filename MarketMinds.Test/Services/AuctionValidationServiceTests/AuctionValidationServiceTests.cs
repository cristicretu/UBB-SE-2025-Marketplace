using System;
using Moq;
using NUnit.Framework;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.AuctionValidationService;
using MarketMinds.Shared.Services.AuctionProductsService;

namespace MarketMinds.Test.Services.AuctionValidationServiceTests
{
    [TestFixture]
    public class AuctionValidationServiceTests
    {
        private Mock<IAuctionProductsService> _mockAuctionProductsService;
        private AuctionValidationService _auctionValidationService;

        [SetUp]
        public void Setup()
        {
            _mockAuctionProductsService = new Mock<IAuctionProductsService>();
            _auctionValidationService = new AuctionValidationService(_mockAuctionProductsService.Object);
        }

        [Test]
        public void ValidateAndPlaceBid_ThrowsArgumentNullException_WhenProductIsNull()
        {
            // Arrange
            User validUser = new User();
            string validBid = "100";

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                _auctionValidationService.ValidateAndPlaceBid(null, validUser, validBid));

            Assert.That(exception.Message, Is.EqualTo("Auction product cannot be null (Parameter 'product')"));
        }

        [Test]
        public void ValidateAndPlaceBid_ThrowsArgumentNullException_WhenBidderIsNull()
        {
            // Arrange
            AuctionProduct validProduct = new AuctionProduct();
            string validBid = "100";

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                _auctionValidationService.ValidateAndPlaceBid(validProduct, null, validBid));

            Assert.That(exception.Message, Is.EqualTo("Bidder cannot be null (Parameter 'bidder')"));
        }

        [Test]
        public void ValidateAndPlaceBid_ThrowsArgumentException_WhenBidIsEmpty()
        {
            // Arrange
            AuctionProduct validProduct = new AuctionProduct();
            User validUser = new User();
            string emptyBid = "  ";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _auctionValidationService.ValidateAndPlaceBid(validProduct, validUser, emptyBid));

            Assert.That(exception.Message, Is.EqualTo("Bid amount is required"));
        }

        [Test]
        public void ValidateAndPlaceBid_ThrowsArgumentException_WhenBidIsNotANumber()
        {
            // Arrange
            AuctionProduct validProduct = new AuctionProduct();
            User validUser = new User();
            string invalidBidText = "ten dollars";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _auctionValidationService.ValidateAndPlaceBid(validProduct, validUser, invalidBidText));

            Assert.That(exception.Message, Is.EqualTo($"Invalid bid format: '{invalidBidText}' is not a valid number"));
        }

        [Test]
        public void ValidateAndPlaceBid_ThrowsArgumentException_WhenBidIsNegative()
        {
            // Arrange
            AuctionProduct validProduct = new AuctionProduct();
            User validUser = new User();
            string negativeBid = "-5";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _auctionValidationService.ValidateAndPlaceBid(validProduct, validUser, negativeBid));

            Assert.That(exception.Message, Is.EqualTo("Bid amount must be positive"));
        }

        [Test]
        public void ValidateAndPlaceBid_CallsPlaceBid_WhenAllInputsAreValid()
        {
            // Arrange
            AuctionProduct validProduct = new AuctionProduct();
            User validUser = new User();
            string validBidText = "150";
            double expectedBidAmount = 150;

            // Act
            _auctionValidationService.ValidateAndPlaceBid(validProduct, validUser, validBidText);

            // Assert
            _mockAuctionProductsService.Verify(
                x => x.PlaceBid(validProduct, validUser, expectedBidAmount),
                Times.Once);
        }

        [Test]
        public void ValidateAndPlaceBid_ThrowsWrappedException_WhenPlaceBidFails()
        {
            // Arrange
            AuctionProduct validProduct = new AuctionProduct();
            User validUser = new User();
            string validBidText = "200";
            string expectedInnerMessage = "Simulated failure";

            _mockAuctionProductsService
                .Setup(x => x.PlaceBid(It.IsAny<AuctionProduct>(), It.IsAny<User>(), It.IsAny<double>()))
                .Throws(new Exception(expectedInnerMessage));

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                _auctionValidationService.ValidateAndPlaceBid(validProduct, validUser, validBidText));

            Assert.That(exception.Message, Does.StartWith("Bid failed:"));
            Assert.That(exception.InnerException?.Message, Does.Contain(expectedInnerMessage));
        }

        [Test]
        public void ValidateAndConcludeAuction_ThrowsArgumentException_WhenProductIsNull()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _auctionValidationService.ValidateAndConcludeAuction(null));

            Assert.That(exception.Message, Is.EqualTo("Product cannot be null"));
        }

        [Test]
        public void ValidateAndConcludeAuction_CallsConcludeAuction_WhenProductIsValid()
        {
            // Arrange
            AuctionProduct validProduct = new AuctionProduct();

            // Act
            _auctionValidationService.ValidateAndConcludeAuction(validProduct);

            // Assert
            _mockAuctionProductsService.Verify(x => x.ConcludeAuction(validProduct), Times.Once);
        }
    }
}
