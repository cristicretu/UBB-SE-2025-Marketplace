//using MarketMinds.Shared.Models;
//using MarketMinds.Repositories;
//using System.Collections.Generic;
//using System.Linq;

//namespace MarketMinds.Test.Services.ProductTagService
//{
//    internal class ProductRepositoryMock : IProductsRepository
//    {
//        public List<Product> Products { get; private set; }

//        public ProductRepositoryMock()
//        {
//            Products = new List<Product>();
//        }

//        public List<Product> GetProducts()
//        {
//            return Products;
//        }

//        public Product GetProductByID(int id)
//        {
//            return Products.FirstOrDefault(p => p.Id == id);
//        }

//        public void AddProduct(Product product)
//        {
//            Products.Add(product);
//        }

//        public void UpdateProduct(Product product)
//        {
//            var existingProduct = Products.FirstOrDefault(p => p.Id == product.Id);
//            if (existingProduct != null)
//            {
//                Products.Remove(existingProduct);
//                Products.Add(product);
//            }
//        }

//        public void DeleteProduct(Product product)
//        {
//            var existingProduct = Products.FirstOrDefault(p => p.Id == product.Id);
//            if (existingProduct != null)
//            {
//                Products.Remove(existingProduct);
//            }
//        }
//    }
//}


using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.ProxyRepository;

namespace MarketMinds.Tests.Mocks
{
    public class MockProductRepository : BuyProductsProxyRepository
    {
        private readonly Dictionary<int, Product> _productStore = new();
        private readonly Dictionary<int, string> _sellerNames = new();
        private readonly List<BorrowProduct> _borrowableProducts = new();

        public MockProductRepository() : base(null) { }

        public  Task<string> GetProductByIdAsync(int productId)
        {
            if (_productStore.TryGetValue(productId, out var product))
            {
                return Task.FromResult(JsonSerializer.Serialize(product));
            }
            return Task.FromResult<string>(null);
        }

        public Task<string> GetSellerNameAsync(int? sellerId)
        {
            if (sellerId.HasValue && _sellerNames.TryGetValue(sellerId.Value, out var name))
            {
                return Task.FromResult(name);
            }
            return Task.FromResult<string>(null);
        }

        public Task<List<BorrowProduct>> GetBorrowableProductsAsync()
        {
            return Task.FromResult(_borrowableProducts);
        }

        // Helpers for test setup
        public void AddProduct(Product product) => _productStore[product.Id] = product;
        public void AddSellerName(int id, string name) => _sellerNames[id] = name;
        public void AddBorrowableProduct(BorrowProduct product) => _borrowableProducts.Add(product);
    }
}
