using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Services.ProductCategoryService;
using MarketMinds.Shared.ProxyRepository;

namespace MarketMinds.Test.Services.ProductCategoryServiceTests
{
    [TestFixture]
    public class ProductCategoryServiceTests
    {
        private FieldInfo _repoField;

        [SetUp]
        public void Setup()
        {
            _repoField = typeof(ProductCategoryService)
                .GetField("productCategoryRepository", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [Test]
        public void Constructor_ThrowsArgumentException_WhenRepositoryNotProxyRepo()
        {
            var dummyRepo = new DummyRepo();

            var ex = Assert.Throws<ArgumentException>(() => new ProductCategoryService(dummyRepo));
            Assert.That(ex.Message, Is.EqualTo("Repository must be of type ProductCategoryProxyRepository"));
        }

        [Test]
        public void Constructor_Succeeds_WhenRepositoryIsProductCategoryProxyRepository()
        {
            var config = new ConfigurationBuilder().AddInMemoryCollection(
                new Dictionary<string, string> { { "ApiSettings:BaseUrl", "http://localhost:5000/" } }
            ).Build();
            var proxyRepo = new ProductCategoryProxyRepository(config);

            var service = new ProductCategoryService(proxyRepo);

            Assert.That(service, Is.Not.Null);
        }

        private ProductCategoryService CreateServiceWithRepo(IProductCategoryRepository repo)
        {
            var service = (ProductCategoryService)FormatterServices.GetUninitializedObject(typeof(ProductCategoryService));
            _repoField.SetValue(service, repo);
            return service;
        }

        [Test]
        public void GetAllProductCategories_ReturnsCategories_WhenJsonIsValid()
        {
            string json = "[ { \"id\": 1, \"name\": \"Cat1\", \"description\": \"Desc1\" }, { \"id\": 2, \"name\": \"Cat2\", \"description\": \"Desc2\" } ]";
            var stubRepo = new DelegateRepo(
                () => json,
                (name, desc) => throw new NotImplementedException(),
                name => throw new NotImplementedException()
            );
            var service = CreateServiceWithRepo(stubRepo);

            var result = service.GetAllProductCategories();

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].Id, Is.EqualTo(1));
            Assert.That(result[0].DisplayTitle, Is.EqualTo("Cat1"));
            Assert.That(result[0].Description, Is.EqualTo("Desc1"));
            Assert.That(result[1].Id, Is.EqualTo(2));
            Assert.That(result[1].DisplayTitle, Is.EqualTo("Cat2"));
            Assert.That(result[1].Description, Is.EqualTo("Desc2"));
        }

        [Test]
        public void GetAllProductCategories_ReturnsEmptyList_WhenJsonArrayEmpty()
        {
            string json = "[]";
            var stubRepo = new DelegateRepo(
                () => json,
                (name, desc) => throw new NotImplementedException(),
                name => throw new NotImplementedException()
            );
            var service = CreateServiceWithRepo(stubRepo);

            var result = service.GetAllProductCategories();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetAllProductCategories_ThrowsInvalidOperationException_WhenJsonInvalid()
        {
            var stubRepo = new DelegateRepo(
                () => "invalid json",
                (name, desc) => throw new NotImplementedException(),
                name => throw new NotImplementedException()
            );
            var service = CreateServiceWithRepo(stubRepo);

            var ex = Assert.Throws<InvalidOperationException>(() => service.GetAllProductCategories());
            Assert.That(ex.Message, Does.StartWith("Error getting product categories:"));
        }

        [Test]
        public void CreateProductCategory_ThrowsArgumentException_WhenNameNullOrWhiteSpace()
        {
            var stubRepo = new DelegateRepo(
                () => throw new NotImplementedException(),
                (name, desc) => throw new NotImplementedException(),
                name => throw new NotImplementedException()
            );
            var service = CreateServiceWithRepo(stubRepo);

            Assert.Throws<ArgumentException>(() => service.CreateProductCategory(null!, "desc"));
            Assert.Throws<ArgumentException>(() => service.CreateProductCategory("  ", "desc"));
        }

        [Test]
        public void CreateProductCategory_ThrowsArgumentException_WhenNameTooLong()
        {
            var longName = new string('a', 101);
            var stubRepo = new DelegateRepo(
                () => throw new NotImplementedException(),
                (name, desc) => throw new NotImplementedException(),
                name => throw new NotImplementedException()
            );
            var service = CreateServiceWithRepo(stubRepo);

            var ex = Assert.Throws<ArgumentException>(() => service.CreateProductCategory(longName, "desc"));
            Assert.That(ex.Message, Is.EqualTo("Category name cannot exceed 100 characters. (Parameter 'name')"));
        }

        [Test]
        public void CreateProductCategory_ThrowsArgumentException_WhenDescriptionTooLong()
        {
            var longDesc = new string('a', 501);
            var stubRepo = new DelegateRepo(
                () => throw new NotImplementedException(),
                (name, desc) => throw new NotImplementedException(),
                name => throw new NotImplementedException()
            );
            var service = CreateServiceWithRepo(stubRepo);

            var ex = Assert.Throws<ArgumentException>(() => service.CreateProductCategory("name", longDesc));
            Assert.That(ex.Message, Is.EqualTo("Category description cannot exceed 500 characters. (Parameter 'description')"));
        }

        [Test]
        public void CreateProductCategory_ReturnsCategory_WhenJsonValid()
        {
            string json = "{ \"id\": 3, \"name\": \"TestCat\", \"description\": \"TestDesc\" }";
            var stubRepo = new DelegateRepo(
                () => throw new NotImplementedException(),
                (name, desc) => json,
                name => throw new NotImplementedException()
            );
            var service = CreateServiceWithRepo(stubRepo);

            var category = service.CreateProductCategory("TestCat", "TestDesc");

            Assert.That(category.Id, Is.EqualTo(3));
            Assert.That(category.DisplayTitle, Is.EqualTo("TestCat"));
            Assert.That(category.Description, Is.EqualTo("TestDesc"));
        }

        [Test]
        public void CreateProductCategory_ReturnsDefaultCategory_WhenFieldsMissing()
        {
            string json = "{}";
            var stubRepo = new DelegateRepo(
                () => throw new NotImplementedException(),
                (name, desc) => json,
                name => throw new NotImplementedException()
            );
            var service = CreateServiceWithRepo(stubRepo);

            var category = service.CreateProductCategory("any", null!);

            Assert.That(category.Id, Is.EqualTo(0));
            Assert.That(category.DisplayTitle, Is.EqualTo(string.Empty));
            Assert.That(category.Description, Is.EqualTo(string.Empty));
        }

        [Test]
        public void CreateProductCategory_ThrowsInvalidOperationException_WhenJsonInvalid()
        {
            var stubRepo = new DelegateRepo(
                () => throw new NotImplementedException(),
                (name, desc) => "bad json",
                name => throw new NotImplementedException()
            );
            var service = CreateServiceWithRepo(stubRepo);

            var ex = Assert.Throws<InvalidOperationException>(() => service.CreateProductCategory("name", "desc"));
            Assert.That(ex.Message, Does.StartWith("Error creating product category:"));
        }

        [Test]
        public void DeleteProductCategory_ThrowsArgumentException_WhenNameNullOrWhiteSpace()
        {
            var stubRepo = new DelegateRepo(
                () => throw new NotImplementedException(),
                (name, desc) => throw new NotImplementedException(),
                name => throw new NotImplementedException()
            );
            var service = CreateServiceWithRepo(stubRepo);

            Assert.Throws<ArgumentException>(() => service.DeleteProductCategory(null!));
            Assert.Throws<ArgumentException>(() => service.DeleteProductCategory("  "));
        }

        [Test]
        public void DeleteProductCategory_CallsRepository_WhenNameValid()
        {
            bool called = false;
            var stubRepo = new DelegateRepo(
                () => throw new NotImplementedException(),
                (name, desc) => throw new NotImplementedException(),
                name => called = true
            );
            var service = CreateServiceWithRepo(stubRepo);

            service.DeleteProductCategory("name");

            Assert.That(called, Is.True);
        }

        [Test]
        public void DeleteProductCategory_ThrowsKeyNotFoundException_WhenRepositoryThrowsKeyNotFound()
        {
            var stubRepo = new DelegateRepo(
                () => throw new NotImplementedException(),
                (name, desc) => throw new NotImplementedException(),
                name => throw new KeyNotFoundException()
            );
            var service = CreateServiceWithRepo(stubRepo);

            Assert.Throws<KeyNotFoundException>(() => service.DeleteProductCategory("name"));
        }

        [Test]
        public void DeleteProductCategory_ThrowsInvalidOperationException_WhenRepositoryThrowsOtherException()
        {
            var stubRepo = new DelegateRepo(
                () => throw new NotImplementedException(),
                (name, desc) => throw new NotImplementedException(),
                name => throw new Exception("Oops")
            );
            var service = CreateServiceWithRepo(stubRepo);

            var ex = Assert.Throws<InvalidOperationException>(() => service.DeleteProductCategory("name"));
            Assert.That(ex.Message, Is.EqualTo("Error deleting product category: Oops"));
        }

        private class DummyRepo : IProductCategoryRepository
        {
            public List<Category> GetAllProductCategories() => throw new NotImplementedException();
            public Category CreateProductCategory(string displayTitle, string description) => throw new NotImplementedException();
            public void DeleteProductCategory(string displayTitle) => throw new NotImplementedException();
            public string GetAllProductCategoriesRaw() => throw new NotImplementedException();
            public string CreateProductCategoryRaw(string displayTitle, string description) => throw new NotImplementedException();
            public void DeleteProductCategoryRaw(string displayTitle) => throw new NotImplementedException();
        }

        private class DelegateRepo : IProductCategoryRepository
        {
            private readonly Func<string> _getAllRaw;
            private readonly Func<string, string, string> _createRaw;
            private readonly Action<string> _deleteRaw;

            public DelegateRepo(Func<string> getAllRaw, Func<string, string, string> createRaw, Action<string> deleteRaw)
            {
                _getAllRaw = getAllRaw;
                _createRaw = createRaw;
                _deleteRaw = deleteRaw;
            }

            public List<Category> GetAllProductCategories() => throw new NotImplementedException();
            public Category CreateProductCategory(string displayTitle, string description) => throw new NotImplementedException();
            public void DeleteProductCategory(string displayTitle) => throw new NotImplementedException();
            public string GetAllProductCategoriesRaw() => _getAllRaw();
            public string CreateProductCategoryRaw(string displayTitle, string description) => _createRaw(displayTitle, description);
            public void DeleteProductCategoryRaw(string displayTitle) => _deleteRaw(displayTitle);
        }
    }
}
