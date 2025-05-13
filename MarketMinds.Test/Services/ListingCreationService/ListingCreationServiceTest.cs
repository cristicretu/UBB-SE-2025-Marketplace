//using MarketMinds.Shared.Models;
//using MarketMinds.Shared.Services;
//using MarketMinds.Shared.Services.AuctionProductsService;
//using MarketMinds.Shared.Services.BorrowProductsService;
//using MarketMinds.Shared.Services.BuyProductsService;
//using Moq;
//using NUnit.Framework;
//using System;
//using MarketMinds.Shared.Services.ListingCreationService;

//namespace MarketMinds.Test.Services
//{
//    [TestFixture]
//    public class ListingCreationServiceTest
//    {
//        // Constants for listing types
//        private const string BUY_LISTING_TYPE = "buy";
//        private const string BORROW_LISTING_TYPE = "borrow";
//        private const string AUCTION_LISTING_TYPE = "auction";
//        private const string INVALID_LISTING_TYPE = "invalid_type";
//        private const string UPPERCASE_BUY_LISTING_TYPE = "BUY";
//        private const string INVALID_LISTING_TYPE_ERROR = "Invalid listing type";

//        private Mock<IBuyProductsService> _mockBuyService;
//        private Mock<IBorrowProductsService> _mockBorrowService;
//        private Mock<IAuctionProductsService> _mockAuctionService;
//        private IListingCreationService _service;
//        private Product _testProduct;

//        [SetUp]
//        public void Setup()
//        {
//            _mockBuyService = new Mock<IBuyProductsService>();
//            _mockBorrowService = new Mock<IBorrowProductsService>();
//            _mockAuctionService = new Mock<IAuctionProductsService>();

//            _service = new ListingCreationService(
//                _mockBuyService.Object,
//                _mockBorrowService.Object,
//                _mockAuctionService.Object);
//        }

//        [Test]
//        public void CreateMarketListing_BuyType_CallsBuyProductsService()
//        {
//            // Act
//            _service.CreateMarketListing(_testProduct, BUY_LISTING_TYPE);

//            // Assert
//            _mockBuyService.Verify(s => s.CreateListing(_testProduct), Times.Once);
//        }

//        [Test]
//        public void CreateMarketListing_BuyType_DoesNotCallOtherServices()
//        {
//            // Act
//            _service.CreateMarketListing(_testProduct, BUY_LISTING_TYPE);

//            // Assert
//            _mockBorrowService.Verify(s => s.CreateListing(It.IsAny<Product>()), Times.Never);
//            _mockAuctionService.Verify(s => s.CreateListing(It.IsAny<Product>()), Times.Never);
//        }

//        [Test]
//        public void CreateMarketListing_BorrowType_CallsBorrowProductsService()
//        {
//            // Act
//            _service.CreateMarketListing(_testProduct, BORROW_LISTING_TYPE);

//            // Assert
//            _mockBorrowService.Verify(s => s.CreateListing(_testProduct), Times.Once);
//        }

//        [Test]
//        public void CreateMarketListing_BorrowType_DoesNotCallOtherServices()
//        {
//            // Act
//            _service.CreateMarketListing(_testProduct, BORROW_LISTING_TYPE);

//            // Assert
//            _mockBuyService.Verify(s => s.CreateListing(It.IsAny<Product>()), Times.Never);
//            _mockAuctionService.Verify(s => s.CreateListing(It.IsAny<Product>()), Times.Never);
//        }

//        [Test]
//        public void CreateMarketListing_AuctionType_CallsAuctionProductsService()
//        {
//            // Act
//            _service.CreateMarketListing(_testProduct, AUCTION_LISTING_TYPE);

//            // Assert
//            _mockAuctionService.Verify(s => s.CreateListing(_testProduct), Times.Once);
//        }

//        [Test]
//        public void CreateMarketListing_AuctionType_DoesNotCallOtherServices()
//        {
//            // Act
//            _service.CreateMarketListing(_testProduct, AUCTION_LISTING_TYPE);

//            // Assert
//            _mockBuyService.Verify(s => s.CreateListing(It.IsAny<Product>()), Times.Never);
//            _mockBorrowService.Verify(s => s.CreateListing(It.IsAny<Product>()), Times.Never);
//        }

//        [Test]
//        public void CreateMarketListing_InvalidType_ThrowsArgumentException()
//        {
//            // Act & Assert
//            ArgumentException thrownException = Assert.Throws<ArgumentException>(() =>
//                _service.CreateMarketListing(_testProduct, INVALID_LISTING_TYPE));

//            Assert.That(thrownException, Is.Not.Null);
//        }

//        [Test]
//        public void CreateMarketListing_InvalidType_ContainsExpectedErrorMessage()
//        {
//            // Act & Assert
//            ArgumentException thrownException = Assert.Throws<ArgumentException>(() =>
//                _service.CreateMarketListing(_testProduct, INVALID_LISTING_TYPE));

//            Assert.That(thrownException.Message, Does.Contain(INVALID_LISTING_TYPE_ERROR));
//        }

//        [Test]
//        public void CreateMarketListing_CaseInsensitive_CallsCorrectService()
//        {
//            // Act
//            _service.CreateMarketListing(_testProduct, UPPERCASE_BUY_LISTING_TYPE);

//            // Assert
//            _mockBuyService.Verify(s => s.CreateListing(_testProduct), Times.Once);
//        }
//    }
//}

