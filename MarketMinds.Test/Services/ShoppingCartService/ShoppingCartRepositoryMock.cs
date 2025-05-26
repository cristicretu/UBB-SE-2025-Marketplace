using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Models;

namespace MarketMinds.Tests.Mocks
{
    public class MockShoppingCartRepository : IShoppingCartRepository
    {
        // buyerId -> (productId -> quantity)
        private readonly Dictionary<int, Dictionary<int, int>> _cart = new();

        public Task AddProductToCartAsync(int buyerId, int productId, int quantity)
        {
            if (!_cart.ContainsKey(buyerId))
                _cart[buyerId] = new Dictionary<int, int>();

            _cart[buyerId][productId] = _cart[buyerId].TryGetValue(productId, out var q)
                                       ? q + quantity
                                       : quantity;
            return Task.CompletedTask;
        }

        public Task RemoveProductFromCartAsync(int buyerId, int productId)
        {
            if (_cart.TryGetValue(buyerId, out var products))
                products.Remove(productId);
            return Task.CompletedTask;
        }

        public Task UpdateProductQuantityAsync(int buyerId, int productId, int quantity)
        {
            if (_cart.TryGetValue(buyerId, out var products) && products.ContainsKey(productId))
                products[productId] = quantity;
            return Task.CompletedTask;
        }

        public Task<bool> IsProductInCartAsync(int buyerId, int productId)
        {
            return Task.FromResult(_cart.TryGetValue(buyerId, out var products) && products.ContainsKey(productId));
        }

        public Task<int> GetProductQuantityAsync(int buyerId, int productId)
        {
            if (_cart.TryGetValue(buyerId, out var products) && products.TryGetValue(productId, out var q))
                return Task.FromResult(q);
            return Task.FromResult(0);
        }

        public Task<List<Product>> GetCartItemsAsync(int buyerId)
        {
            var list = new List<Product>();
            if (_cart.TryGetValue(buyerId, out var products))
            {
                foreach (var kv in products)
                {
                    list.Add(new Product
                    {
                        Id = kv.Key,
                        Price = kv.Key * 1.0,    // for testing, price = productId
                        Stock = kv.Value        // use Stock field as quantity
                    });
                }
            }
            return Task.FromResult(list);
        }

        public Task<int> GetCartItemCountAsync(int buyerId)
        {
            if (_cart.TryGetValue(buyerId, out var products))
                return Task.FromResult(products.Values.Sum());
            return Task.FromResult(0);
        }

        public Task ClearCartAsync(int buyerId)
        {
            _cart.Remove(buyerId);
            return Task.CompletedTask;
        }
    }
}
