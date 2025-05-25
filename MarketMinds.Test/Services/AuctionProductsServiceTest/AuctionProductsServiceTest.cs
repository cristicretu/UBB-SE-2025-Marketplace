using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.ProxyRepository;
using MarketMinds.Shared.Services.AuctionProductsService;
using MarketMinds.Test.Services.Mocks;
using Moq;
using NUnit.Framework;

namespace MarketMinds.Test.Services.AuctionProductsServiceTest
{
    [TestFixture]
    public class AuctionProductsServiceTest
    {
        private Mock<AuctionProductsProxyRepository> _mockRepository;
        private AuctionProductsService _service;
        private List<AuctionProduct> _testProducts;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new Mock<AuctionProductsProxyRepository>();
            _service = new AuctionProductsService(_mockRepository.Object);
            _testProducts = new List<AuctionProduct>();

            // Setup mock repository methods
            _mockRepository.Setup(repo => repo.GetProducts())
                .Returns(() => _testProducts);

            _mockRepository.Setup(repo => repo.GetProductById(It.IsAny<int>()))
                .Returns<int>(id => _testProducts.FirstOrDefault(p => p.Id == id));

            _mockRepository.Setup(repo => repo.CreateListing(It.IsAny<AuctionProduct>()))
                .Callback<Product>(product =>
                {
                    var auctionProduct = (AuctionProduct)product;
                    if (auctionProduct.Id == 0)
                    {
                        auctionProduct.Id = _testProducts.Count > 0 ? _testProducts.Max(p => p.Id) + 1 : 1;
                    }

                    var existingProduct = _testProducts.FirstOrDefault(p => p.Id == auctionProduct.Id);
                    if (existingProduct != null)
                    {
                        _testProducts.Remove(existingProduct);
                    }

                    _testProducts.Add(auctionProduct);
                });

            _mockRepository.Setup(repo => repo.PlaceBid(It.IsAny<AuctionProduct>(), It.IsAny<User>(), It.IsAny<double>()))
                .Callback<AuctionProduct, User, double>((auction, bidder, amount) =>
                {
                    var existingAuction = _testProducts.FirstOrDefault(a => a.Id == auction.Id);
                    if (existingAuction == null)
                    {
                        throw new KeyNotFoundException("Auction not found");
                    }

                    if (amount <= existingAuction.CurrentPrice)
                    {
                        throw new InvalidOperationException("Bid amount must be higher than current price");
                    }

                    if (bidder.Id == auction.SellerId)
                    {
                        throw new InvalidOperationException("You cannot bid on your own auction");
                    }

                    var bid = new Bid(bidder.Id, auction.Id, amount)
                    {
                        Timestamp = DateTime.Now
                    };

                    if (existingAuction.Bids == null)
                    {
                        existingAuction.Bids = new List<Bid>();
                    }

                    existingAuction.Bids.Add(bid);
                    existingAuction.CurrentPrice = amount;
                });

            _mockRepository.Setup(repo => repo.ConcludeAuction(It.IsAny<AuctionProduct>()))
                .Callback<AuctionProduct>(auction =>
                {
                    var existingAuction = _testProducts.FirstOrDefault(a => a.Id == auction.Id);
                    if (existingAuction != null)
                    {
                        _testProducts.Remove(existingAuction);
                    }
                });
        }

        #region CreateListing Tests

        [Test]
        public void CreateListing_WithValidProduct_ShouldAddToRepository()
        {
            // Arrange
            var product = new AuctionProduct
            {
                Title = "Vintage Watch",
                StartPrice = 100,
                CurrentPrice = 100,
                SellerId = 1,
                Condition = new Condition { Id = 1 },
                Category = new Category { Id = 1 },
                Tags = new List<ProductTag>(),
                Bids = new List<Bid>()
            };

            // Act
            _service.CreateListing(product);

            // Assert
            _mockRepository.Verify(repo => repo.CreateListing(It.IsAny<AuctionProduct>()), Times.Once);
            Assert.That(product.Id, Is.GreaterThan(0));
        }

        [Test]
        public void CreateListing_WithNonAuctionProduct_ShouldThrowArgumentException()
        {
            // Arrange
            var product = new MockNonAuctionProduct();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _service.CreateListing(product));
            Assert.That(exception.Message, Does.Contain("Product must be an AuctionProduct"));
        }

        [Test]
        public void CreateListing_WithDefaultStartTime_ShouldSetCurrentTime()
        {
            // Arrange
            var product = new AuctionProduct
            {
                Title = "Test Product",
                StartPrice = 50,
                StartTime = default(DateTime)
            };

            // Act
            _service.CreateListing(product);

            // Assert
            Assert.That(product.StartTime, Is.GreaterThanOrEqualTo(DateTime.Now.AddSeconds(-5)));
            Assert.That(product.StartTime, Is.LessThanOrEqualTo(DateTime.Now));
        }

        [Test]
        public void CreateListing_WithDefaultEndTime_ShouldSetFutureTime()
        {
            // Arrange
            var product = new AuctionProduct
            {
                Title = "Test Product",
                StartPrice = 50,
                EndTime = default(DateTime)
            };

            // Act
            _service.CreateListing(product);

            // Assert
            Assert.That(product.EndTime, Is.GreaterThan(DateTime.Now.AddDays(6.9)));
            Assert.That(product.EndTime, Is.LessThan(DateTime.Now.AddDays(7.1)));
        }

        [Test]
        public void CreateListing_WithMissingStartPrice_ShouldUseCurrentPrice()
        {
            // Arrange
            var product = new AuctionProduct
            {
                Title = "Test Product",
                StartPrice = 0,
                CurrentPrice = 75
            };

            // Act
            _service.CreateListing(product);

            // Assert
            Assert.That(product.StartPrice, Is.EqualTo(75));
        }

        [Test]
        public void CreateListing_WithMissingCurrentPrice_ShouldUseStartPrice()
        {
            // Arrange
            var product = new AuctionProduct
            {
                Title = "Test Product",
                StartPrice = 100,
                CurrentPrice = 0
            };

            // Act
            _service.CreateListing(product);

            // Assert
            Assert.That(product.CurrentPrice, Is.EqualTo(100));
        }

        #endregion

        #region PlaceBid Tests

        [Test]
        public void PlaceBid_WithValidBid_ShouldUpdateAuctionAndAddBid()
        {
            // Arrange
            var auction = new AuctionProduct
            {
                Id = 1,
                Title = "Rare Coin",
                StartPrice = 100,
                CurrentPrice = 100,
                StartTime = DateTime.Now.AddMinutes(-10),
                EndTime = DateTime.Now.AddDays(1),
                SellerId = 1,
                Bids = new List<Bid>()
            };
            _testProducts.Add(auction);

            var bidder = new User { Id = 2, Balance = 500 };
            const double bidAmount = 150;

            // Act
            _service.PlaceBid(auction, bidder, bidAmount);

            // Assert
            _mockRepository.Verify(repo => repo.PlaceBid(auction, bidder, bidAmount), Times.Once);
            Assert.That(auction.CurrentPrice, Is.EqualTo(bidAmount));
            Assert.That(bidder.Balance, Is.EqualTo(500 - bidAmount));
        }

        [Test]
        public void PlaceBid_WithNullAuction_ShouldThrowArgumentNullException()
        {
            // Arrange
            var bidder = new User { Id = 2, Balance = 500 };
            const double bidAmount = 150;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => _service.PlaceBid(null, bidder, bidAmount));
            Assert.That(exception.ParamName, Is.EqualTo("auction"));
        }

        [Test]
        public void PlaceBid_WithNullBidder_ShouldThrowArgumentNullException()
        {
            // Arrange
            var auction = new AuctionProduct
            {
                Id = 1,
                StartPrice = 100,
                CurrentPrice = 100,
                StartTime = DateTime.Now.AddMinutes(-10),
                EndTime = DateTime.Now.AddDays(1)
            };
            const double bidAmount = 150;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => _service.PlaceBid(auction, null, bidAmount));
            Assert.That(exception.ParamName, Is.EqualTo("bidder"));
        }

        [Test]
        public void PlaceBid_WithUnsavedAuction_ShouldThrowException()
        {
            // Arrange
            var auction = new AuctionProduct
            {
                Id = 0,
                StartPrice = 100,
                CurrentPrice = 100,
                StartTime = DateTime.Now.AddMinutes(-10),
                EndTime = DateTime.Now.AddDays(1),
                Bids = new List<Bid>()
            };
            var bidder = new User { Id = 2, Balance = 500 };
            const double bidAmount = 150;

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => _service.PlaceBid(auction, bidder, bidAmount));
            Assert.That(exception.Message, Does.Contain("Cannot bid on an unsaved auction"));
        }

        [Test]
        public void PlaceBid_WithUnsavedBidder_ShouldThrowException()
        {
            // Arrange
            var auction = new AuctionProduct
            {
                Id = 1,
                StartPrice = 100,
                CurrentPrice = 100,
                StartTime = DateTime.Now.AddMinutes(-10),
                EndTime = DateTime.Now.AddDays(1),
                SellerId = 1,
                Bids = new List<Bid>()
            };
            _testProducts.Add(auction);

            var bidder = new User { Id = 0, Balance = 500 };
            const double bidAmount = 150;

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => _service.PlaceBid(auction, bidder, bidAmount));
            Assert.That(exception.Message, Does.Contain("Cannot bid with an unsaved user profile"));
        }

        [Test]
        public void PlaceBid_WhenBidderIsSeller_ShouldThrowException()
        {
            // Arrange
            var auction = new AuctionProduct
            {
                Id = 1,
                StartPrice = 100,
                CurrentPrice = 100,
                StartTime = DateTime.Now.AddMinutes(-10),
                EndTime = DateTime.Now.AddDays(1),
                SellerId = 2,
                Bids = new List<Bid>()
            };
            _testProducts.Add(auction);

            var bidder = new User { Id = 2, Balance = 500 };
            const double bidAmount = 150;

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => _service.PlaceBid(auction, bidder, bidAmount));
            Assert.That(exception.Message, Does.Contain("You cannot bid on your own auction"));
        }

        [Test]
        public void PlaceBid_WithEndedAuction_ShouldThrowException()
        {
            // Arrange
            var auction = new AuctionProduct
            {
                Id = 1,
                StartPrice = 100,
                CurrentPrice = 100,
                StartTime = DateTime.Now.AddDays(-2),
                EndTime = DateTime.Now.AddHours(-1),
                SellerId = 1,
                Bids = new List<Bid>()
            };
            _testProducts.Add(auction);

            var bidder = new User { Id = 2, Balance = 500 };
            const double bidAmount = 150;

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => _service.PlaceBid(auction, bidder, bidAmount));
            Assert.That(exception.Message, Does.Contain("Auction already ended"));
        }

        [Test]
        public void PlaceBid_WithFutureAuction_ShouldThrowException()
        {
            // Arrange
            var auction = new AuctionProduct
            {
                Id = 1,
                StartPrice = 100,
                CurrentPrice = 100,
                StartTime = DateTime.Now.AddHours(1),
                EndTime = DateTime.Now.AddDays(1),
                SellerId = 1,
                Bids = new List<Bid>()
            };
            _testProducts.Add(auction);

            var bidder = new User { Id = 2, Balance = 500 };
            const double bidAmount = 150;

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => _service.PlaceBid(auction, bidder, bidAmount));
            Assert.That(exception.Message, Does.Contain("Auction hasn't started yet"));
        }

        [Test]
        public void PlaceBid_WithBidTooLow_ShouldThrowException()
        {
            // Arrange
            var auction = new AuctionProduct
            {
                Id = 1,
                StartPrice = 100,
                CurrentPrice = 100,
                StartTime = DateTime.Now.AddMinutes(-10),
                EndTime = DateTime.Now.AddDays(1),
                SellerId = 1,
                Bids = new List<Bid>()
            };
            _testProducts.Add(auction);

            var bidder = new User { Id = 2, Balance = 500 };
            const double bidAmount = 99; // Lower than starting price

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => _service.PlaceBid(auction, bidder, bidAmount));
            Assert.That(exception.Message, Does.Contain("Bid must be at least $100"));
        }

        [Test]
        public void PlaceBid_WithInsufficientBalance_ShouldThrowException()
        {
            // Arrange
            var auction = new AuctionProduct
            {
                Id = 1,
                StartPrice = 100,
                CurrentPrice = 100,
                StartTime = DateTime.Now.AddMinutes(-10),
                EndTime = DateTime.Now.AddDays(1),
                SellerId = 1,
                Bids = new List<Bid>()
            };
            _testProducts.Add(auction);

            var bidder = new User { Id = 2, Balance = 90 }; // Not enough balance
            const double bidAmount = 150;

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => _service.PlaceBid(auction, bidder, bidAmount));
            Assert.That(exception.Message, Does.Contain("Insufficient balance"));
        }

        [Test]
        public void PlaceBid_WithExistingBids_ShouldRequireHigherBid()
        {
            // Arrange
            var auction = new AuctionProduct
            {
                Id = 1,
                StartPrice = 100,
                CurrentPrice = 120,
                StartTime = DateTime.Now.AddMinutes(-10),
                EndTime = DateTime.Now.AddDays(1),
                SellerId = 1,
                Bids = new List<Bid>
                {
                    new Bid(2, 1, 120) { Timestamp = DateTime.Now.AddMinutes(-5) }
                }
            };
            _testProducts.Add(auction);

            var bidder = new User { Id = 3, Balance = 500 };
            const double bidAmount = 120; // Same as current price, not higher

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => _service.PlaceBid(auction, bidder, bidAmount));
            Assert.That(exception.Message, Does.Contain("Bid must be at least $121"));
        }

        [Test]
        public void PlaceBid_WithShortTimeLeft_ShouldExtendAuctionTime()
        {
            // Arrange
            var nearEndTime = DateTime.Now.AddMinutes(3);
            var auction = new AuctionProduct
            {
                Id = 1,
                StartPrice = 100,
                CurrentPrice = 100,
                StartTime = DateTime.Now.AddHours(-1),
                EndTime = nearEndTime,
                SellerId = 1,
                Bids = new List<Bid>()
            };
            _testProducts.Add(auction);

            var bidder = new User { Id = 2, Balance = 500 };
            const double bidAmount = 150;

            // Act
            _service.PlaceBid(auction, bidder, bidAmount);

            // Assert
            Assert.That(auction.EndTime, Is.GreaterThan(nearEndTime));
            Assert.That(auction.EndTime, Is.GreaterThanOrEqualTo(DateTime.Now.AddMinutes(4.9)));
            Assert.That(auction.EndTime, Is.LessThanOrEqualTo(DateTime.Now.AddMinutes(5.1)));
        }

        #endregion

        #region ConcludeAuction Tests

        [Test]
        public void ConcludeAuction_WithValidAuction_ShouldRemoveFromRepository()
        {
            // Arrange
            var auction = new AuctionProduct
            {
                Id = 1,
                Title = "Painting",
                StartPrice = 50,
                CurrentPrice = 60,
                SellerId = 10,
                StartTime = DateTime.Now.AddMinutes(-10),
                EndTime = DateTime.Now.AddMinutes(10)
            };
            _testProducts.Add(auction);

            // Act
            _service.ConcludeAuction(auction);

            // Assert
            _mockRepository.Verify(repo => repo.ConcludeAuction(auction), Times.Once);
            Assert.That(_testProducts, Does.Not.Contain(auction));
        }

        [Test]
        public void ConcludeAuction_WithUnsavedAuction_ShouldThrowArgumentException()
        {
            // Arrange
            var auction = new AuctionProduct
            {
                Id = 0,
                Title = "Unsaved Auction"
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _service.ConcludeAuction(auction));
            Assert.That(exception.Message, Does.Contain("Auction Product ID must be set for delete"));
        }

        #endregion

        #region Time and Status Tests

        [Test]
        public void GetTimeLeft_WithActiveAuction_ShouldReturnFormattedTimeString()
        {
            // Arrange
            var auction = new AuctionProduct
            {
                EndTime = DateTime.Now.AddDays(2).AddHours(3).AddMinutes(15)
            };

            // Act
            var timeLeft = _service.GetTimeLeft(auction);

            // Assert
            Assert.That(timeLeft, Does.Match(@"02\:03\:15\:.*"));
        }

        [Test]
        public void GetTimeLeft_WithEndedAuction_ShouldReturnAuctionEnded()
        {
            // Arrange
            var auction = new AuctionProduct
            {
                EndTime = DateTime.Now.AddMinutes(-30)
            };

            // Act
            var timeLeft = _service.GetTimeLeft(auction);

            // Assert
            Assert.That(timeLeft, Is.EqualTo("Auction Ended"));
        }

        [Test]
        public void IsAuctionEnded_WithFutureEndTime_ShouldReturnFalse()
        {
            // Arrange
            var auction = new AuctionProduct
            {
                EndTime = DateTime.Now.AddHours(1)
            };

            // Act
            var isEnded = _service.IsAuctionEnded(auction);

            // Assert
            Assert.That(isEnded, Is.False);
        }

        [Test]
        public void IsAuctionEnded_WithPastEndTime_ShouldReturnTrue()
        {
            // Arrange
            var auction = new AuctionProduct
            {
                EndTime = DateTime.Now.AddMinutes(-1)
            };

            // Act
            var isEnded = _service.IsAuctionEnded(auction);

            // Assert
            Assert.That(isEnded, Is.True);
        }

        #endregion

        #region GetProducts and GetProductById Tests

        [Test]
        public void GetProducts_ShouldReturnAllAuctionProducts()
        {
            // Arrange
            _testProducts.Add(new AuctionProduct { Id = 1, Title = "Product 1" });
            _testProducts.Add(new AuctionProduct { Id = 2, Title = "Product 2" });
            _testProducts.Add(new AuctionProduct { Id = 3, Title = "Product 3" });

            // Act
            var products = _service.GetProducts();

            // Assert
            Assert.That(products.Count, Is.EqualTo(3));
            Assert.That(products, Does.Contain(_testProducts[0]));
            Assert.That(products, Does.Contain(_testProducts[1]));
            Assert.That(products, Does.Contain(_testProducts[2]));
        }

        [Test]
        public void GetProductById_WithValidId_ShouldReturnProduct()
        {
            // Arrange
            var expectedProduct = new AuctionProduct { Id = 5, Title = "Test Product" };
            _testProducts.Add(expectedProduct);

            // Act
            var product = _service.GetProductById(5);

            // Assert
            Assert.That(product, Is.EqualTo(expectedProduct));
        }

        [Test]
        public void GetProductById_WithInvalidId_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            int nonExistentId = 999;

            // Act & Assert
            var exception = Assert.Throws<KeyNotFoundException>(() => _service.GetProductById(nonExistentId));
            Assert.That(exception.Message, Does.Contain($"Auction product with ID {nonExistentId} not found"));
        }

        #endregion

        #region GetSortedFilteredProducts Tests

        [Test]
        public void GetSortedFilteredProducts_WithNoFilters_ShouldReturnAllProducts()
        {
            // Arrange
            _testProducts.Add(new AuctionProduct
            {
                Id = 1,
                Title = "Product 1",
                Condition = new Condition { Id = 1 },
                Category = new Category { Id = 1 },
                Tags = new List<ProductTag>()
            });
            _testProducts.Add(new AuctionProduct
            {
                Id = 2,
                Title = "Product 2",
                Condition = new Condition { Id = 2 },
                Category = new Category { Id = 2 },
                Tags = new List<ProductTag>()
            });

            // Act
            var products = _service.GetSortedFilteredProducts(null, null, null, null, null);

            // Assert
            Assert.That(products.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetSortedFilteredProducts_WithConditionFilter_ShouldReturnMatchingProducts()
        {
            // Arrange
            var condition1 = new Condition { Id = 1, Name = "New" };
            var condition2 = new Condition { Id = 2, Name = "Used" };

            _testProducts.Add(new AuctionProduct
            {
                Id = 1,
                Title = "Product 1",
                Condition = condition1,
                Category = new Category { Id = 1 },
                Tags = new List<ProductTag>()
            });
            _testProducts.Add(new AuctionProduct
            {
                Id = 2,
                Title = "Product 2",
                Condition = condition2,
                Category = new Category { Id = 1 },
                Tags = new List<ProductTag>()
            });

            var conditionFilter = new List<Condition> { condition1 };

            // Act
            var products = _service.GetSortedFilteredProducts(conditionFilter, null, null, null, null);

            // Assert
            Assert.That(products.Count, Is.EqualTo(1));
            Assert.That(products[0].Title, Is.EqualTo("Product 1"));
        }

        [Test]
        public void GetSortedFilteredProducts_WithSearchQuery_ShouldReturnMatchingProducts()
        {
            // Arrange
            _testProducts.Add(new AuctionProduct
            {
                Id = 1,
                Title = "Vintage Watch",
                Condition = new Condition { Id = 1 },
                Category = new Category { Id = 1 },
                Tags = new List<ProductTag>()
            });
            _testProducts.Add(new AuctionProduct
            {
                Id = 2,
                Title = "Modern Laptop",
                Condition = new Condition { Id = 1 },
                Category = new Category { Id = 1 },
                Tags = new List<ProductTag>()
            });

            // Act
            var products = _service.GetSortedFilteredProducts(null, null, null, null, "vintage");

            // Assert
            Assert.That(products.Count, Is.EqualTo(1));
            Assert.That(products[0].Title, Is.EqualTo("Vintage Watch"));
        }

        [Test]
        public void GetSortedFilteredProducts_WithSortCondition_ShouldReturnSortedProducts()
        {
            // Arrange
            _testProducts.Add(new AuctionProduct
            {
                Id = 1,
                Title = "Product B",
                CurrentPrice = 200,
                Condition = new Condition { Id = 1 },
                Category = new Category { Id = 1 },
                Tags = new List<ProductTag>()
            });
            _testProducts.Add(new AuctionProduct
            {
                Id = 2,
                Title = "Product A",
                CurrentPrice = 100,
                Condition = new Condition { Id = 1 },
                Category = new Category { Id = 1 },
                Tags = new List<ProductTag>()
            });

            // Sort by current price ascending
            var sortCondition = new ProductSortType("Current Price", "CurrentPrice", true);

            // Act
            var products = _service.GetSortedFilteredProducts(null, null, null, sortCondition, null);

            // Assert
            Assert.That(products.Count, Is.EqualTo(2));
            Assert.That(products[0].Title, Is.EqualTo("Product A"));
            Assert.That(products[1].Title, Is.EqualTo("Product B"));
        }

        #endregion

        #region Async Methods Tests

        [Test]
        public async Task GetAllAuctionProductsAsync_ShouldReturnAllProducts()
        {
            // Arrange
            _testProducts.Add(new AuctionProduct { Id = 1, Title = "Product 1" });
            _testProducts.Add(new AuctionProduct { Id = 2, Title = "Product 2" });

            // Act
            var products = await _service.GetAllAuctionProductsAsync();

            // Assert
            Assert.That(products.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetAuctionProductByIdAsync_WithValidId_ShouldReturnProduct()
        {
            // Arrange
            var expectedProduct = new AuctionProduct { Id = 3, Title = "Async Test Product" };
            _testProducts.Add(expectedProduct);

            // Act
            var product = await _service.GetAuctionProductByIdAsync(3);

            // Assert
            Assert.That(product, Is.EqualTo(expectedProduct));
        }

        [Test]
        public async Task CreateAuctionProductAsync_ShouldAddProductToRepository()
        {
            // Arrange
            var product = new AuctionProduct
            {
                Title = "Async Created Product",
                StartPrice = 50,
                SellerId = 1
            };

            // Act
            var result = await _service.CreateAuctionProductAsync(product);

            // Assert
            Assert.That(result, Is.True);
            _mockRepository.Verify(repo => repo.CreateListing(It.IsAny<AuctionProduct>()), Times.Once);
        }

        [Test]
        public async Task PlaceBidAsync_WithValidBid_ShouldReturnTrue()
        {
            // Arrange
            var auction = new AuctionProduct
            {
                Id = 4,
                Title = "Async Auction",
                StartPrice = 100,
                CurrentPrice = 100,
                StartTime = DateTime.Now.AddMinutes(-10),
                EndTime = DateTime.Now.AddDays(1),
                SellerId = 1,
                Bids = new List<Bid>()
            };
            _testProducts.Add(auction);

            // Mock the repository behavior for this test
            _mockRepository.Setup(repo => repo.PlaceBid(It.IsAny<AuctionProduct>(), It.IsAny<User>(), It.IsAny<double>()))
                .Callback<AuctionProduct, User, double>((a, b, amount) =>
                {
                    var existingAuction = _testProducts.FirstOrDefault(x => x.Id == a.Id);
                    if (existingAuction != null)
                    {
                        existingAuction.CurrentPrice = amount;
                        if (existingAuction.Bids == null)
                            existingAuction.Bids = new List<Bid>();
                        existingAuction.Bids.Add(new Bid(b.Id, a.Id, amount));
                    }
                });

            // Act
            var result = await _service.PlaceBidAsync(4, 2, 150);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(auction.CurrentPrice, Is.EqualTo(150));
        }

        [Test]
        public async Task DeleteAuctionProductAsync_WithValidId_ShouldReturnTrue()
        {
            // Arrange
            var auction = new AuctionProduct
            {
                Id = 5,
                Title = "Product to Delete"
            };
            _testProducts.Add(auction);

            // Act
            var result = await _service.DeleteAuctionProductAsync(5);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(_testProducts, Does.Not.Contain(auction));
        }

        #endregion

        #region Helper Classes

        private class MockNonAuctionProduct : Product
        {
            public MockNonAuctionProduct() : base() { }
        }

        #endregion
    }
}
