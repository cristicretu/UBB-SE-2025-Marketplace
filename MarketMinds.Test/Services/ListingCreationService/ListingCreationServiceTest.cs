using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.ListingCreationService;
using MarketMinds.Shared.Services.BuyProductsService;
using MarketMinds.Shared.Services.BorrowProductsService;
using MarketMinds.Shared.Services.AuctionProductsService;
using Moq;
using NUnit.Framework;
using System;

namespace MarketMinds.Test.Services.ListingCreationService
{
    [TestFixture]
    public class ListingCreationServiceTest
    {
        private const string BUY_LISTING_TYPE = "buy";
        private const string BORROW_LISTING_TYPE = "borrow";
        private const string AUCTION_LISTING_TYPE = "auction";
        private const string INVALID_LISTING_TYPE = "invalid_type";
        private const string UPPERCASE_BUY_LISTING_TYPE = "BUY";
        private const string INVALID_LISTING_TYPE_ERROR = "Invalid listing type";

        private Mock<IBuyProductsService> _mockBuyService;
        private Mock<IBorrowProductsService> _mockBorrowService;
        private Mock<IAuctionProductsService> _mockAuctionService;
        private IListingCreationService _service;

        [SetUp]
        public void Setup()
        {
            _mockBuyService = new Mock<IBuyProductsService>(MockBehavior.Strict);
            _mockBorrowService = new Mock<IBorrowProductsService>(MockBehavior.Strict);
            _mockAuctionService = new Mock<IAuctionProductsService>(MockBehavior.Strict);

            _service = new MarketMinds.Shared.Services.ListingCreationService.ListingCreationService(
                _mockBuyService.Object,
                _mockBorrowService.Object,
                _mockAuctionService.Object);


        }

        [Test]
        public void CreateMarketListing_BuyType_WithBuyProduct_CallsBuyProductsService()
        {
            var buyProduct = new BuyProduct();
            _mockBuyService.Setup(s => s.CreateListing(buyProduct));
            _service.CreateMarketListing(buyProduct, BUY_LISTING_TYPE);
            _mockBuyService.Verify(s => s.CreateListing(buyProduct), Times.Once);
        }

        [Test]
        public void CreateMarketListing_BuyType_WithNonBuyProduct_DoesNotCallBuyProductsService()
        {
            var notBuyProduct = new AuctionProduct();
            // No setup for _mockBuyService, so if called, test will fail
            _service.CreateMarketListing(notBuyProduct, BUY_LISTING_TYPE);
            _mockBuyService.Verify(s => s.CreateListing(It.IsAny<BuyProduct>()), Times.Never);
        }

        [Test]
        public void CreateMarketListing_BuyType_DoesNotCallOtherServices()
        {
            var buyProduct = new BuyProduct();
            _mockBuyService.Setup(s => s.CreateListing(buyProduct));
            _service.CreateMarketListing(buyProduct, BUY_LISTING_TYPE);
            _mockBorrowService.Verify(s => s.CreateListing(It.IsAny<Product>()), Times.Never);
            _mockAuctionService.Verify(s => s.CreateListing(It.IsAny<Product>()), Times.Never);
        }

        [Test]
        public void CreateMarketListing_BorrowType_CallsBorrowProductsService()
        {
            var product = new BorrowProduct();
            _mockBorrowService.Setup(s => s.CreateListing(product));
            _service.CreateMarketListing(product, BORROW_LISTING_TYPE);
            _mockBorrowService.Verify(s => s.CreateListing(product), Times.Once);
        }

        [Test]
        public void CreateMarketListing_BorrowType_DoesNotCallOtherServices()
        {
            var product = new BorrowProduct();
            _mockBorrowService.Setup(s => s.CreateListing(product));
            _service.CreateMarketListing(product, BORROW_LISTING_TYPE);
            _mockBuyService.Verify(s => s.CreateListing(It.IsAny<BuyProduct>()), Times.Never);
            _mockAuctionService.Verify(s => s.CreateListing(It.IsAny<Product>()), Times.Never);
        }

        [Test]
        public void CreateMarketListing_AuctionType_CallsAuctionProductsService()
        {
            var product = new AuctionProduct();
            _mockAuctionService.Setup(s => s.CreateListing(product));
            _service.CreateMarketListing(product, AUCTION_LISTING_TYPE);
            _mockAuctionService.Verify(s => s.CreateListing(product), Times.Once);
        }

        [Test]
        public void CreateMarketListing_AuctionType_DoesNotCallOtherServices()
        {
            var product = new AuctionProduct();
            _mockAuctionService.Setup(s => s.CreateListing(product));
            _service.CreateMarketListing(product, AUCTION_LISTING_TYPE);
            _mockBuyService.Verify(s => s.CreateListing(It.IsAny<BuyProduct>()), Times.Never);
            _mockBorrowService.Verify(s => s.CreateListing(It.IsAny<Product>()), Times.Never);
        }

        [Test]
        public void CreateMarketListing_InvalidType_ThrowsArgumentException()
        {
            var product = new BuyProduct();
            var ex = Assert.Throws<ArgumentException>(() =>
                _service.CreateMarketListing(product, INVALID_LISTING_TYPE));
            Assert.That(ex.Message, Does.Contain(INVALID_LISTING_TYPE_ERROR));
        }

        [Test]
        public void CreateMarketListing_CaseInsensitive_CallsCorrectService()
        {
            var buyProduct = new BuyProduct();
            _mockBuyService.Setup(s => s.CreateListing(buyProduct));
            _service.CreateMarketListing(buyProduct, UPPERCASE_BUY_LISTING_TYPE);
            _mockBuyService.Verify(s => s.CreateListing(buyProduct), Times.Once);
        }

        [Test]
        public void CreateMarketListing_BuyType_WithNullProduct_DoesNotThrow()
        {
            // Should not throw, but also should not call any service
            _service.CreateMarketListing(null, BUY_LISTING_TYPE);
            _mockBuyService.Verify(s => s.CreateListing(It.IsAny<BuyProduct>()), Times.Never);
            _mockBorrowService.Verify(s => s.CreateListing(It.IsAny<Product>()), Times.Never);
            _mockAuctionService.Verify(s => s.CreateListing(It.IsAny<Product>()), Times.Never);
        }

        //[Test]
        //public void CreateMarketListing_BorrowType_WithNullProduct_DoesNotThrow()
        //{
        //    _service.CreateMarketListing(null, BORROW_LISTING_TYPE);
        //    _mockBuyService.Verify(s => s.CreateListing(It.IsAny<BuyProduct>()), Times.Never);
        //    _mockBorrowService.Verify(s => s.CreateListing(It.IsAny<Product>()), Times.Never);
        //    _mockAuctionService.Verify(s => s.CreateListing(It.IsAny<Product>()), Times.Never);
        //}

        //[Test]
        //public void CreateMarketListing_AuctionType_WithNullProduct_DoesNotThrow()
        //{
        //    _service.CreateMarketListing(null, AUCTION_LISTING_TYPE);
        //    _mockBuyService.Verify(s => s.CreateListing(It.IsAny<BuyProduct>()), Times.Never);
        //    _mockBorrowService.Verify(s => s.CreateListing(It.IsAny<Product>()), Times.Never);
        //    _mockAuctionService.Verify(s => s.CreateListing(It.IsAny<Product>()), Times.Never);
        //}
    }
}
