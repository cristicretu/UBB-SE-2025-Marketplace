using System;
using Moq;
using Xunit;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.AuctionValidationService;
using MarketMinds.Shared.Services.AuctionProductsService;

namespace MarketMinds.Tests.Services
{
    public class AuctionValidationServiceTests
    {
        private readonly Mock<IAuctionProductsService> mockAuctionProductsService;
        private readonly AuctionValidationService auctionValidationService;

        public AuctionValidationServiceTests()
        {
            mockAuctionProductsService = new Mock<IAuctionProductsService>();
            auctionValidationService = new AuctionValidationService(mockAuctionProductsService.Object);
        }

        [Fact]
        public void ValidateAndPlaceBid_ThrowsArgumentNullException_WhenProductIsNull()
        {
            User validUser = new User();
            string validBid = "100";

            var exception = Assert.Throws<ArgumentNullException>(() =>
                auctionValidationService.ValidateAndPlaceBid(null, validUser, validBid));

            Assert.Equal("Auction product cannot be null (Parameter 'product')", exception.Message);
        }

        [Fact]
        public void ValidateAndPlaceBid_ThrowsArgumentNullException_WhenBidderIsNull()
        {
            AuctionProduct validProduct = new AuctionProduct();
            string validBid = "100";

            var exception = Assert.Throws<ArgumentNullException>(() =>
                auctionValidationService.ValidateAndPlaceBid(validProduct, null, validBid));

            Assert.Equal("Bidder cannot be null (Parameter 'bidder')", exception.Message);
        }

        [Fact]
        public void ValidateAndPlaceBid_ThrowsArgumentException_WhenBidIsEmpty()
        {
            AuctionProduct validProduct = new AuctionProduct();
            User validUser = new User();
            string emptyBid = "  ";

            var exception = Assert.Throws<ArgumentException>(() =>
                auctionValidationService.ValidateAndPlaceBid(validProduct, validUser, emptyBid));

            Assert.Equal("Bid amount is required", exception.Message);
        }

        [Fact]
        public void ValidateAndPlaceBid_ThrowsArgumentException_WhenBidIsNotANumber()
        {
            AuctionProduct validProduct = new AuctionProduct();
            User validUser = new User();
            string invalidBidText = "ten dollars";

            var exception = Assert.Throws<ArgumentException>(() =>
                auctionValidationService.ValidateAndPlaceBid(validProduct, validUser, invalidBidText));

            Assert.Equal($"Invalid bid format: '{invalidBidText}' is not a valid number", exception.Message);
        }

        [Fact]
        public void ValidateAndPlaceBid_ThrowsArgumentException_WhenBidIsNegative()
        {
            AuctionProduct validProduct = new AuctionProduct();
            User validUser = new User();
            string negativeBid = "-5";

            var exception = Assert.Throws<ArgumentException>(() =>
                auctionValidationService.ValidateAndPlaceBid(validProduct, validUser, negativeBid));

            Assert.Equal("Bid amount must be positive", exception.Message);
        }

        [Fact]
        public void ValidateAndPlaceBid_CallsPlaceBid_WhenAllInputsAreValid()
        {
            AuctionProduct validProduct = new AuctionProduct();
            User validUser = new User();
            string validBidText = "150";
            double expectedBidAmount = 150;

            auctionValidationService.ValidateAndPlaceBid(validProduct, validUser, validBidText);

            mockAuctionProductsService.Verify(
                x => x.PlaceBid(validProduct, validUser, expectedBidAmount),
                Times.Once);
        }

        [Fact]
        public void ValidateAndPlaceBid_ThrowsWrappedException_WhenPlaceBidFails()
        {
            AuctionProduct validProduct = new AuctionProduct();
            User validUser = new User();
            string validBidText = "200";
            string expectedInnerMessage = "Simulated failure";

            mockAuctionProductsService
                .Setup(x => x.PlaceBid(It.IsAny<AuctionProduct>(), It.IsAny<User>(), It.IsAny<double>()))
                .Throws(new Exception(expectedInnerMessage));

            var exception = Assert.Throws<Exception>(() =>
                auctionValidationService.ValidateAndPlaceBid(validProduct, validUser, validBidText));

            Assert.StartsWith("Bid failed:", exception.Message);
            Assert.Contains(expectedInnerMessage, exception.InnerException?.Message);
        }

        [Fact]
        public void ValidateAndConcludeAuction_ThrowsArgumentException_WhenProductIsNull()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
                auctionValidationService.ValidateAndConcludeAuction(null));

            Assert.Equal("Product cannot be null", exception.Message);
        }

        [Fact]
        public void ValidateAndConcludeAuction_CallsConcludeAuction_WhenProductIsValid()
        {
            AuctionProduct validProduct = new AuctionProduct();

            auctionValidationService.ValidateAndConcludeAuction(validProduct);

            mockAuctionProductsService.Verify(x => x.ConcludeAuction(validProduct), Times.Once);
        }
    }
}
