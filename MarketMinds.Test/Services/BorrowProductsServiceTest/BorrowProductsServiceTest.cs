//using System;
//using System.Collections.Generic;
//using NUnit.Framework;
//using MarketMinds.Shared.Models;
//using MarketMinds.Shared.Services.BorrowProductsService;
//using MarketMinds.Repositories.BorrowProductsRepository;
//using MarketMinds.Test.Services.BorrowProductsServiceTest;

//namespace MarketMinds.Tests.Services.BorrowProductsServiceTest
//{
//    [TestFixture]
//    public class BorrowProductsServiceTest
//    {
//        // Constants to replace magic numbers
//        private const int TEST_SELLER_ID = 1;
//        private const int TEST_BORROW_PRODUCT_ID = 1;
//        private const int TEST_INVALID_PRODUCT_ID = 2;
//        private const int NON_EXISTENT_PRODUCT_ID = 999;
//        private const float DAILY_RATE_AMOUNT = 20.0f;
//        private const float STARTING_PRICE_AMOUNT = 100.0f;
//        private const int EXPECTED_SINGLE_COUNT = 1;
//        private const int EXPECTED_ZERO_COUNT = 0;
//        private const int TIME_SPAN_DAYS_LONG = 7;
//        private const int TIME_SPAN_DAYS_MEDIUM = 5;

//        private BorrowProductsService borrowProductsService;
//        private BorrowProductsRepositoryMock borrowProductsRepositoryMock;
//        private User testSeller;
//        private BorrowProduct testBorrowProduct;
//        private AuctionProduct testInvalidProduct;

//        [SetUp]
//        public void Setup()
//        {
//            borrowProductsRepositoryMock = new BorrowProductsRepositoryMock();
//            borrowProductsService = new BorrowProductsService(borrowProductsRepositoryMock);

//            testSeller = new User(TEST_SELLER_ID, "Test Seller", "seller@test.com");

//            var testCondition = new Condition(1, "New", "Brand new item");
//            var testCategory = new Category(1, "Electronics", "Electronic devices");
//            var testTags = new List<ProductTag>();
//            var testImages = new List<Image>();

//            DateTime timeLimit = DateTime.Now.AddDays(TIME_SPAN_DAYS_LONG);
//            DateTime startDate = DateTime.Now;
//            DateTime endDate = DateTime.Now.AddDays(TIME_SPAN_DAYS_MEDIUM);
//            bool isBorrowed = false;

//            testBorrowProduct = new BorrowProduct(
//                TEST_BORROW_PRODUCT_ID,
//                "Test Borrow Product",
//                "Test Description",
//                testSeller,
//                testCondition,
//                testCategory,
//                testTags,
//                testImages,
//                timeLimit,
//                startDate,
//                endDate,
//                DAILY_RATE_AMOUNT,
//                isBorrowed);

//            // Create an invalid product type for testing type validation
//            testInvalidProduct = new AuctionProduct(
//                TEST_INVALID_PRODUCT_ID,
//                "Test Auction Product",
//                "Test Description",
//                testSeller,
//                testCondition,
//                testCategory,
//                testTags,
//                testImages,
//                DateTime.Now,
//                DateTime.Now.AddDays(TIME_SPAN_DAYS_LONG),
//                STARTING_PRICE_AMOUNT);
//        }

//        [Test]
//        public void TestCreateListing_ValidProduct_AddsProduct()
//        {
//            // Act
//            borrowProductsService.CreateListing(testBorrowProduct);

//            // Assert
//            Assert.That(borrowProductsRepositoryMock.GetCreateListingCount(), Is.EqualTo(EXPECTED_SINGLE_COUNT));
//        }

//        [Test]
//        public void TestCreateListing_InvalidProductType_ThrowsInvalidCastException()
//        {
//            // Arrange & Act & Assert
//            InvalidCastException castException = Assert.Throws<InvalidCastException>(() =>
//                borrowProductsService.CreateListing(testInvalidProduct));

//            Assert.That(castException, Is.Not.Null);
//        }

//        [Test]
//        public void TestCreateListing_InvalidProductType_DoesNotAddToRepository()
//        {
//            // Arrange & Act
//            try
//            {
//                borrowProductsService.CreateListing(testInvalidProduct);
//            }
//            catch
//            {
//                // Expected exception, ignore
//            }

//            // Assert
//            Assert.That(borrowProductsRepositoryMock.GetCreateListingCount(), Is.EqualTo(EXPECTED_ZERO_COUNT));
//        }

//        [Test]
//        public void TestDeleteListing_ValidProduct_DeletesProduct()
//        {
//            // Act
//            borrowProductsService.DeleteListing(testBorrowProduct);

//            // Assert
//            Assert.That(borrowProductsRepositoryMock.GetDeleteListingCount(), Is.EqualTo(EXPECTED_SINGLE_COUNT));
//        }

//        [Test]
//        public void TestGetProducts_ReturnsNonNullProductsList()
//        {
//            // Arrange
//            borrowProductsRepositoryMock.AddProduct(testBorrowProduct);

//            // Act
//            var products = borrowProductsService.GetProducts();

//            // Assert
//            Assert.That(products, Is.Not.Null);
//        }

//        [Test]
//        public void TestGetProducts_ReturnsCorrectNumberOfProducts()
//        {
//            // Arrange
//            borrowProductsRepositoryMock.AddProduct(testBorrowProduct);

//            // Act
//            var products = borrowProductsService.GetProducts();

//            // Assert
//            Assert.That(products.Count, Is.EqualTo(EXPECTED_SINGLE_COUNT));
//        }

//        [Test]
//        public void TestGetProducts_CallsRepositoryCorrectly()
//        {
//            // Arrange
//            borrowProductsRepositoryMock.AddProduct(testBorrowProduct);

//            // Act
//            borrowProductsService.GetProducts();

//            // Assert
//            Assert.That(borrowProductsRepositoryMock.GetGetProductsCount(), Is.EqualTo(EXPECTED_SINGLE_COUNT));
//        }

//        [Test]
//        public void TestGetProductById_ValidId_ReturnsNonNullProduct()
//        {
//            // Arrange
//            borrowProductsRepositoryMock.AddProduct(testBorrowProduct);

//            // Act
//            var product = borrowProductsService.GetProductById(testBorrowProduct.Id);

//            // Assert
//            Assert.That(product, Is.Not.Null);
//        }

//        [Test]
//        public void TestGetProductById_ValidId_ReturnsProductWithCorrectId()
//        {
//            // Arrange
//            borrowProductsRepositoryMock.AddProduct(testBorrowProduct);

//            // Act
//            var product = borrowProductsService.GetProductById(testBorrowProduct.Id);

//            // Assert
//            Assert.That(product.Id, Is.EqualTo(testBorrowProduct.Id));
//        }

//        [Test]
//        public void TestGetProductById_ValidId_CallsRepositoryCorrectly()
//        {
//            // Arrange
//            borrowProductsRepositoryMock.AddProduct(testBorrowProduct);

//            // Act
//            borrowProductsService.GetProductById(testBorrowProduct.Id);

//            // Assert
//            Assert.That(borrowProductsRepositoryMock.GetGetProductByIdCount(), Is.EqualTo(EXPECTED_SINGLE_COUNT));
//        }

//        [Test]
//        public void TestGetProductById_InvalidId_ReturnsNull()
//        {
//            // Act
//            var product = borrowProductsService.GetProductById(NON_EXISTENT_PRODUCT_ID);

//            // Assert
//            Assert.That(product, Is.Null);
//        }

//        [Test]
//        public void TestGetProductById_InvalidId_CallsRepositoryCorrectly()
//        {
//            // Act
//            borrowProductsService.GetProductById(NON_EXISTENT_PRODUCT_ID);

//            // Assert
//            Assert.That(borrowProductsRepositoryMock.GetGetProductByIdCount(), Is.EqualTo(EXPECTED_SINGLE_COUNT));
//        }
//    }
//}