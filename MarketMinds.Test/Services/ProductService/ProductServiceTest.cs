//using MarketMinds.Shared.Models;
//using NUnit.Framework;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace MarketMinds.Test.Services.ProductTagService
//{
//    [TestFixture]
//    internal class ProductServiceTests
//    {
//        // Constants to replace magic strings and numbers
//        private const int PRODUCT_ID1 = 1;
//        private const int PRODUCT_ID2 = 2;
//        private const int PRODUCT_ID3 = 3;
//        private const int PRODUCT_ID4 = 4;
//        private const int CATEGORY_ID1 = 1;
//        private const int CATEGORY_ID2 = 2;
//        private const int CONDITION_ID1 = 1;
//        private const int CONDITION_ID2 = 2;
//        private const int TAG_ID1 = 1;
//        private const int TAG_ID2 = 2;

//        // Product titles
//        private const string PRODUCT_TITLE_LAPTOP = "Laptop";
//        private const string PRODUCT_TITLE_SMARTPHONE = "Smartphone";
//        private const string PRODUCT_TITLE_TABLET = "Tablet";
//        private const string PRODUCT_TITLE_HEADPHONES = "Headphones";
//        private const string PRODUCT_TITLE_KEYBOARD = "Keyboard";
//        private const string PRODUCT_TITLE_PRODUCT1 = "Product1";
//        private const string PRODUCT_TITLE_PRODUCT2 = "Product2";
//        private const string PRODUCT_TITLE_PRODUCT3 = "Product3";
//        private const string PRODUCT_TITLE_PRODUCT_A = "Product A";
//        private const string PRODUCT_TITLE_PRODUCT_B = "Product B";
//        private const string PRODUCT_TITLE_PRODUCT_C = "Product C";
//        private const string PRODUCT_TITLE_GAMING_COMPUTER = "Gaming Computer";
//        private const string PRODUCT_TITLE_WIRELESS_MOUSE = "Wireless Mouse";
//        private const string PRODUCT_TITLE_WIRED_KEYBOARD = "Wired Keyboard";
//        private const string PRODUCT_TITLE_USED_LAPTOP = "Used Laptop";
//        private const string PRODUCT_TITLE_OFFICE_KEYBOARD = "Office Keyboard";
//        private const string PRODUCT_TITLE_OFFICE_CHAIR = "Office Chair";
//        private const string PRODUCT_TITLE_GAMING_MOUSE = "Gaming Mouse";
//        private const string PRODUCT_TITLE_GAMING_LAPTOP = "Gaming Laptop";
//        private const string PRODUCT_TITLE_BUSINESS_DESKTOP = "Business Desktop";
//        private const string PRODUCT_TITLE_GRAPHIC_TABLET = "Graphic Tablet";
//        private const string PRODUCT_TITLE_INITIAL_NAME = "Initial Name";
//        private const string PRODUCT_TITLE_UPDATED_NAME = "Updated Name";
//        private const string PRODUCT_TITLE_LAPTOP_COMPUTER = "LAPTOP Computer";
//        private const string PRODUCT_TITLE_DESKTOP_COMPUTER = "Desktop computer";
//        private const string PRODUCT_TITLE_PRODUCT_1_WITH_CATEGORY_2 = "Product 1";
//        private const string PRODUCT_TITLE_PRODUCT_2_WITH_CATEGORY_1 = "Product 2";
//        private const string PRODUCT_TITLE_PRODUCT_3_WITH_CATEGORY_1 = "Product 3";

//        // Category titles
//        private const string CATEGORY_ELECTRONICS_TITLE = "Electronics";
//        private const string CATEGORY_ELECTRONICS_DESCRIPTION = "Electronic devices";
//        private const string CATEGORY_CLOTHING_TITLE = "Clothing";
//        private const string CATEGORY_CLOTHING_DESCRIPTION = "Apparel";
//        private const string CATEGORY_DEFAULT_TITLE = "Default Category";
//        private const string CATEGORY_DEFAULT_DESCRIPTION = "Default category description";
//        private const string CATEGORY_A_CATEGORY_TITLE = "A Category";
//        private const string CATEGORY_A_DESCRIPTION = "First category";
//        private const string CATEGORY_B_TITLE = "B Category";
//        private const string CATEGORY_B_DESCRIPTION = "Second category";

//        // Condition titles
//        private const string CONDITION_NEW_TITLE = "New";
//        private const string CONDITION_NEW_DESCRIPTION = "Brand new";
//        private const string CONDITION_USER_TITLE = "Used";
//        private const string CONDITION_USED_ITEM = "Used item";
//        private const string CONDITION_DEFAULT_TITLE = "New";
//        private const string CONDITION_DEFAULT_DESCRIPTION = "Unused product in original packaging";

//        // Tag titles
//        private const string TAG_WIRELESS_TITLE = "Wireless";
//        private const string TAG_BLUETOOTH_TITLE = "Bluetooth";
//        private const string TAG_OFFICE_TITLE = "Office";
//        private const string TAG_GAMING_TITLE = "Gaming";

//        // Search queries
//        private const string SEARCH_QUERY_COMPUTER = "computer";
//        private const string SEARCH_QUERY_WIRELESS = "wireless";
//        private const string SEARCH_QUERY_TOP = "top";

//        // Sort field names
//        private const string SORT_FIELD_ID_EXTERNAL_NAME = "Id";
//        private const string SORT_FIELD_ID_INTERNAL_NAME = "Id";
//        private const string SORT_FIELD_TITLE_EXTERNAL_NAME = "Title";
//        private const string SORT_FIELD_TITLE_INTERNAL_NAME = "Title";
//        private const string SORT_FIELD_CATEGORY_EXTERNAL_NAME = "CategoryName";
//        private const string SORT_FIELD_CATEGORY_SERVICE = "Category.DisplayTitle";

//        // Common test values
//        private const string DEFAULT_PRODUCT_DESCRIPTION = "Test product description";
//        private const int EXPECTED_ITEM_COUNT0 = 0;
//        private const int EXPECTED_ITEM_COUNT1 = 1;
//        private const int EXPECTED_ITEM_COUNT2 = 2;
//        private const int EXPECTED_ITEM_COUNT3 = 3;

//        private class TestProduct : Product
//        {
//        }

//        #region Helper Methods

//        private ProductRepositoryMock CreateMockRepository()
//        {
//            return new ProductRepositoryMock();
//        }

//        private MarketMinds.Shared.Services.ProductTagService.ProductService CreateProductService(ProductRepositoryMock repository)
//        {
//            return new MarketMinds.Shared.Services.ProductTagService.ProductService(repository);
//        }

//        private Product CreateSampleProduct(int id, string title, Category category = null, Condition condition = null)
//        {
//            return new TestProduct
//            {
//                Id = id,
//                Title = title,
//                Description = DEFAULT_PRODUCT_DESCRIPTION,
//                Category = category ?? new Category(CATEGORY_ID1, CATEGORY_DEFAULT_TITLE, CATEGORY_DEFAULT_DESCRIPTION),
//                Condition = condition ?? new Condition(CONDITION_ID1, CONDITION_DEFAULT_TITLE, CONDITION_DEFAULT_DESCRIPTION),
//                Tags = new List<ProductTag>(),
//                Images = new List<Image>()
//            };
//        }

//        #endregion

//        #region GetProducts Tests

//        [Test]
//        public void GetProducts_ReturnsCorrectNumberOfProducts()
//        {
//            // Arrange
//            var mockRepository = CreateMockRepository();
//            var productService = CreateProductService(mockRepository);
//            AddThreeSampleProducts(mockRepository);

//            // Act
//            var result = productService.GetProducts();

//            // Assert
//            Assert.That(result.Count, Is.EqualTo(EXPECTED_ITEM_COUNT3));
//        }

//        [Test]
//        public void GetProducts_ContainsFirstProduct()
//        {
//            // Arrange
//            var mockRepository = CreateMockRepository();
//            var productService = CreateProductService(mockRepository);
//            AddThreeSampleProducts(mockRepository);

//            // Act
//            var result = productService.GetProducts();

//            // Assert
//            Assert.That(result.Any(p => p.Id == PRODUCT_ID1 && p.Title == PRODUCT_TITLE_LAPTOP), Is.True);
//        }

//        [Test]
//        public void GetProducts_ContainsSecondProduct()
//        {
//            // Arrange
//            var mockRepository = CreateMockRepository();
//            var productService = CreateProductService(mockRepository);
//            AddThreeSampleProducts(mockRepository);

//            // Act
//            var result = productService.GetProducts();

//            // Assert
//            Assert.That(result.Any(p => p.Id == PRODUCT_ID2 && p.Title == PRODUCT_TITLE_SMARTPHONE), Is.True);
//        }

//        [Test]
//        public void GetProducts_ContainsThirdProduct()
//        {
//            // Arrange
//            var mockRepository = CreateMockRepository();
//            var productService = CreateProductService(mockRepository);
//            AddThreeSampleProducts(mockRepository);

//            // Act
//            var result = productService.GetProducts();

//            // Assert
//            Assert.That(result.Any(p => p.Id == PRODUCT_ID3 && p.Title == PRODUCT_TITLE_TABLET), Is.True);
//        }

//        private void AddThreeSampleProducts(ProductRepositoryMock mockRepository)
//        {
//            var product1 = CreateSampleProduct(PRODUCT_ID1, PRODUCT_TITLE_LAPTOP);
//            var product2 = CreateSampleProduct(PRODUCT_ID2, PRODUCT_TITLE_SMARTPHONE);
//            var product3 = CreateSampleProduct(PRODUCT_ID3, PRODUCT_TITLE_TABLET);

//            mockRepository.AddProduct(product1);
//            mockRepository.AddProduct(product2);
//            mockRepository.AddProduct(product3);
//        }

//        #endregion

//        #region GetProductById Tests

//        [Test]
//        public void GetProductById_ReturnsNonNullProduct()
//        {
//            // Arrange
//            var mockRepository = CreateMockRepository();
//            var productService = CreateProductService(mockRepository);
//            AddTwoSampleProducts(mockRepository);

//            // Act
//            var result = productService.GetProductById(PRODUCT_ID2);

//            // Assert
//            Assert.That(result, Is.Not.Null);
//        }

//        [Test]
//        public void GetProductById_ReturnsProductWithCorrectId()
//        {
//            // Arrange
//            var mockRepository = CreateMockRepository();
//            var productService = CreateProductService(mockRepository);
//            AddTwoSampleProducts(mockRepository);

//            // Act
//            var result = productService.GetProductById(PRODUCT_ID2);

//            // Assert
//            Assert.That(result.Id, Is.EqualTo(PRODUCT_ID2));
//        }

//        [Test]
//        public void GetProductById_ReturnsProductWithCorrectTitle()
//        {
//            // Arrange
//            var mockRepository = CreateMockRepository();
//            var productService = CreateProductService(mockRepository);
//            AddTwoSampleProducts(mockRepository);

//            // Act
//            var result = productService.GetProductById(PRODUCT_ID2);

//            // Assert
//            Assert.That(result.Title, Is.EqualTo(PRODUCT_TITLE_SMARTPHONE));
//        }

//        private void AddTwoSampleProducts(ProductRepositoryMock mockRepository)
//        {
//            var product1 = CreateSampleProduct(PRODUCT_ID1, PRODUCT_TITLE_LAPTOP);
//            var product2 = CreateSampleProduct(PRODUCT_ID2, PRODUCT_TITLE_SMARTPHONE);

//            mockRepository.AddProduct(product1);
//            mockRepository.AddProduct(product2);
//        }

//        #endregion

//        #region AddProduct Tests

//        [Test]
//        public void AddProduct_AddsProductToRepository()
//        {
//            // Arrange
//            var mockRepository = CreateMockRepository();
//            var productService = CreateProductService(mockRepository);
//            var product = CreateSampleProduct(PRODUCT_ID1, PRODUCT_TITLE_HEADPHONES);

//            // Act
//            productService.AddProduct(product);

//            // Assert
//            Assert.That(mockRepository.Products.Count, Is.EqualTo(EXPECTED_ITEM_COUNT1));
//        }

//        [Test]
//        public void AddProduct_AddsProductWithCorrectId()
//        {
//            // Arrange
//            var mockRepository = CreateMockRepository();
//            var productService = CreateProductService(mockRepository);
//            var product = CreateSampleProduct(PRODUCT_ID1, PRODUCT_TITLE_HEADPHONES);

//            // Act
//            productService.AddProduct(product);

//            // Assert
//            Assert.That(mockRepository.Products[0].Id, Is.EqualTo(PRODUCT_ID1));
//        }

//        [Test]
//        public void AddProduct_AddsProductWithCorrectTitle()
//        {
//            // Arrange
//            var mockRepository = CreateMockRepository();
//            var productService = CreateProductService(mockRepository);
//            var product = CreateSampleProduct(PRODUCT_ID1, PRODUCT_TITLE_HEADPHONES);

//            // Act
//            productService.AddProduct(product);

//            // Assert
//            Assert.That(mockRepository.Products[0].Title, Is.EqualTo(PRODUCT_TITLE_HEADPHONES));
//        }

//        #endregion

//        #region DeleteProduct Tests

//        [Test]
//        public void DeleteProduct_RemovesProductFromRepository()
//        {
//            // Arrange
//            var mockRepository = CreateMockRepository();
//            var productService = CreateProductService(mockRepository);
//            var product = CreateSampleProduct(PRODUCT_ID1, PRODUCT_TITLE_KEYBOARD);

//            mockRepository.AddProduct(product);
//            Assert.That(mockRepository.Products.Count, Is.EqualTo(EXPECTED_ITEM_COUNT1),
//                "Precondition: Repository should have one product before deletion");

//            // Act
//            productService.DeleteProduct(product);

//            // Assert
//            Assert.That(mockRepository.Products.Count, Is.EqualTo(EXPECTED_ITEM_COUNT0));
//        }

//        #endregion

//        #region UpdateProduct Tests

//        [Test]
//        public void UpdateProduct_UpdatesProductInRepository()
//        {
//            // Arrange
//            var mockRepository = CreateMockRepository();
//            var productService = CreateProductService(mockRepository);

//            var productToUpdate = CreateSampleProduct(PRODUCT_ID1, PRODUCT_TITLE_INITIAL_NAME);
//            mockRepository.AddProduct(productToUpdate);
//            productToUpdate.Title = PRODUCT_TITLE_UPDATED_NAME;

//            // Act
//            productService.UpdateProduct(productToUpdate);

//            // Assert
//            var updatedProduct = mockRepository.Products.FirstOrDefault(p => p.Id == PRODUCT_ID1);
//            Assert.That(updatedProduct, Is.Not.Null);
//            Assert.That(updatedProduct.Title, Is.EqualTo(PRODUCT_TITLE_UPDATED_NAME));
//        }

//        [Test]
//        public void UpdateProduct_DoesNotAddMoreProducts()
//        {
//            // Arrange
//            var mockRepository = CreateMockRepository();
//            var productService = CreateProductService(mockRepository);

//            var productToUpdate = CreateSampleProduct(PRODUCT_ID1, PRODUCT_TITLE_INITIAL_NAME);
//            mockRepository.AddProduct(productToUpdate);
//            productToUpdate.Title = PRODUCT_TITLE_UPDATED_NAME;

//            // Act
//            productService.UpdateProduct(productToUpdate);

//            // Assert
//            Assert.That(mockRepository.Products.Count, Is.EqualTo(EXPECTED_ITEM_COUNT1));
//        }

//        #endregion

//        #region GetSortedFilteredProducts - Basic Tests

//        [Test]
//        public void GetSortedFilteredProducts_EmptyProductList_ReturnsEmptyList()
//        {
//            // Arrange
//            var products = new List<Product>();

//            // Act
//            var result = MarketMinds.Shared.Services.ProductTagService.ProductService.GetSortedFilteredProducts(
//                products, null, null, null, null, null);

//            // Assert
//            Assert.That(result.Count, Is.EqualTo(EXPECTED_ITEM_COUNT0));
//        }

//        [Test]
//        public void GetSortedFilteredProducts_WithNullFirstParameter_ShouldThrowException()
//        {
//            // Act & Assert
//            Assert.Throws<NullReferenceException>(() =>
//                MarketMinds.Shared.Services.ProductTagService.ProductService.GetSortedFilteredProducts(
//                    null, null, null, null, null, null));
//        }

//        #endregion

//        #region GetSortedFilteredProducts - Filtering by Condition Tests

//        [Test]
//        public void GetSortedFilteredProducts_WithMultipleConditions_ReturnsCorrectNumberOfProducts()
//        {
//            // Arrange
//            var condition1 = new Condition(CONDITION_ID1, CONDITION_NEW_TITLE, CONDITION_NEW_DESCRIPTION);
//            var condition2 = new Condition(CONDITION_ID2, CONDITION_USER_TITLE, CONDITION_USED_ITEM);
//            var products = CreateProductsWithDifferentConditions(condition1, condition2);
//            var selectedConditions = new List<Condition> { condition1, condition2 };

//            // Act
//            var result = MarketMinds.Shared.Services.ProductTagService.ProductService.GetSortedFilteredProducts(
//                products, selectedConditions, null, null, null, null);

//            // Assert
//            Assert.That(result.Count, Is.EqualTo(EXPECTED_ITEM_COUNT3));
//        }

//        private List<Product> CreateProductsWithDifferentConditions(
//            Condition condition1, Condition condition2)
//        {
//            var product1 = CreateSampleProduct(PRODUCT_ID1, PRODUCT_TITLE_PRODUCT1, condition: condition1);
//            var product2 = CreateSampleProduct(PRODUCT_ID2, PRODUCT_TITLE_PRODUCT2, condition: condition2);
//            var product3 = CreateSampleProduct(PRODUCT_ID3, PRODUCT_TITLE_PRODUCT3, condition: condition1);

//            return new List<Product> { product1, product2, product3 };
//        }

//        #endregion

//        #region GetSortedFilteredProducts - Filtering by Category Tests

//        [Test]
//        public void GetSortedFilteredProducts_WithMultipleCategories_ReturnsCorrectNumberOfProducts()
//        {
//            // Arrange
//            var category1 = new Category(CATEGORY_ID1, CATEGORY_ELECTRONICS_TITLE, CATEGORY_ELECTRONICS_DESCRIPTION);
//            var category2 = new Category(CATEGORY_ID2, CATEGORY_CLOTHING_TITLE, CATEGORY_CLOTHING_DESCRIPTION);
//            var products = CreateProductsWithDifferentCategories(category1, category2);
//            var selectedCategories = new List<Category> { category1, category2 };

//            // Act
//            var result = MarketMinds.Shared.Services.ProductTagService.ProductService.GetSortedFilteredProducts(
//                products, null, selectedCategories, null, null, null);

//            // Assert
//            Assert.That(result.Count, Is.EqualTo(EXPECTED_ITEM_COUNT3));
//        }

//        private List<Product> CreateProductsWithDifferentCategories(
//            Category category1, Category category2)
//        {
//            var product1 = CreateSampleProduct(PRODUCT_ID1, PRODUCT_TITLE_PRODUCT1, category: category1);
//            var product2 = CreateSampleProduct(PRODUCT_ID2, PRODUCT_TITLE_PRODUCT2, category: category2);
//            var product3 = CreateSampleProduct(PRODUCT_ID3, PRODUCT_TITLE_PRODUCT3, category: category1);

//            return new List<Product> { product1, product2, product3 };
//        }

//        #endregion

//        #region GetSortedFilteredProducts - Filtering by Tag Tests

//        [Test]
//        public void GetSortedFilteredProducts_WithMultipleTags_ReturnsCorrectNumberOfProducts()
//        {
//            // Arrange
//            var tag1 = new ProductTag(TAG_ID1, TAG_WIRELESS_TITLE);
//            var tag2 = new ProductTag(TAG_ID2, TAG_BLUETOOTH_TITLE);
//            var products = CreateProductsWithDifferentTags(tag1, tag2);
//            var selectedTags = new List<ProductTag> { tag1, tag2 };

//            // Act
//            var result = MarketMinds.Shared.Services.ProductTagService.ProductService.GetSortedFilteredProducts(
//                products, null, null, selectedTags, null, null);

//            // Assert
//            Assert.That(result.Count, Is.EqualTo(EXPECTED_ITEM_COUNT3));
//        }

//        private List<Product> CreateProductsWithDifferentTags(ProductTag tag1, ProductTag tag2)
//        {
//            var product1 = CreateSampleProduct(PRODUCT_ID1, PRODUCT_TITLE_PRODUCT1);
//            product1.Tags.Add(tag1);

//            var product2 = CreateSampleProduct(PRODUCT_ID2, PRODUCT_TITLE_PRODUCT2);
//            product2.Tags.Add(tag2);

//            var product3 = CreateSampleProduct(PRODUCT_ID3, PRODUCT_TITLE_PRODUCT3);
//            product3.Tags.Add(tag1);
//            product3.Tags.Add(tag2);

//            return new List<Product> { product1, product2, product3 };
//        }

//        #endregion

//        #region GetSortedFilteredProducts - Text Search Tests

//        [Test]
//        public void GetSortedFilteredProducts_WithCaseInsensitiveSearch_ReturnsCorrectNumberOfProducts()
//        {
//            // Arrange
//            var products = CreateProductsForTextSearch();

//            // Act
//            var result = MarketMinds.Shared.Services.ProductTagService.ProductService.GetSortedFilteredProducts(
//                products, null, null, null, null, SEARCH_QUERY_COMPUTER);

//            // Assert
//            Assert.That(result.Count, Is.EqualTo(EXPECTED_ITEM_COUNT2));
//        }

//        [Test]
//        public void GetSortedFilteredProducts_WithCaseInsensitiveSearch_ReturnsProductsContainingSearchTerm()
//        {
//            // Arrange
//            var products = CreateProductsForTextSearch();

//            // Act
//            var result = MarketMinds.Shared.Services.ProductTagService.ProductService.GetSortedFilteredProducts(
//                products, null, null, null, null, SEARCH_QUERY_COMPUTER);

//            // Assert
//            Assert.That(result.All(p => p.Title.ToLower().Contains(SEARCH_QUERY_COMPUTER)), Is.True);
//        }

//        [Test]
//        public void GetSortedFilteredProducts_WithPartialWordSearch_ReturnsCorrectNumberOfProducts()
//        {
//            // Arrange
//            var products = new List<Product>
//            {
//                CreateSampleProduct(PRODUCT_ID1, PRODUCT_TITLE_GAMING_LAPTOP),
//                CreateSampleProduct(PRODUCT_ID2, PRODUCT_TITLE_BUSINESS_DESKTOP),
//                CreateSampleProduct(PRODUCT_ID3, PRODUCT_TITLE_GRAPHIC_TABLET)
//            };

//            // Act
//            var result = MarketMinds.Shared.Services.ProductTagService.ProductService.GetSortedFilteredProducts(
//                products, null, null, null, null, SEARCH_QUERY_TOP);

//            // Assert
//            Assert.That(result.Count, Is.EqualTo(EXPECTED_ITEM_COUNT2));
//        }

//        [Test]
//        public void GetSortedFilteredProducts_WithPartialWordSearch_ReturnsProductsContainingSearchTerm()
//        {
//            // Arrange
//            var products = new List<Product>
//            {
//                CreateSampleProduct(PRODUCT_ID1, PRODUCT_TITLE_GAMING_LAPTOP),
//                CreateSampleProduct(PRODUCT_ID2, PRODUCT_TITLE_BUSINESS_DESKTOP),
//                CreateSampleProduct(PRODUCT_ID3, PRODUCT_TITLE_GRAPHIC_TABLET)
//            };

//            // Act
//            var result = MarketMinds.Shared.Services.ProductTagService.ProductService.GetSortedFilteredProducts(
//                products, null, null, null, null, SEARCH_QUERY_TOP);

//            // Assert
//            Assert.That(result.All(p => p.Title.ToLower().Contains(SEARCH_QUERY_TOP)), Is.True);
//        }

//        private List<Product> CreateProductsForTextSearch()
//        {
//            return new List<Product>
//            {
//                CreateSampleProduct(PRODUCT_ID1, PRODUCT_TITLE_LAPTOP_COMPUTER),
//                CreateSampleProduct(PRODUCT_ID2, PRODUCT_TITLE_DESKTOP_COMPUTER),
//                CreateSampleProduct(PRODUCT_ID3, PRODUCT_TITLE_TABLET)
//            };
//        }

//        #endregion

//        #region GetSortedFilteredProducts - Combined Filtering Tests

//        [Test]
//        public void GetSortedFilteredProducts_WithAllFilterTypes_ReturnsCorrectNumberOfProducts()
//        {
//            // Arrange
//            var (category, condition, tag, products) = SetupAllFilterTypesTest();

//            // Act
//            var result = MarketMinds.Shared.Services.ProductTagService.ProductService.GetSortedFilteredProducts(
//                products,
//                new List<Condition> { condition },
//                new List<Category> { category },
//                new List<ProductTag> { tag },
//                null,
//                SEARCH_QUERY_WIRELESS
//            );

//            // Assert
//            Assert.That(result.Count, Is.EqualTo(EXPECTED_ITEM_COUNT1));
//        }

//        [Test]
//        public void GetSortedFilteredProducts_WithAllFilterTypes_ReturnsCorrectProduct()
//        {
//            // Arrange
//            var (category, condition, tag, products) = SetupAllFilterTypesTest();

//            // Act
//            var result = MarketMinds.Shared.Services.ProductTagService.ProductService.GetSortedFilteredProducts(
//                products,
//                new List<Condition> { condition },
//                new List<Category> { category },
//                new List<ProductTag> { tag },
//                null,
//                SEARCH_QUERY_WIRELESS
//            );

//            // Assert
//            Assert.That(result[0].Title, Is.EqualTo(PRODUCT_TITLE_WIRELESS_MOUSE));
//        }

//        private (ProductCategory, ProductCondition, ProductTag, List<Product>) SetupAllFilterTypesTest()
//        {
//            var category = new Category(CATEGORY_ID1, CATEGORY_ELECTRONICS_TITLE, CATEGORY_ELECTRONICS_DESCRIPTION);
//            var condition = new Condition(CONDITION_ID1, CONDITION_NEW_TITLE, CONDITION_NEW_DESCRIPTION);
//            var tag = new ProductTag(TAG_ID1, TAG_WIRELESS_TITLE);

//            var product1 = CreateSampleProduct(PRODUCT_ID1, PRODUCT_TITLE_GAMING_COMPUTER, category, condition);
//            product1.Tags.Add(tag);

//            var product2 = CreateSampleProduct(PRODUCT_ID2, PRODUCT_TITLE_WIRELESS_MOUSE, category, condition);
//            product2.Tags.Add(tag);

//            var product3 = CreateSampleProduct(PRODUCT_ID3, PRODUCT_TITLE_WIRED_KEYBOARD, category, condition);

//            var product4 = CreateSampleProduct(PRODUCT_ID4, PRODUCT_TITLE_USED_LAPTOP, category,
//                new Condition(CONDITION_ID2, CONDITION_USER_TITLE, CONDITION_USED_ITEM));
//            product4.Tags.Add(tag);

//            var products = new List<Product> { product1, product2, product3, product4 };

//            return (category, condition, tag, products);
//        }

//        [Test]
//        public void GetSortedFilteredProducts_WithFilteringAndNullSort_FiltersCorrectly()
//        {
//            // Arrange
//            var condition = new Condition(CONDITION_ID1, CONDITION_NEW_TITLE, CONDITION_NEW_DESCRIPTION);
//            var products = CreateProductsForFilteringTest(condition);
//            var selectedConditions = new List<Condition> { condition };

//            // Act
//            var result = MarketMinds.Shared.Services.ProductTagService.ProductService.GetSortedFilteredProducts(
//                products, selectedConditions, null, null, null, null);

//            // Assert
//            Assert.That(result.Count, Is.EqualTo(EXPECTED_ITEM_COUNT2));
//        }

//        [Test]
//        public void GetSortedFilteredProducts_WithFilteringAndNullSort_ReturnsProductsWithMatchingCondition()
//        {
//            // Arrange
//            var condition = new Condition(CONDITION_ID1, CONDITION_NEW_TITLE, CONDITION_NEW_DESCRIPTION);
//            var products = CreateProductsForFilteringTest(condition);
//            var selectedConditions = new List<Condition> { condition };

//            // Act
//            var result = MarketMinds.Shared.Services.ProductTagService.ProductService.GetSortedFilteredProducts(
//                products, selectedConditions, null, null, null, null);

//            // Assert
//            Assert.That(result.All(p => p.Condition.Id == condition.Id), Is.True);
//        }

//        private List<Product> CreateProductsForFilteringTest(ProductCondition condition)
//        {
//            var product1 = CreateSampleProduct(PRODUCT_ID1, PRODUCT_TITLE_PRODUCT_A, condition: condition);
//            var product2 = CreateSampleProduct(PRODUCT_ID2, PRODUCT_TITLE_PRODUCT_B,
//                condition: new Condition(CONDITION_ID2, CONDITION_USER_TITLE, CONDITION_USED_ITEM));
//            var product3 = CreateSampleProduct(PRODUCT_ID3, PRODUCT_TITLE_PRODUCT_C, condition: condition);

//            return new List<Product> { product1, product2, product3 };
//        }

//        #endregion

//        #region GetSortedFilteredProducts - Sorting Tests

//        [Test]
//        public void GetSortedFilteredProducts_WithSortingId_SortsProductsCorrectly()
//        {
//            // Arrange
//            var products = CreateProductListForSorting();
//            var sortCondition = new ProductSortType(
//                SORT_FIELD_ID_EXTERNAL_NAME, SORT_FIELD_ID_INTERNAL_NAME, true);

//            // Act
//            var result = MarketMinds.Shared.Services.ProductTagService.ProductService.GetSortedFilteredProducts(
//                products, null, null, null, sortCondition, null);

//            // Assert
//            Assert.That(result.Count, Is.EqualTo(EXPECTED_ITEM_COUNT3));
//            Assert.That(result[0].Id, Is.EqualTo(PRODUCT_ID1));
//            Assert.That(result[1].Id, Is.EqualTo(PRODUCT_ID2));
//            Assert.That(result[2].Id, Is.EqualTo(PRODUCT_ID3));
//        }

//        [Test]
//        public void GetSortedFilteredProducts_WithSortingDescendingId_SortsProductsCorrectly()
//        {
//            // Arrange
//            var products = CreateProductListForSorting();
//            var sortCondition = new ProductSortType(
//                SORT_FIELD_ID_EXTERNAL_NAME, SORT_FIELD_ID_INTERNAL_NAME, false);

//            // Act
//            var result = MarketMinds.Shared.Services.ProductTagService.ProductService.GetSortedFilteredProducts(
//                products, null, null, null, sortCondition, null);

//            // Assert
//            Assert.That(result.Count, Is.EqualTo(EXPECTED_ITEM_COUNT3));
//            Assert.That(result[0].Id, Is.EqualTo(PRODUCT_ID3));
//            Assert.That(result[1].Id, Is.EqualTo(PRODUCT_ID2));
//            Assert.That(result[2].Id, Is.EqualTo(PRODUCT_ID1));
//        }

//        private List<Product> CreateProductListForSorting()
//        {
//            return new List<Product>
//            {
//                CreateSampleProduct(PRODUCT_ID1, PRODUCT_TITLE_PRODUCT_A),
//                CreateSampleProduct(PRODUCT_ID2, PRODUCT_TITLE_PRODUCT_B),
//                CreateSampleProduct(PRODUCT_ID3, PRODUCT_TITLE_PRODUCT_C)
//            };
//        }

//        [Test]
//        public void GetSortedFilteredProducts_WithSortingAndFiltering_FiltersThenSortsCorrectly()
//        {
//            // Arrange
//            var products = SetupSortingAndFilteringTest(out var tag1, out var selectedTags);
//            var sortCondition = new ProductSortType(
//                SORT_FIELD_TITLE_EXTERNAL_NAME, SORT_FIELD_TITLE_INTERNAL_NAME, true);

//            // Act
//            var result = MarketMinds.Shared.Services.ProductTagService.ProductService.GetSortedFilteredProducts(
//                products, null, null, selectedTags, sortCondition, null);

//            // Assert
//            Assert.That(result.Count, Is.EqualTo(EXPECTED_ITEM_COUNT2));
//            Assert.That(result[0].Title, Is.EqualTo(PRODUCT_TITLE_OFFICE_CHAIR));
//            Assert.That(result[1].Title, Is.EqualTo(PRODUCT_TITLE_OFFICE_KEYBOARD));
//        }

//        private List<Product> SetupSortingAndFilteringTest(
//            out ProductTag tag1, out List<ProductTag> selectedTags)
//        {
//            tag1 = new ProductTag(TAG_ID1, TAG_OFFICE_TITLE);
//            var tag2 = new ProductTag(TAG_ID2, TAG_GAMING_TITLE);

//            var product1 = CreateSampleProduct(PRODUCT_ID1, PRODUCT_TITLE_OFFICE_KEYBOARD);
//            product1.Tags.Add(tag1);

//            var product2 = CreateSampleProduct(PRODUCT_ID2, PRODUCT_TITLE_OFFICE_CHAIR);
//            product2.Tags.Add(tag1);

//            var product3 = CreateSampleProduct(PRODUCT_ID3, PRODUCT_TITLE_GAMING_MOUSE);
//            product3.Tags.Add(tag2);

//            selectedTags = new List<ProductTag> { tag1 };

//            return new List<Product> { product1, product2, product3 };
//        }

//        [Test]
//        public void GetSortedFilteredProducts_WithComplexSorting_HandlesComplexProperties()
//        {
//            // Arrange
//            var category1 = new Category(CATEGORY_ID1, CATEGORY_A_CATEGORY_TITLE, CATEGORY_A_DESCRIPTION);
//            var category2 = new Category(CATEGORY_ID2, CATEGORY_B_TITLE, CATEGORY_B_DESCRIPTION);

//            var product1 = CreateSampleProduct(PRODUCT_ID1, PRODUCT_TITLE_PRODUCT_1_WITH_CATEGORY_2, category2);
//            var product2 = CreateSampleProduct(PRODUCT_ID2, PRODUCT_TITLE_PRODUCT_2_WITH_CATEGORY_1, category1);
//            var product3 = CreateSampleProduct(PRODUCT_ID3, PRODUCT_TITLE_PRODUCT_3_WITH_CATEGORY_1, category1);

//            var products = new List<Product> { product1, product2, product3 };
//            var sortCondition = new ProductSortType(
//                SORT_FIELD_CATEGORY_EXTERNAL_NAME, SORT_FIELD_CATEGORY_SERVICE, true);

//            // Act
//            var result = MarketMinds.Shared.Services.ProductTagService.ProductService.GetSortedFilteredProducts(
//                products, null, null, null, sortCondition, null);

//            // Assert
//            Assert.That(result.Count, Is.EqualTo(EXPECTED_ITEM_COUNT3));
//        }

//        #endregion
//    }
//}

using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;
using MarketMinds.Tests.Mocks;

namespace MarketMinds.Tests.Services
{
    public class ProductServiceTest
    {
        private readonly MockProductRepository _mockRepo;
        private readonly ProductService _service;

        public ProductServiceTests()
        {
            _mockRepo = new MockProductRepository();
            _service = new ProductService(_mockRepo);
        }

        [Fact]
        public async Task GetProductByIdAsync_ExistingId_ReturnsProduct()
        {
            var product = new Product { Id = 1, Name = "Test Product", SellerId = 10, Price = 99.99 };
            _mockRepo.AddProduct(product);

            var result = await _service.GetProductByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(product.Id, result.Id);
            Assert.Equal(product.Name, result.Name);
        }

        [Fact]
        public async Task GetProductByIdAsync_NonExistingId_ReturnsNull()
        {
            var result = await _service.GetProductByIdAsync(999);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetSellerNameAsync_ValidId_ReturnsName()
        {
            _mockRepo.AddSellerName(5, "Alice");

            var result = await _service.GetSellerNameAsync(5);

            Assert.Equal("Alice", result);
        }

        [Fact]
        public async Task GetSellerNameAsync_InvalidId_ReturnsNull()
        {
            var result = await _service.GetSellerNameAsync(42);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetBorrowableProductsAsync_ReturnsList()
        {
            _mockRepo.AddBorrowableProduct(new BorrowProduct { Id = 1, Name = "Borrowable A", SellerId = 7 });
            _mockRepo.AddBorrowableProduct(new BorrowProduct { Id = 2, Name = "Borrowable B", SellerId = 8 });

            var results = await _service.GetBorrowableProductsAsync();

            Assert.Equal(2, results.Count);
            Assert.Contains(results, p => p.Name == "Borrowable A");
            Assert.Contains(results, p => p.Name == "Borrowable B");
        }

        [Fact]
        public void GetSortedFilteredProducts_AlwaysEmptyList()
        {
            var results = _service.GetSortedFilteredProducts(new(), new(), new(), ProductSortType.PriceAsc, "test");

            Assert.NotNull(results);
            Assert.Empty(results); 
        }
    }
}
