using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;
using Moq;
using Xunit;

namespace MarketMinds.Test.Services
{
    public class OrderServiceTest
    {
        private readonly Mock<IOrderRepository> _orderRepoMock;
        private readonly Mock<IOrderHistoryService> _orderHistoryMock;
        private readonly Mock<IOrderSummaryService> _orderSummaryMock;
        private readonly Mock<IShoppingCartService> _cartMock;
        private readonly IOrderService _service;

        public OrderServiceTest()
        {
            _orderRepoMock = new Mock<IOrderRepository>();
            _orderHistoryMock = new Mock<IOrderHistoryService>();
            _orderSummaryMock = new Mock<IOrderSummaryService>();
            _cartMock = new Mock<IShoppingCartService>();
            _service = new OrderService(
                _orderRepoMock.Object,
                _orderHistoryMock.Object,
                _orderSummaryMock.Object,
                _cartMock.Object
            );
        }

        [Fact]
        public async Task AddOrderAsync_ValidParams_CallsRepository()
        {
            await _service.AddOrderAsync(1, 2, "new", "card", 3, DateTime.UtcNow);
            _orderRepoMock.Verify(r => r.AddOrderAsync(1, 2, "new", "card", 3, It.IsAny<DateTime>()), Times.Once);
        }

        [Fact]
        public async Task AddOrderAsync_InvalidProductId_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.AddOrderAsync(0, 2, "new", "card", 3, DateTime.UtcNow));
        }

        [Fact]
        public async Task UpdateOrderAsync_InvalidOrderId_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateOrderAsync(0, "new", "card", DateTime.UtcNow));
        }

        [Fact]
        public async Task UpdateOrderAsync_EmptyPaymentMethod_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateOrderAsync(1, "new", "", DateTime.UtcNow));
        }

        [Fact]
        public async Task DeleteOrderAsync_Valid_CallsRepository()
        {
            await _service.DeleteOrderAsync(1);
            _orderRepoMock.Verify(r => r.DeleteOrderAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteOrderAsync_InvalidOrderId_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteOrderAsync(0));
        }

        [Fact]
        public async Task GetBorrowedOrderHistoryAsync_Valid_CallsRepository()
        {
            _orderRepoMock.Setup(r => r.GetBorrowedOrderHistoryAsync(1)).ReturnsAsync(new List<Order>());
            var result = await _service.GetBorrowedOrderHistoryAsync(1);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetBorrowedOrderHistoryAsync_NegativeBuyerId_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetBorrowedOrderHistoryAsync(-1));
        }

        [Fact]
        public async Task GetOrdersByNameAsync_EmptyText_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetOrdersByNameAsync(1, ""));
        }

        [Fact]
        public async Task GetOrderByIdAsync_Valid_CallsRepository()
        {
            _orderRepoMock.Setup(r => r.GetOrderByIdAsync(1)).ReturnsAsync(new Order());
            var result = await _service.GetOrderByIdAsync(1);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetOrderByIdAsync_InvalidOrderId_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetOrderByIdAsync(0));
        }

        [Fact]
        public async Task CreateOrderFromCartAsync_Valid_CallsAllDependencies()
        {
            var orderRequest = new OrderCreationRequestDto
            {
                Subtotal = 10,
                WarrantyTax = 1,
                DeliveryFee = 2,
                Total = 13,
                FullName = "Test",
                Email = "test@test.com",
                PhoneNumber = "123",
                Address = "Addr",
                ZipCode = "0000",
                AdditionalInfo = "",
                SelectedPaymentMethod = "card"
            };
            var cartItems = new List<Product> { new BuyProduct { Id = 1 } };

            _orderHistoryMock.Setup(x => x.CreateOrderHistoryAsync(1)).ReturnsAsync(100);
            _orderSummaryMock.Setup(x => x.CreateOrderSummaryAsync(It.IsAny<OrderSummary>())).ReturnsAsync(200);
            _orderRepoMock.Setup(x => x.AddOrderAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<DateTime>())).Returns(Task.CompletedTask);
            _cartMock.Setup(x => x.ClearCartAsync(1)).Returns(Task.CompletedTask);

            var result = await _service.CreateOrderFromCartAsync(orderRequest, 1, cartItems);

            Assert.Equal(100, result);
            _orderHistoryMock.Verify(x => x.CreateOrderHistoryAsync(1), Times.Once);
            _orderSummaryMock.Verify(x => x.CreateOrderSummaryAsync(It.IsAny<OrderSummary>()), Times.Once);
            _orderRepoMock.Verify(x => x.AddOrderAsync(1, 1, "new", "card", 200, It.IsAny<DateTime>()), Times.Once);
            _cartMock.Verify(x => x.ClearCartAsync(1), Times.Once);
        }

        [Fact]
        public async Task CreateOrderFromCartAsync_NullOrderRequest_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.CreateOrderFromCartAsync(null, 1, new List<Product> { new BuyProduct { Id = 1 } }));
        }

        [Fact]
        public async Task CreateOrderFromCartAsync_EmptyCart_Throws()
        {
            var orderRequest = new OrderCreationRequestDto { SelectedPaymentMethod = "card" };
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateOrderFromCartAsync(orderRequest, 1, new List<Product>()));
        }

        [Fact]
        public async Task CreateOrderFromCartAsync_InvalidUserId_Throws()
        {
            var orderRequest = new OrderCreationRequestDto { SelectedPaymentMethod = "card" };
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateOrderFromCartAsync(orderRequest, 0, new List<Product> { new BuyProduct { Id = 1 } }));
        }

        // Minimal test implementation for BuyProduct
        private class BuyProduct : Product
        {
            // Implement any required abstract members or properties if needed
        }
    }
}
