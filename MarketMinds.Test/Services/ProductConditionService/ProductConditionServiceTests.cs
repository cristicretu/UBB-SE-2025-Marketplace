using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using NUnit.Framework;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.ProductConditionService;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.ProxyRepository;
using Microsoft.Extensions.Configuration;

namespace MarketMinds.Test.Services.ProductConditionServiceTests
{
    [TestFixture]
    public class ProductConditionServiceTests
    {
        private FieldInfo _repoField;

        [SetUp]
        public void Setup()
        {
            _repoField = typeof(ProductConditionService)
                .GetField("productConditionRepository", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [Test]
        public void Constructor_ThrowsArgumentException_WhenRepositoryNotProxyRepo()
        {
            var dummy = new DummyRepo();
            var ex = Assert.Throws<ArgumentException>(() => new ProductConditionService(dummy));
            Assert.That(ex.Message, Is.EqualTo("Repository must be of type ProductConditionProxyRepository"));
        }

        [Test]
        public void Constructor_Succeeds_WhenRepositoryIsProxyRepo()
        {
            var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> { { "ApiSettings:BaseUrl", "http://localhost:5000/" } })
                .Build();
            var proxy = new ProductConditionProxyRepository(config);
            var service = new ProductConditionService(proxy);
            Assert.That(service, Is.Not.Null);
        }

        private ProductConditionService CreateServiceWithRepo(IProductConditionRepository repo)
        {
            var service = (ProductConditionService)FormatterServices.GetUninitializedObject(typeof(ProductConditionService));
            _repoField.SetValue(service, repo);
            return service;
        }

        private class DummyRepo : IProductConditionRepository
        {
            public List<Condition> GetAllProductConditions() => throw new NotImplementedException();
            public Condition CreateProductCondition(string displayTitle, string description) => throw new NotImplementedException();
            public void DeleteProductCondition(string displayTitle) => throw new NotImplementedException();
            public string GetAllProductConditionsRaw() => throw new NotImplementedException();
            public string CreateProductConditionRaw(string displayTitle, string description) => throw new NotImplementedException();
            public void DeleteProductConditionRaw(string displayTitle) => throw new NotImplementedException();
        }

        private class DelegateRepo : IProductConditionRepository
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

            public List<Condition> GetAllProductConditions() => throw new NotImplementedException();
            public Condition CreateProductCondition(string displayTitle, string description) => throw new NotImplementedException();
            public void DeleteProductCondition(string displayTitle) => throw new NotImplementedException();
            public string GetAllProductConditionsRaw() => _getAllRaw();
            public string CreateProductConditionRaw(string displayTitle, string description) => _createRaw(displayTitle, description);
            public void DeleteProductConditionRaw(string displayTitle) => _deleteRaw(displayTitle);
        }

        [Test]
        public void GetAllProductConditions_ReturnsList_WhenJsonValid()
        {
            var json = "[ { \"id\": 1, \"name\": \"C1\", \"description\": \"D1\" }, { \"id\": 2, \"name\": \"C2\", \"description\": \"D2\" } ]";
            var repo = new DelegateRepo(
                () => json,
                (title, desc) => throw new NotImplementedException(),
                title => throw new NotImplementedException()
            );
            var service = CreateServiceWithRepo(repo);

            var result = service.GetAllProductConditions();

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0].Id, Is.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("C1"));
            Assert.That(result[0].Description, Is.EqualTo("D1"));
            Assert.That(result[1].Id, Is.EqualTo(2));
            Assert.That(result[1].Name, Is.EqualTo("C2"));
            Assert.That(result[1].Description, Is.EqualTo("D2"));
        }

        [Test]
        public void GetAllProductConditions_ReturnsEmpty_WhenJsonArrayEmpty()
        {
            var repo = new DelegateRepo(
                () => "[]",
                (title, desc) => throw new NotImplementedException(),
                title => throw new NotImplementedException()
            );
            var service = CreateServiceWithRepo(repo);
            var result = service.GetAllProductConditions();
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetAllProductConditions_ThrowsInvalidOperationException_WhenJsonInvalid()
        {
            var repo = new DelegateRepo(
                () => "bad json",
                (title, desc) => throw new NotImplementedException(),
                title => throw new NotImplementedException()
            );
            var service = CreateServiceWithRepo(repo);

            var ex = Assert.Throws<InvalidOperationException>(() => service.GetAllProductConditions());
            Assert.That(ex.Message, Does.StartWith("Error retrieving product conditions:"));
        }

        [Test]
        public void CreateProductCondition_ThrowsArgumentException_WhenTitleNullOrWhiteSpace()
        {
            var repo = new DelegateRepo(
                () => throw new NotImplementedException(),
                (title, desc) => throw new NotImplementedException(),
                title => throw new NotImplementedException()
            );
            var service = CreateServiceWithRepo(repo);
            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentException>(() => service.CreateProductCondition(null!, "desc"));
                Assert.Throws<ArgumentException>(() => service.CreateProductCondition("  ", "desc"));
            });
        }

        [Test]
        public void CreateProductCondition_ThrowsArgumentException_WhenTitleTooLong()
        {
            var longTitle = new string('x', 101);
            var repo = new DelegateRepo(
                () => throw new NotImplementedException(),
                (title, desc) => throw new NotImplementedException(),
                title => throw new NotImplementedException()
            );
            var service = CreateServiceWithRepo(repo);

            var ex = Assert.Throws<ArgumentException>(() => service.CreateProductCondition(longTitle, "desc"));
            Assert.That(ex.Message, Is.EqualTo("Condition title cannot exceed 100 characters. (Parameter 'displayTitle')"));
        }

        [Test]
        public void CreateProductCondition_ThrowsArgumentException_WhenDescriptionTooLong()
        {
            var longDesc = new string('d', 501);
            var repo = new DelegateRepo(
                () => throw new NotImplementedException(),
                (title, desc) => throw new NotImplementedException(),
                title => throw new NotImplementedException()
            );
            var service = CreateServiceWithRepo(repo);

            var ex = Assert.Throws<ArgumentException>(() => service.CreateProductCondition("t", longDesc));
            Assert.That(ex.Message, Is.EqualTo("Condition description cannot exceed 500 characters. (Parameter 'description')"));
        }

        [Test]
        public void CreateProductCondition_ReturnsCondition_WhenJsonValid()
        {
            var json = "{ \"id\": 5, \"name\": \"Cond\", \"description\": \"Desc\" }";
            var repo = new DelegateRepo(
                () => throw new NotImplementedException(),
                (title, desc) => json,
                title => throw new NotImplementedException()
            );
            var service = CreateServiceWithRepo(repo);

            var cond = service.CreateProductCondition("Cond", "Desc");
            Assert.That(cond.Id, Is.EqualTo(5));
            Assert.That(cond.Name, Is.EqualTo("Cond"));
            Assert.That(cond.Description, Is.EqualTo("Desc"));
        }

        [Test]
        public void CreateProductCondition_ReturnsDefault_WhenFieldsMissing()
        {
            var repo = new DelegateRepo(
                () => throw new NotImplementedException(),
                (title, desc) => "{}",
                title => throw new NotImplementedException()
            );
            var service = CreateServiceWithRepo(repo);

            var cond = service.CreateProductCondition("x", null!);
            Assert.That(cond.Id, Is.EqualTo(0));
            Assert.That(cond.Name, Is.EqualTo(string.Empty));
            Assert.That(cond.Description, Is.EqualTo(string.Empty));
        }

        [Test]
        public void CreateProductCondition_ThrowsInvalidOperationException_WhenJsonInvalid()
        {
            var repo = new DelegateRepo(
                () => throw new NotImplementedException(),
                (title, desc) => "invalid",
                title => throw new NotImplementedException()
            );
            var service = CreateServiceWithRepo(repo);

            var ex = Assert.Throws<InvalidOperationException>(() => service.CreateProductCondition("t", "d"));
            Assert.That(ex.Message, Does.StartWith("Error creating product condition:"));
        }

        [Test]
        public void DeleteProductCondition_ThrowsArgumentException_WhenTitleNullOrWhiteSpace()
        {
            var repo = new DelegateRepo(
                () => throw new NotImplementedException(),
                (title, desc) => throw new NotImplementedException(),
                title => throw new NotImplementedException()
            );
            var service = CreateServiceWithRepo(repo);
            Assert.Multiple(() =>
            {
                Assert.Throws<ArgumentException>(() => service.DeleteProductCondition(null!));
                Assert.Throws<ArgumentException>(() => service.DeleteProductCondition("  "));
            });
        }

        [Test]
        public void DeleteProductCondition_CallsRepo_WhenTitleValid()
        {
            var called = false;
            var repo = new DelegateRepo(
                () => throw new NotImplementedException(),
                (title, desc) => throw new NotImplementedException(),
                title => called = true
            );
            var service = CreateServiceWithRepo(repo);
            service.DeleteProductCondition("ok");
            Assert.That(called, Is.True);
        }

        [Test]
        public void DeleteProductCondition_SwallowsKeyNotFoundException()
        {
            var repo = new DelegateRepo(
                () => throw new NotImplementedException(),
                (title, desc) => throw new NotImplementedException(),
                title => throw new KeyNotFoundException()
            );
            var service = CreateServiceWithRepo(repo);
            Assert.DoesNotThrow(() => service.DeleteProductCondition("x"));
        }

        [Test]
        public void DeleteProductCondition_ThrowsInvalidOperationException_OnGenericError()
        {
            var repo = new DelegateRepo(
                () => throw new NotImplementedException(),
                (title, desc) => throw new NotImplementedException(),
                title => throw new Exception("Oops")
            );
            var service = CreateServiceWithRepo(repo);
            var ex = Assert.Throws<InvalidOperationException>(() => service.DeleteProductCondition("x"));
            Assert.That(ex.Message, Is.EqualTo("Error deleting product condition: Oops"));
        }

        [Test]
        public void DeleteProductCondition_ThrowsInvalidOperationException_OnForeignKeyReference()
        {
            var repo = new DelegateRepo(
                () => throw new NotImplementedException(),
                (title, desc) => throw new NotImplementedException(),
                title => throw new Exception("Some REFERENCE constraint violation")
            );
            var service = CreateServiceWithRepo(repo);
            var ex = Assert.Throws<InvalidOperationException>(() => service.DeleteProductCondition("ref"));
            Assert.That(ex.Message, Is.EqualTo("Cannot delete condition 'ref' because it is being used by one or more products."));
        }
    }
}
