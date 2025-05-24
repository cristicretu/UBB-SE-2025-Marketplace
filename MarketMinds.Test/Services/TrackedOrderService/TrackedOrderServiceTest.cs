using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;
using MarketMinds.Shared.IRepository;
using Moq;
using NUnit.Framework;

namespace MarketMinds.Test.Services
{
    [TestFixture]
    public class TrackedOrderServiceTest
    {
        private Mock<ITrackedOrderRepository> _mockRepo;
        private TrackedOrderService _service;

        [SetUp]
        public void Setup()
        {
            _mockRepo = new Mock<ITrackedOrderRepository>();
            _service = new TrackedOrderService();
            typeof(TrackedOrderService)
                .GetField("trackedOrderRepository", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_service, _mockRepo.Object);
        }

        [Test]
        public async Task AddTrackedOrderAsync_CallsRepository()
        {
            var order = new TrackedOrder { TrackedOrderID = 1, OrderID = 1, CurrentStatus = OrderStatus.PROCESSING, EstimatedDeliveryDate = DateOnly.FromDateTime(DateTime.Today), DeliveryAddress = "Test" };
            _mockRepo.Setup(r => r.AddTrackedOrderAsync(order)).ReturnsAsync(1);
            var result = await _service.AddTrackedOrderAsync(order);
            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public async Task GetTrackedOrderByIDAsyncReturnsOrder()
        {
            var order = new TrackedOrder
            {
                TrackedOrderID = 2,
                DeliveryAddress = "Test Address" // <-- Required property set
            };
            _mockRepo.Setup(r => r.GetTrackedOrderByIdAsync(2)).ReturnsAsync(order);
            var result = await _service.GetTrackedOrderByIDAsync(2);
            Assert.That(result, Is.EqualTo(order));
        }


        [Test]
        public async Task GetTrackedOrderByIDAsync_ReturnsNullOnException()
        {
            _mockRepo.Setup(r => r.GetTrackedOrderByIdAsync(3)).ThrowsAsync(new Exception());
            var result = await _service.GetTrackedOrderByIDAsync(3);
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task DeleteTrackedOrderAsync_CallsRepository()
        {
            _mockRepo.Setup(r => r.DeleteTrackedOrderAsync(1)).ReturnsAsync(true);
            var result = await _service.DeleteTrackedOrderAsync(1);
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task AddOrderCheckpointAsync_CallsRepository()
        {
            var checkpoint = new OrderCheckpoint { CheckpointID = 1 };
            _mockRepo.Setup(r => r.AddOrderCheckpointAsync(checkpoint)).ReturnsAsync(1);
            var result = await _service.AddOrderCheckpointAsync(checkpoint);
            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public async Task GetOrderCheckpointByIDAsync_ReturnsCheckpoint()
        {
            var checkpoint = new OrderCheckpoint { CheckpointID = 2 };
            _mockRepo.Setup(r => r.GetOrderCheckpointByIdAsync(2)).ReturnsAsync(checkpoint);
            var result = await _service.GetOrderCheckpointByIDAsync(2);
            Assert.That(result, Is.EqualTo(checkpoint));
        }

        [Test]
        public async Task GetOrderCheckpointByIDAsync_ReturnsNullOnException()
        {
            _mockRepo.Setup(r => r.GetOrderCheckpointByIdAsync(3)).ThrowsAsync(new Exception());
            var result = await _service.GetOrderCheckpointByIDAsync(3);
            Assert.That(result, Is.Null);
        }
    }
} 