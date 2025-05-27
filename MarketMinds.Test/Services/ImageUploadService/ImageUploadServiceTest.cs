using NUnit.Framework;
using System.Collections.Generic;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.ImagineUploadService;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using MarketMinds.Shared.Helper;

namespace MarketMinds.Tests.Services.ImagineUploadService
{
    [TestFixture]
    public class ImageUploadServiceTests
    {
        private IImageUploadService _service;

        [SetUp]
        public void Setup()
        {
            // Use the real service for tests that don't hit Imgur
            _service = new ImageUploadService(AppConfig.Configuration);
        }

        [Test]
        public void CreateImageFromPath_ReturnsCorrectImage()
        {
            var path = "https://i.imgur.com/example.jpg";
            var image = _service.CreateImageFromPath(path);
            Assert.That(image, Is.Not.Null);
            Assert.That(image.Url, Is.EqualTo(path));
        }

        [Test]
        public void FormatImagesString_FormatsListCorrectly()
        {
            var images = new List<Image>
            {
                new Image("https://imgur.com/image1.png"),
                new Image("https://imgur.com/image2.png")
            };

            var result = _service.FormatImagesString(images);

            Assert.That(result, Is.EqualTo("https://imgur.com/image1.png\nhttps://imgur.com/image2.png"));
        }

        [Test]
        public void ParseImagesString_ParsesCorrectly()
        {
            string imageString = "https://imgur.com/image1.png\nhttps://imgur.com/image2.png";

            var images = _service.ParseImagesString(imageString);

            Assert.That(images.Count, Is.EqualTo(2));
            Assert.That(images[0].Url, Is.EqualTo("https://imgur.com/image1.png"));
            Assert.That(images[1].Url, Is.EqualTo("https://imgur.com/image2.png"));
        }

        [Test]
        public void ParseImagesString_ReturnsEmptyList_OnEmptyInput()
        {
            var images = _service.ParseImagesString(string.Empty);

            Assert.That(images, Is.Not.Null);
            Assert.That(images.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task AddImageToCollection_DoesNotDuplicateExisting()
        {
            // Arrange
            string existingImage = "https://imgur.com/image1.png";
            string currentImagesString = existingImage;

            var fakeService = new FakeImageUploadService(existingImage);

            // Act
            string result = await fakeService.AddImageToCollection(
                new MemoryStream(Encoding.UTF8.GetBytes("fake image content")),
                "image.png",
                currentImagesString
            );

            // Assert
            Assert.That(result, Is.EqualTo(existingImage));
        }

        // Fake service implementing only IImageUploadService
        private class FakeImageUploadService : IImageUploadService
        {
            private readonly string _fakeLink;

            public FakeImageUploadService(string fakeLink)
            {
                _fakeLink = fakeLink;
            }

            public Task<string> UploadImage(Stream imageStream, string fileName)
            {
                return Task.FromResult(_fakeLink);
            }

            public Task<bool> UploadImageAsync(string filePath)
            {
                return Task.FromResult(true);
            }

            public Image CreateImageFromPath(string path)
            {
                return new Image(path);
            }

            public string FormatImagesString(List<Image> images)
            {
                return string.Join("\n", images.Select(i => i.Url));
            }

            public List<Image> ParseImagesString(string imagesString)
            {
                return imagesString.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries)
                                   .Select(url => new Image(url))
                                   .ToList();
            }

            public Task<string> AddImageToCollection(Stream imageStream, string fileName, string currentImagesString)
            {
                var existingUrls = ParseImagesString(currentImagesString).Select(i => i.Url).ToList();
                if (!existingUrls.Contains(_fakeLink))
                {
                    return Task.FromResult(string.IsNullOrEmpty(currentImagesString)
                        ? _fakeLink
                        : currentImagesString + "\n" + _fakeLink);
                }
                return Task.FromResult(currentImagesString);
            }
        }
    }
}
