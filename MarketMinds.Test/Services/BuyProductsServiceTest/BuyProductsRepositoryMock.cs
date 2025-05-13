using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.ProxyRepository;
using Microsoft.Extensions.Configuration;

namespace MarketMinds.Test.Services.BuyProductsServiceTest
{
    public class BuyProductsProxyRepositoryMock : BuyProductsProxyRepository
    {
        private readonly Dictionary<int, BuyProduct> _mockDb = new();
        private int _nextId = 1;

        public BuyProductsProxyRepositoryMock() : base(new ConfigurationBuilder().Build())
        {
            var product = new BuyProduct
            {
                Id = _nextId++,
                Title = "Test Product",
                Description = "Mocked product",
                Seller = new User { Id = 1, Username = "Seller", Email = "seller@test.com" },
                SellerId = 1,
                Condition = new Condition(1, "New", "Brand new"),
                Category = new Category(1, "MockCat", "MockDesc"),
                Tags = new List<ProductTag>(),
                Images = new List<BuyProductImage>(),
                Price = 10.0f
            };

            _mockDb[product.Id] = product;
        }

        public virtual string GetProducts()
        {
            return JsonSerializer.Serialize(_mockDb.Values.ToList());
        }

        public virtual string CreateListing(object productToSend)
        {
            var json = JsonSerializer.Serialize(productToSend);
            var product = JsonSerializer.Deserialize<BuyProduct>(json);
            if (product == null) throw new InvalidOperationException("Invalid product data.");

            product.Id = _nextId++;
            _mockDb[product.Id] = product;
            return JsonSerializer.Serialize(product);
        }

        public virtual void DeleteListing(int id)
        {
            if (!_mockDb.Remove(id))
            {
                throw new KeyNotFoundException($"Product ID {id} not found.");
            }
        }

        public virtual string GetProductById(int id)
        {
            if (_mockDb.TryGetValue(id, out var product))
            {
                return JsonSerializer.Serialize(product);
            }

            throw new KeyNotFoundException($"Product ID {id} not found.");
        }

        // Optional helper for test setup
        public void AddProduct(BuyProduct product)
        {
            _mockDb[product.Id] = product;
        }
    }
}