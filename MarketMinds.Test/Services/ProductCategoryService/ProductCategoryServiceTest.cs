//using MarketMinds.Shared.Models;
//using MarketMinds.Shared.Services.ProductCategoryService;
//using NUnit.Framework;

//namespace MarketMinds.Test.Services.ProductCategoryService
//{
//    [TestFixture]
//    public class ProductCategoryServiceTests
//    {
//        // Constants to replace magic strings and numbers
//        private const int FIRST_CATEGORY_ID = 1;
//        private const string ELECTRONICS_TITLE = "Electronics";
//        private const string ELECTRONICS_DESCRIPTION = "Electronic devices";
//        private const string CLOTHING_TITLE = "Clothing";
//        private const string CLOTHING_DESCRIPTION = "Apparel items";
//        private const string TEST_CATEGORY_TITLE = "Test Category";
//        private const string TEST_CATEGORY_DESCRIPTION = "Test Description";
//        private const string CATEGORY_TO_DELETE_TITLE = "Category to Delete";
//        private const string CATEGORY_TO_DELETE_DESCRIPTION = "Description";
//        private const int EXPECTED_SINGLE_ITEM = 1;
//        private const int EXPECTED_TWO_ITEMS = 2;
//        private const int EXPECTED_ZERO_ITEMS = 0;

//        private ProductCategoryRepositoryMock _mockRepository;
//        private IProductCategoryService _service;

//        [SetUp]
//        public void Setup()
//        {
//            _mockRepository = new ProductCategoryRepositoryMock();
//            _service = new MarketMinds.Shared.Services.ProductCategoryService.ProductCategoryService(_mockRepository);
//        }

//        #region GetAllProductCategories Tests

//        [Test]
//        public void GetAllProductCategories_ReturnsCorrectNumberOfCategories()
//        {
//            // Arrange
//            AddTestCategories();

//            // Act
//            var result = _service.GetAllProductCategories();

//            // Assert
//            Assert.That(result.Count, Is.EqualTo(EXPECTED_TWO_ITEMS));
//        }

//        [Test]
//        public void GetAllProductCategories_ReturnsFirstCategoryWithCorrectTitle()
//        {
//            // Arrange
//            AddTestCategories();

//            // Act
//            var result = _service.GetAllProductCategories();

//            // Assert
//            Assert.That(result[0].DisplayTitle, Is.EqualTo(ELECTRONICS_TITLE));
//        }

//        [Test]
//        public void GetAllProductCategories_ReturnsSecondCategoryWithCorrectTitle()
//        {
//            // Arrange
//            AddTestCategories();

//            // Act
//            var result = _service.GetAllProductCategories();

//            // Assert
//            Assert.That(result[1].DisplayTitle, Is.EqualTo(CLOTHING_TITLE));
//        }

//        #endregion

//        #region CreateProductCategory Tests

//        [Test]
//        public void CreateProductCategory_ReturnsNewCategoryWithCorrectTitle()
//        {
//            // Act
//            var result = _service.CreateProductCategory(TEST_CATEGORY_TITLE, TEST_CATEGORY_DESCRIPTION);

//            // Assert
//            Assert.That(result.DisplayTitle, Is.EqualTo(TEST_CATEGORY_TITLE));
//        }

//        [Test]
//        public void CreateProductCategory_ReturnsNewCategoryWithCorrectDescription()
//        {
//            // Act
//            var result = _service.CreateProductCategory(TEST_CATEGORY_TITLE, TEST_CATEGORY_DESCRIPTION);

//            // Assert
//            Assert.That(result.Description, Is.EqualTo(TEST_CATEGORY_DESCRIPTION));
//        }

//        [Test]
//        public void CreateProductCategory_ReturnsNewCategoryWithCorrectId()
//        {
//            // Act
//            var result = _service.CreateProductCategory(TEST_CATEGORY_TITLE, TEST_CATEGORY_DESCRIPTION);

//            // Assert
//            Assert.That(result.Id, Is.EqualTo(FIRST_CATEGORY_ID));
//        }

//        [Test]
//        public void CreateProductCategory_AddsNewCategoryToRepository()
//        {
//            // Act
//            _service.CreateProductCategory(TEST_CATEGORY_TITLE, TEST_CATEGORY_DESCRIPTION);

//            // Assert
//            Assert.That(_mockRepository.Categories.Count, Is.EqualTo(EXPECTED_SINGLE_ITEM));
//        }

//        #endregion

//        #region DeleteProductCategory Tests

//        [Test]
//        public void DeleteProductCategory_RemovesCategoryFromList()
//        {
//            // Arrange
//            AddCategoryToDelete();
//            Assert.That(_mockRepository.Categories.Count, Is.EqualTo(EXPECTED_SINGLE_ITEM),
//                "Precondition: Repository should have exactly one category before deletion");

//            // Act
//            _service.DeleteProductCategory(CATEGORY_TO_DELETE_TITLE);

//            // Assert
//            Assert.That(_mockRepository.Categories.Count, Is.EqualTo(EXPECTED_ZERO_ITEMS));
//        }

//        #endregion

//        #region Helper Methods

//        private void AddTestCategories()
//        {
//            _mockRepository.Categories.Add(new Category(FIRST_CATEGORY_ID, ELECTRONICS_TITLE, ELECTRONICS_DESCRIPTION));
//            _mockRepository.Categories.Add(new Category(SECOND_CATEGORY_ID, CLOTHING_TITLE, CLOTHING_DESCRIPTION));
//        }

//        private void AddCategoryToDelete()
//        {
//            _mockRepository.Categories.Add(new Category(FIRST_CATEGORY_ID, CATEGORY_TO_DELETE_TITLE, CATEGORY_TO_DELETE_DESCRIPTION));
//        }

//        #endregion
//    }
//}