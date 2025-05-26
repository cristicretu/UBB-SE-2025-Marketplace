using System;
using System.Threading.Tasks;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;
using Moq;
using Xunit;
using System.Reflection;

namespace MarketMinds.Test.Services
{
    public class OrderSummaryServiceTest
    {
        private readonly Mock<IOrderSummaryRepository> _repoMock;
        private readonly IOrderSummaryService _service;

        public OrderSummaryServiceTest()
        {
            _repoMock = new Mock<IOrderSummaryRepository>();
            _service = new OrderSummaryService();

            // Use reflection to inject the mock repository
            var field = typeof(OrderSummaryService)
                .GetField("orderSummaryRepository", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(_service, _repoMock.Object);
        }

        [Fact]
        public async Task UpdateOrderSummaryAsync_ValidParams_CallsRepository()
        {
            await _service.UpdateOrderSummaryAsync(1, 10, 1, 2, 13, "Name", "email@test.com", "123", "Addr", "0000", "info", "contract");
            _repoMock.Verify(r => r.UpdateOrderSummaryAsync(1, 10, 1, 2, 13, "Name", "email@test.com", "123", "Addr", "0000", "info", "contract"), Times.Once);
        }

        [Theory]
        [InlineData(0, 10, 1, 2, 13, "Name", "email", "123", "Addr", "0000")] // id <= 0
        [InlineData(1, -1, 1, 2, 13, "Name", "email", "123", "Addr", "0000")] // subtotal < 0
        [InlineData(1, 10, -1, 2, 13, "Name", "email", "123", "Addr", "0000")] // warrantyTax < 0
        [InlineData(1, 10, 1, -1, 13, "Name", "email", "123", "Addr", "0000")] // deliveryFee < 0
        [InlineData(1, 10, 1, 2, -1, "Name", "email", "123", "Addr", "0000")] // finalTotal < 0
        [InlineData(1, 10, 1, 2, 13, "", "email", "123", "Addr", "0000")] // fullName empty
        [InlineData(1, 10, 1, 2, 13, "Name", "", "123", "Addr", "0000")] // email empty
        [InlineData(1, 10, 1, 2, 13, "Name", "email", "", "Addr", "0000")] // phoneNumber empty
        [InlineData(1, 10, 1, 2, 13, "Name", "email", "123", "", "0000")] // address empty
        [InlineData(1, 10, 1, 2, 13, "Name", "email", "123", "Addr", "")] // postalCode empty
        public async Task UpdateOrderSummaryAsync_InvalidParams_Throws(
            int id, double subtotal, double warrantyTax, double deliveryFee, double finalTotal,
            string fullName, string email, string phoneNumber, string address, string postalCode)
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateOrderSummaryAsync(id, subtotal, warrantyTax, deliveryFee, finalTotal,
                    fullName, email, phoneNumber, address, postalCode, "info", "contract"));
        }

        [Fact]
        public async Task GetOrderSummaryByIdAsync_Valid_CallsRepository()
        {
            _repoMock.Setup(r => r.GetOrderSummaryByIdAsync(1)).ReturnsAsync(new OrderSummary());
            var result = await _service.GetOrderSummaryByIdAsync(1);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetOrderSummaryByIdAsync_InvalidId_Throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetOrderSummaryByIdAsync(0));
        }

        [Fact]
        public async Task CreateOrderSummaryAsync_Valid_CallsRepository()
        {
            var summary = new OrderSummary { ID = 1 };
            _repoMock.Setup(r => r.AddOrderSummaryAsync(summary)).ReturnsAsync(10);
            var result = await _service.CreateOrderSummaryAsync(summary);
            Assert.Equal(10, result);
        }

        [Fact]
        public async Task CreateOrderSummaryAsync_Null_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateOrderSummaryAsync(null));
        }
    }
}
