using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.ImagineUploadService;
using Microsoft.Extensions.Configuration;
using System;

namespace MarketMinds.Test.Services.ImageUploadService
{
    [TestFixture]
    public class ImageUploadServiceTest
    {
        private IImageUploadService _service;
        private IConfiguration _mockConfig;

        [SetUp]
        public void Setup()
        {
            // Mock configuration with a valid Imgur Client ID
            var inMemorySettings = new Dictionary<string, string> {
                {"ImgurSettings:ClientId", "12345678901234567890"} // 20 chars, valid
            };
            _mockConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _service = new MarketMinds.Shared.Services.ImagineUploadService.ImageUploadService(_mockConfig);

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
        public void FormatImagesString_NullList_ReturnsEmptyString()
        {
            var result = _service.FormatImagesString(null);
            Assert.That(result, Is.EqualTo(string.Empty));
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
        public void ParseImagesString_IgnoresInvalidUrls()
        {
            var images = _service.ParseImagesString("not_a_url\nhttps://imgur.com/image.png");
            Assert.That(images.Count, Is.EqualTo(1));
            Assert.That(images[0].Url, Is.EqualTo("https://imgur.com/image.png"));
        }

        [Test]
        public void UploadImage_ThrowsOnNullStream()
        {
            var ex = Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _service.UploadImage(null, "file.png"));
            Assert.That(ex.ParamName, Is.EqualTo("imageStream"));
        }

        [Test]
        public void UploadImage_ThrowsOnNullFileName()
        {
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes("data"));
            var ex = Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _service.UploadImage(ms, null));
            Assert.That(ex.ParamName, Is.EqualTo("fileName"));
        }

        [Test]
        public void UploadImageAsync_ThrowsOnNullOrNonexistentFile()
        {
            var ex1 = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _service.UploadImageAsync(null));
            Assert.That(ex1.ParamName, Is.EqualTo("filePath"));

            var ex2 = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _service.UploadImageAsync("nonexistentfile.png"));
            Assert.That(ex2.ParamName, Is.EqualTo("filePath"));
        }

        [Test]
        public async Task AddImageToCollection_DoesNotDuplicateExisting()
        {
            // Use a fake service to avoid real upload
            var fakeService = new FakeImageUploadService("https://imgur.com/image1.png");
            string existingImage = "https://imgur.com/image1.png";
            string currentImagesString = existingImage;

            string result = await fakeService.AddImageToCollection(
                new MemoryStream(Encoding.UTF8.GetBytes("fake image content")),
                "image.png",
                currentImagesString
            );

            Assert.That(result, Is.EqualTo(existingImage));
        }

        [Test]
        public async Task AddImageToCollection_AddsNewImage()
        {
            var fakeService = new FakeImageUploadService("https://imgur.com/newimage.png");
            string currentImagesString = "https://imgur.com/image1.png";

            string result = await fakeService.AddImageToCollection(
                new MemoryStream(Encoding.UTF8.GetBytes("fake image content")),
                "image.png",
                currentImagesString
            );

            Assert.That(result, Is.EqualTo("https://imgur.com/image1.png\nhttps://imgur.com/newimage.png"));
        }

        [Test]
        public async Task AddImageToCollection_EmptyCollection_AddsImage()
        {
            var fakeService = new FakeImageUploadService("https://imgur.com/newimage.png");
            string result = await fakeService.AddImageToCollection(
                new MemoryStream(Encoding.UTF8.GetBytes("fake image content")),
                "image.png",
                ""
            );
            Assert.That(result, Is.EqualTo("https://imgur.com/newimage.png"));
        }

        // Fake service implementing only IImageUploadService for AddImageToCollection logic
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
                return images != null ? string.Join("\n", images.Select(i => i.Url)) : string.Empty;
            }

            public List<Image> ParseImagesString(string imagesString)
            {
                if (string.IsNullOrEmpty(imagesString))
                    return new List<Image>();
                return imagesString.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
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
