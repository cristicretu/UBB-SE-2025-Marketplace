// ShoppingCartServiceTests.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using MarketMinds.Shared.Services;
using MarketMinds.Tests.Mocks;
using MarketMinds.Shared.Models;

namespace MarketMinds.Tests.Services
{
    public class ShoppingCartServiceTests
    {
        private readonly MockShoppingCartRepository _repo;
        private readonly ShoppingCartService _service;

        public ShoppingCartServiceTests()
        {
            _repo = new MockShoppingCartRepository();
            _service = new ShoppingCartService(_repo);
        }

        [Theory]
        [InlineData(0, 1, 1)]
        [InlineData(1, 0, 1)]
        [InlineData(1, 1, 0)]
        public async Task AddProductToCartAsync_InvalidArgs_Throws(int buyerId, int productId, int qty)
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.AddProductToCartAsync(buyerId, productId, qty));
        }

        [Fact]
        public async Task AddProductToCartAsync_Valid_AddsQuantity()
        {
            await _service.AddProductToCartAsync(1, 2, 3);
            Assert.Equal(3, await _repo.GetProductQuantityAsync(1, 2));

            // adding again increments
            await _service.AddProductToCartAsync(1, 2, 2);
            Assert.Equal(5, await _repo.GetProductQuantityAsync(1, 2));
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        public async Task RemoveProductFromCartAsync_InvalidArgs_Throws(int b, int p)
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.RemoveProductFromCartAsync(b, p));
        }

        [Fact]
        public async Task RemoveProductFromCartAsync_Valid_RemovesItem()
        {
            await _repo.AddProductToCartAsync(1, 5, 2);
            await _service.RemoveProductFromCartAsync(1, 5);
            Assert.False(await _repo.IsProductInCartAsync(1, 5));
        }

        [Theory]
        [InlineData(0, 1, 1)]
        [InlineData(1, 0, 1)]
        [InlineData(1, 1, 0)]
        public async Task UpdateProductQuantityAsync_InvalidArgs_Throws(int b, int p, int q)
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateProductQuantityAsync(b, p, q));
        }

        [Fact]
        public async Task UpdateProductQuantityAsync_Valid_Updates()
        {
            await _repo.AddProductToCartAsync(2, 3, 4);
            await _service.UpdateProductQuantityAsync(2, 3, 7);
            Assert.Equal(7, await _repo.GetProductQuantityAsync(2, 3));
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        public async Task IncrementProductQuantityAsync_InvalidArgs_Throws(int b, int p)
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.IncrementProductQuantityAsync(b, p));
        }

        [Fact]
        public async Task IncrementProductQuantityAsync_NotInCart_AddsOne()
        {
            await _service.IncrementProductQuantityAsync(3, 9);
            Assert.Equal(1, await _repo.GetProductQuantityAsync(3, 9));
        }

        [Fact]
        public async Task IncrementProductQuantityAsync_InCart_Increments()
        {
            await _repo.AddProductToCartAsync(4, 8, 2);
            await _service.IncrementProductQuantityAsync(4, 8);
            Assert.Equal(3, await _repo.GetProductQuantityAsync(4, 8));
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        public async Task DecrementProductQuantityAsync_InvalidArgs_Throws(int b, int p)
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.DecrementProductQuantityAsync(b, p));
        }

        [Fact]
        public async Task DecrementProductQuantityAsync_QuantityGreaterThanOne_Decrements()
        {
            await _repo.AddProductToCartAsync(5, 7, 3);
            await _service.DecrementProductQuantityAsync(5, 7);
            Assert.Equal(2, await _repo.GetProductQuantityAsync(5, 7));
        }

        [Fact]
        public async Task DecrementProductQuantityAsync_QuantityEqualsOne_Removes()
        {
            await _repo.AddProductToCartAsync(6, 4, 1);
            await _service.DecrementProductQuantityAsync(6, 4);
            Assert.False(await _repo.IsProductInCartAsync(6, 4));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetCartItemsAsync_InvalidBuyer_Throws(int b)
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetCartItemsAsync(b));
        }

        [Fact]
        public async Task GetCartItemsAsync_Valid_ReturnsList()
        {
            await _repo.AddProductToCartAsync(7, 2, 2);
            await _repo.AddProductToCartAsync(7, 3, 1);
            var items = await _service.GetCartItemsAsync(7);
            Assert.Equal(2, items.Count);
            Assert.Contains(items, p => p.Id == 2 && p.Stock == 2);
        }

        [Fact]
        public async Task GetProductPriceAsync_ProductMissing_Throws()
        {
            await Assert.ThrowsAsync<Exception>(() => _service.GetProductPriceAsync(8, 99));
        }

        [Fact]
        public async Task GetProductPriceAsync_ProductExists_ReturnsPrice()
        {
            // price = productId in mock
            await _repo.AddProductToCartAsync(9, 11, 1);
            var price = await _service.GetProductPriceAsync(9, 11);
            Assert.Equal(11.0, price);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        public async Task GetCartTotalAsync_InvalidBuyer_Throws(int b, int p)
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetCartTotalAsync(b));
        }

        [Fact]
        public async Task GetCartTotalAsync_Valid_ComputesSum()
        {
            // price=Id, stock=quantity
            await _repo.AddProductToCartAsync(10, 2, 3); // 2*3 =6
            await _repo.AddProductToCartAsync(10, 5, 2); // 5*2 =10
            var total = await _service.GetCartTotalAsync(10);
            Assert.Equal(16, total);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public async Task ClearCartAsync_InvalidBuyer_Throws(int b)
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.ClearCartAsync(b));
        }

        [Fact]
        public async Task ClearCartAsync_Valid_Clears()
        {
            await _repo.AddProductToCartAsync(11, 1, 1);
            await _service.ClearCartAsync(11);
            Assert.Empty(await _repo.GetCartItemsAsync(11));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-3)]
        public async Task GetCartItemCountAsync_InvalidBuyer_Throws(int b)
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetCartItemCountAsync(b));
        }

        [Fact]
        public async Task GetCartItemCountAsync_Valid_ReturnsSum()
        {
            await _repo.AddProductToCartAsync(12, 1, 2);
            await _repo.AddProductToCartAsync(12, 2, 3);
            var cnt = await _service.GetCartItemCountAsync(12);
            Assert.Equal(5, cnt);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        public async Task IsProductInCartAsync_InvalidArgs_Throws(int b, int p)
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.IsProductInCartAsync(b, p));
        }

        [Fact]
        public async Task IsProductInCartAsync_Valid_ReturnsCorrect()
        {
            await _repo.AddProductToCartAsync(13, 9, 1);
            Assert.True(await _service.IsProductInCartAsync(13, 9));
            Assert.False(await _service.IsProductInCartAsync(13, 99));
        }
    }
}
