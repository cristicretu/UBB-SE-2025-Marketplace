using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.ImagineUploadService;

namespace MarketMinds.Test.Services.ImageUploadService
{
    public class ImageUploadMockRepository : IImageUploadService
    {
        public Func<Stream, string, Task<string>> UploadImageFunc { get; set; }
        public Func<string, Task<bool>> UploadImageAsyncFunc { get; set; }
        public Func<string, Image> CreateImageFromPathFunc { get; set; }
        public Func<List<Image>, string> FormatImagesStringFunc { get; set; }
        public Func<string, List<Image>> ParseImagesStringFunc { get; set; }
        public Func<Stream, string, string, Task<string>> AddImageToCollectionFunc { get; set; }

        public Task<string> UploadImage(Stream imageStream, string fileName)
            => UploadImageFunc?.Invoke(imageStream, fileName) ?? Task.FromResult("https://imgur.com/fake.png");

        public Task<bool> UploadImageAsync(string filePath)
            => UploadImageAsyncFunc?.Invoke(filePath) ?? Task.FromResult(true);

        public Image CreateImageFromPath(string path)
            => CreateImageFromPathFunc?.Invoke(path) ?? new Image(path);

        public string FormatImagesString(List<Image> images)
            => FormatImagesStringFunc?.Invoke(images) ?? (images != null ? string.Join("\n", images.ConvertAll(i => i.Url)) : string.Empty);        public List<Image> ParseImagesString(string imagesString)
        {
            if (ParseImagesStringFunc != null)
                return ParseImagesStringFunc.Invoke(imagesString);
                
            if (string.IsNullOrEmpty(imagesString))
                return new List<Image>();
                
            List<Image> images = new List<Image>();
            string[] urls = imagesString.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var url in urls)
            {
                if (!string.IsNullOrEmpty(url) && Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                    images.Add(new Image(url));
                }
            }
            return images;
        }public async Task<string> AddImageToCollection(Stream imageStream, string fileName, string currentImagesString)
        {
            if (AddImageToCollectionFunc != null)
                return await AddImageToCollectionFunc.Invoke(imageStream, fileName, currentImagesString);
                
            string imgurLink = await UploadImage(imageStream, fileName);
            if (!string.IsNullOrEmpty(imgurLink))
            {
                // Simplified implementation matching real service behavior
                var existingImages = ParseImagesString(currentImagesString).ConvertAll(i => i.Url);
                if (!existingImages.Contains(imgurLink))
                {
                    return string.IsNullOrEmpty(currentImagesString)
                        ? imgurLink
                        : currentImagesString + "\n" + imgurLink;
                }
            }
            return currentImagesString;
        }
    }
}
