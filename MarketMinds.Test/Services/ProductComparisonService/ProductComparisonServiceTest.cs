//using System;
//using Xunit;
//using MarketMinds.Shared.Models;
//using MarketMinds.Shared.Services.ProductComparisonService;

//namespace MarketMinds.Tests.Services.ProductComparisonService
//{
//    public class ProductComparisonServiceTests
//    {
//        private readonly IProductComparisonService _service;

//        public ProductComparisonServiceTests()
//        {
//            _service = new ProductComparisonService();
//        }

//        [Fact]
//        public void AddProduct_Should_Set_LeftProduct_If_LeftIsNull()
//        {
//            // Arrange
//            Product left = null;
//            Product right = new Product { Id = 2, Name = "Right" };
//            Product newProduct = new Product { Id = 1, Name = "New" };

//            // Act
//            var result = _service.AddProduct(left, right, newProduct);

//            // Assert
//            Assert.Equal(newProduct, result.LeftProduct);
//            Assert.Equal(right, result.RightProduct);
//            Assert.False(result.IsComplete);
//        }

//        [Fact]
//        public void AddProduct_Should_Set_RightProduct_If_LeftIsNotNull_And_NewIsDifferent()
//        {
//            // Arrange
//            Product left = new Product { Id = 1, Name = "Left" };
//            Product right = null;
//            Product newProduct = new Product { Id = 2, Name = "New" };

//            // Act
//            var result = _service.AddProduct(left, right, newProduct);

//            // Assert
//            Assert.Equal(left, result.LeftProduct);
//            Assert.Equal(newProduct, result.RightProduct);
//            Assert.True(result.IsComplete);
//        }

//        [Fact]
//        public void AddProduct_Should_DoNothing_If_NewEqualsLeft()
//        {
//            // Arrange
//            Product left = new Product { Id = 1, Name = "Same" };
//            Product right = new Product { Id = 2, Name = "Right" };
//            Product newProduct = left; // Same reference

//            // Act
//            var result = _service.AddProduct(left, right, newProduct);

//            // Assert
//            Assert.Equal(left, result.LeftProduct);
//            Assert.Equal(right, result.RightProduct);
//            Assert.False(result.IsComplete);
//        }
//    }
//}
