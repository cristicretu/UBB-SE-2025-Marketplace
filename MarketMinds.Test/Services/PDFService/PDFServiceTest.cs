using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using MarketMinds.Shared.Services;
using MarketMinds.Shared.IRepository;
using MarketMinds.Tests.Services;

namespace MarketMinds.Tests.Services
{
    public class PDFServiceTests
    {
        private readonly PDFService _service;
        private readonly MockPdfRepository _mockRepo;

        public PDFServiceTests()
        {
            _service = new PDFService();

            _mockRepo = new MockPdfRepository();

            var field = typeof(PDFService)
                .GetField("pdfRepository", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(_service, _mockRepo);
        }

        [Fact]
        public async Task InsertPdfAsync_ValidBytes_ReturnsNewId()
        {
            var fakeBytes = new byte[] { 1, 2, 3, 4 };

            var result = await _service.InsertPdfAsync(fakeBytes);

            Assert.True(result > 0);
            Assert.Equal(1, result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(new byte[0])]
        public async Task InsertPdfAsync_NullOrEmpty_ThrowsArgumentException(byte[] bytes)
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.InsertPdfAsync(bytes));
        }
    }
}
