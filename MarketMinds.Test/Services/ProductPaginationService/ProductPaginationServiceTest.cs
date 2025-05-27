using MarketMinds.Shared.Services;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

public class TestProduct
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }

    public TestProduct(
        int id,
        string name,
        decimal price,
        string description,
        string category)
    {
        Id = id;
        Name = name;
        Price = price;
        Description = description;
        Category = category;
    }
    public TestProduct() { }
}

namespace MarketMinds.Test.Services.ProductPaginationService
{
    [TestFixture]
    public class ProductPaginationServiceTest
    {
        private IProductPaginationService _productPaginationService;
        private const int CUSTOM_ITEMS_PER_PAGE = 5;

        [SetUp]
        public void Setup()
        {
            _productPaginationService = new MarketMinds.Shared.Services.ProductPaginationService.ProductPaginationService(CUSTOM_ITEMS_PER_PAGE);
        }

        #region GetPaginatedProducts Tests

        [Test]
        public void GetPaginatedProducts_NullProductList_ThrowsArgumentNullException()
        {
            // Arrange
            List<TestProduct> products = null;

            // Act & a  
            Assert.Throws<ArgumentNullException>(() => _productPaginationService.GetPaginatedProducts(products, 1));
        }
        [Test]
        public void GetPaginatedProducts_CurrentPageLessThanOne_ThrowsArgumentException()
        {
            // Arrange
            var products = GetTestProducts(10);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _productPaginationService.GetPaginatedProducts(products, 0));
        }
        [Test]
        public void GetPaginatedProducts_CurrentPageExceedsTotalPages_ReturnsLastPage()
        {
            // Arrange
            var products = GetTestProducts(10);
            // Act
            var result = _productPaginationService.GetPaginatedProducts(products, 3);
            // Assert
            Assert.That(result.CurrentPage.Select(p => p.Id).ToList(), Is.EqualTo(new List<int> { 6, 7, 8, 9, 10 }));
        }
        [Test]
        public void GetPaginatedProdcts_EmptyList_ReturnsEmptyPage()
        {
            // Arrange
            var products = new List<TestProduct>();

            // Act
            var result = _productPaginationService.GetPaginatedProducts(products, 1);

            // Assert
            Assert.That(result.CurrentPage, Is.Empty);
        }
        [Test]
        public void GetPaginatedProducts_EmptyList_ReturnsZeroTotalPages()
        {
            // Arrange
            var products = new List<TestProduct>();
            // Act
            var result = _productPaginationService.GetPaginatedProducts(products, 1);
            // Assert
            Assert.That(result.TotalPages, Is.EqualTo(0));
        }
        [Test]
        public void GetPaginatedProducts_FewerThanItemsPerPage_ReturnsAllProducts()
        {
            // Arrange
            var products = GetTestProducts(3);

            // Act
            var result = _productPaginationService.GetPaginatedProducts(products, 1);

            // Assert
            Assert.That(result.CurrentPage.Count, Is.EqualTo(products.Count));
        }
        [Test]
        public void GetPaginatedProducts_FewerThanItemsPerPage_ReturnsOneTotalPage()
        {
            // Arrange
            var products = GetTestProducts(3);
            // Act
            var result = _productPaginationService.GetPaginatedProducts(products, 1);

            // Assert
            Assert.That(result.TotalPages, Is.EqualTo(1));
        }
        [Test]
        public void GetPaginatedProducts_ExactlyItemsPerPage_ReturnsAllProducts()
        {
            // Arrange
            var products = GetTestProducts(5);

            // Act
            var result = _productPaginationService.GetPaginatedProducts(products, 1);

            // Assert
            Assert.That(result.CurrentPage.Count, Is.EqualTo(products.Count));
        }
        [Test]
        public void GetPaginatedProducts_ExactlyItemsPerPage_ReturnsOneTotalPage()
        {
            // Arrange
            var products = GetTestProducts(5);
            // Act
            var result = _productPaginationService.GetPaginatedProducts(products, 1);
            // Assert
            Assert.That(result.TotalPages, Is.EqualTo(1));
        }
        [Test]
        public void GetPaginatedProducts_MoreThanItemsPerPage_ReturnsCorrectPage()
        {
            // Arrange
            var products = GetTestProducts(10);

            // Act
            var result = _productPaginationService.GetPaginatedProducts(products, 1);

            // Assert
            Assert.That(result.CurrentPage.Count, Is.EqualTo(CUSTOM_ITEMS_PER_PAGE));
        }
        [Test]
        public void GetPaginatedProducts_MoreThanItemsPerPage_ReturnsCorrectTotalPages()
        {
            // Arrange
            var products = GetTestProducts(10);

            // Act
            var result = _productPaginationService.GetPaginatedProducts(products, 1);

            // Assert
            Assert.That(result.TotalPages, Is.EqualTo(2));
        }
        [Test]
        public void GetPaginatedProducts_MoreThanItemsPerPage_FirstPageCorrect()
        {
            // Arrange
            var products = GetTestProducts(10);
            // Act
            var result = _productPaginationService.GetPaginatedProducts(products, 1);
            // Assert
            Assert.That(result.CurrentPage.Select(p => p.Id).ToList(), Is.EqualTo(new List<int> { 1, 2, 3, 4, 5 }));
        }
        [Test]
        public void GetPaginatedProducts_MoreThanItemsPerPage_SecondPageCorrect()
        {
            // Arrange
            var products = GetTestProducts(10);
            // Act
            var result = _productPaginationService.GetPaginatedProducts(products, 2);
            // Assert
            Assert.That(result.CurrentPage.Select(p => p.Id).ToList(), Is.EqualTo(new List<int> { 6, 7, 8, 9, 10 }));
        }

        #endregion

        #region ApplyFilters Tests
        [Test]
        public void ApplyFilters_NullProductList_ThrowsArgumentNullException()
        {
            // Arrange
            List<TestProduct> products = null;
            Func<TestProduct, bool> filter = p => p.Category == "Category 1";

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _productPaginationService.ApplyFilters(products, filter));
        }
        [Test]
        public void ApplyFilters_NullFilter_ThrowsArgumentNullException()
        {
            // Arrange
            var products = GetTestProducts(10);
            Func<TestProduct, bool> filter = null;
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _productPaginationService.ApplyFilters(products, filter));
        }
        [Test]
        public void ApplyFilters_EmptyProductList_ReturnsEmptyList()
        {
            // Arrange
            var products = new List<TestProduct>();
            Func<TestProduct, bool> filter = p => p.Category == "Category 1";

            // Act
            var result = _productPaginationService.ApplyFilters(products, filter);

            // Assert
            Assert.That(result, Is.Empty);
        }
        [Test]
        public void ApplyFilters_NoMatchingProducts_ReturnsEmptyList()
        {
            // Arrange
            var products = GetTestProducts(10);
            Func<TestProduct, bool> filter = p => p.Category == "NonExistentCategory";
            // Act
            var result = _productPaginationService.ApplyFilters(products, filter);
            // Assert
            Assert.That(result, Is.Empty);
        }
        [Test]
        public void ApplyFilters_MatchingProducts_ReturnsFilteredList()
        {
            // Arrange
            var products = GetTestProducts(10);
            Func<TestProduct, bool> filter = p => p.Category == "TestCategory";
            // Act
            var result = _productPaginationService.ApplyFilters(products, filter);
            // Assert
            Assert.That(result.Count, Is.EqualTo(products.Count));
        }
        [Test]
        public void ApplyFilters_MatchingSomeProducts_ReturnsFilteredList()
        {
            // Arrange
            var products = GetTestProducts(10);
            Func<TestProduct, bool> filter = p => p.Id <= 5;
            // Act
            var result = _productPaginationService.ApplyFilters(products, filter);
            // Assert
            Assert.That(result.Count, Is.EqualTo(5));
        }

        #endregion

        #region HELPER METHODS

        public List<TestProduct> GetTestProducts(int count)
        {
            var products = new List<TestProduct>();
            for (int i = 1; i <= count; i++)
            {
                products.Add(new TestProduct
                {
                    Id = i,
                    Name = $"Product {i}",
                    Price = i * 10,
                    Description = $"Description for product {i}",
                    Category = "TestCategory"
                });
            }
            return products;
        }

        #endregion
    }
}
