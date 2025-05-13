//using System;
//using System.Collections.Generic;
//using NUnit.Framework;
//using MarketMinds.Shared.Models;
//using MarketMinds.Shared.Services;
//using MarketMinds.Views.Pages;
//using Microsoft.UI.Xaml;
//using Moq;
//using BusinessLogicLayer.ViewModel;

//namespace MarketMinds.Test.Services.ProductViewNavigationService
//{
//    [TestFixture]
//    public class ProductViewNavigationServiceTest
//    {
//        // Constants to replace magic values
//        private const int SELLER_ID = 1;
//        private const string SELLER_USERNAME = "TestSeller";
//        private const string SELLER_EMAIL = "test@example.com";
//        private const string ARGUMENT_NAME_PRODUCT = "product";
//        private const string ARGUMENT_NAME_SELLER = "seller";

//        private MarketMinds.Shared.Services.ProductViewNavigationService _navigationService;
//        private User _seller;
//        private BuyProduct _testProduct;

//        [SetUp]
//        public void Setup()
//        {
//            _navigationService = new MarketMinds.Shared.Services.ProductViewNavigationService();
//            _seller = new User(SELLER_ID, SELLER_USERNAME, SELLER_EMAIL);

//            // Initialize a test product if needed for non-null tests
//            _testProduct = CreateTestProduct();
//        }

//        #region CreateProductDetailView Tests

//        [Test]
//        public void CreateProductDetailView_WithNullProduct_ThrowsException()
//        {
//            // Act & Assert
//            var exception = Assert.Throws<ArgumentNullException>(() =>
//                _navigationService.CreateProductDetailView(null));

//            // Additional assertion can be in a separate test
//            Assert.That(exception, Is.Not.Null);
//        }

//        [Test]
//        public void CreateProductDetailView_WithNullProduct_HasCorrectParamName()
//        {
//            // Act
//            var exception = Assert.Throws<ArgumentNullException>(() =>
//                _navigationService.CreateProductDetailView(null));

//            // Assert
//            Assert.That(exception.ParamName, Is.EqualTo(ARGUMENT_NAME_PRODUCT));
//        }

//        #endregion

//        #region CreateSellerReviewsView Tests

//        [Test]
//        public void CreateSellerReviewsView_WithNullSeller_ThrowsException()
//        {
//            // Act & Assert
//            var exception = Assert.Throws<ArgumentNullException>(() =>
//                _navigationService.CreateSellerReviewsView(null));

//            // Additional assertion can be in a separate test
//            Assert.That(exception, Is.Not.Null);
//        }

//        [Test]
//        public void CreateSellerReviewsView_WithNullSeller_HasCorrectParamName()
//        {
//            // Act
//            var exception = Assert.Throws<ArgumentNullException>(() =>
//                _navigationService.CreateSellerReviewsView(null));

//            // Assert
//            Assert.That(exception.ParamName, Is.EqualTo(ARGUMENT_NAME_SELLER));
//        }

//        #endregion

//        #region Helper Methods

//        private BuyProduct CreateTestProduct()
//        {
//            // Create a minimal valid product for testing non-null scenarios
//            // This is a helper method that can be expanded if needed for additional tests
//            var category = new Category(1, "Test Category", "Test Category Description");
//            var condition = new Condition(1, "New", "Brand new item");

//            return new BuyProduct(
//                1,
//                "Test Product",
//                "Test Description",
//                _seller,
//                condition,
//                category,
//                new List<ProductTag>(),
//                new List<Image>(),
//                99.99f
//            );
//        }

//        #endregion
//    }
//}