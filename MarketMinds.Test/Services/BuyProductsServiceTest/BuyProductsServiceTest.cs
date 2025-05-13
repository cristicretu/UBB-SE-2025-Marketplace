using System;
using System.Collections.Generic;
using NUnit.Framework;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.BuyProductsService;
using MarketMinds.Test.Services.BuyProductsServiceTest;
using MarketMinds.Shared.ProxyRepository;


namespace MarketMinds.Tests.Services.BuyProductsServiceTest
{
    [TestFixture]
    public class BuyProductsServiceTest
    {
        private const int TEST_SELLER_ID = 1;
        private const int TEST_BUY_PRODUCT_ID = 1;
        private const int TEST_INVALID_PRODUCT_ID = 2;
        private const int NON_EXISTENT_PRODUCT_ID = 999;
        private const float TEST_PRODUCT_PRICE = 99.99f;
        private const float TEST_INVALID_PRODUCT_PRICE = 100.0f;
        private const int AUCTION_DAYS_LENGTH = 7;

        private BuyProductsService buyProductsService;
        private BuyProductsProxyRepositoryMock buyProductsRepositoryMock;
        
        private User testSeller;
        private BuyProduct testBuyProduct;
        private AuctionProduct testInvalidProduct;

        [SetUp]
        public void Setup()
        {
            buyProductsRepositoryMock = new BuyProductsProxyRepositoryMock();
            buyProductsService = new BuyProductsService(buyProductsRepositoryMock); // This should now work


            testSeller = new User
            {
                Id = TEST_SELLER_ID,
                Username = "Test Seller",
                Email = "seller@test.com"
            };

            var testCondition = new Condition(1, "New", "Brand new item");
            var testCategory = new Category(1, "Electronics", "Electronic devices");
            var testTags = new List<ProductTag>();
            var testImages = new List<Image>();

            testBuyProduct = new BuyProduct(
                TEST_BUY_PRODUCT_ID,
                "Test Buy Product",
                "Test Description",
                testSeller,
                testCondition,
                testCategory,
                testTags,
                testImages,
                TEST_PRODUCT_PRICE);

            testInvalidProduct = new AuctionProduct(
                TEST_INVALID_PRODUCT_ID,
                "Test Auction Product",
                "Test Description",
                testSeller,
                testCondition,
                testCategory,
                testTags,
                testImages,
                DateTime.Now,
                DateTime.Now.AddDays(AUCTION_DAYS_LENGTH),
                TEST_INVALID_PRODUCT_PRICE);
        }

        [Test]
        public void TestCreateListing_ValidProduct_AddsProduct()
        {
            Assert.DoesNotThrow(() => buyProductsService.CreateListing(testBuyProduct));
        }



        [Test]
        public void TestDeleteListing_ValidProduct_DeletesProduct()
        {
            Assert.DoesNotThrow(() => buyProductsService.DeleteListing(testBuyProduct));
        }

        [Test]
        public void TestGetProducts_ReturnsList()
        {
            var result = buyProductsService.GetSortedFilteredProducts(null, null, null, null, null);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestGetProductById_ValidId_ReturnsProduct()
        {
            string idAsString = TEST_BUY_PRODUCT_ID.ToString();
            var result = buyProductsService.GetProductById(TEST_BUY_PRODUCT_ID);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestGetProductById_InvalidId_ThrowsKeyNotFound()
        {
            Assert.Throws<KeyNotFoundException>(() => buyProductsService.GetProductById(NON_EXISTENT_PRODUCT_ID));
        }
    }
}
