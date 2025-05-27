using System.Net.Http.Headers;
using MarketMinds.Shared.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace MarketMinds.Shared.Services.ImagineUploadService
{
    public class ImageUploadService : IImageUploadService
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        private readonly IConfiguration _configuration;

        private const int MAXIMUM_IMAGE_SIZE = 10 * 1024 * 1024;
        private const int MAXIMUM_NUMBER_OF_RETRIES = 3;
        private const int RETRY_DELAY = 2;
        private const string IMGUR_API_URL = "https://api.imgur.com/3/image";
        private const string IMGUR_CLIENT_ID_PLACEHOLDER = "YOUR_IMGUR_CLIENT_ID";
        private const string IMAGE_CONTENT_TYPE = "image/png";
        private const int IMGUR_CLIENT_ID_LENGTH = 20;
        private const int BASE_RETRY = 0;

        public ImageUploadService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> UploadImage(Stream imageStream, string fileName)
        {
            if (imageStream == null)
            {
                throw new ArgumentNullException(nameof(imageStream));
            }
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }
            return await UploadToImgur(imageStream, fileName);
        }

        private async Task<string> UploadToImgur(Stream imageStream, string fileName)
        {
            try
            {
                string clientId = _configuration["ImgurSettings:ClientId"];
                if (string.IsNullOrEmpty(clientId))
                {
                    throw new InvalidOperationException("Imgur Client ID is not configured. Please check your appsettings.json file.");
                }
                if (clientId.Length > IMGUR_CLIENT_ID_LENGTH)
                {
                    throw new InvalidOperationException("Client ID format appears invalid. Please ensure you're using the Client ID, not the Client Secret.");
                }

                using (var memoryStream = new MemoryStream())
                {
                    await imageStream.CopyToAsync(memoryStream);
                    if (memoryStream.Length == 0)
                    {
                        throw new InvalidOperationException("Image stream is empty.");
                    }
                    if (memoryStream.Length > MAXIMUM_IMAGE_SIZE)
                    {
                        throw new InvalidOperationException($"File size ({memoryStream.Length} bytes) exceeds Imgur's 10MB limit.");
                    }

                    byte[] buffer = memoryStream.ToArray();

                    int maximumNumberOfRetries = MAXIMUM_NUMBER_OF_RETRIES;
                    int currentRetry = BASE_RETRY;
                    TimeSpan delay = TimeSpan.FromSeconds(RETRY_DELAY);

                    while (currentRetry < maximumNumberOfRetries)
                    {
                        try
                        {
                            using (var content = new MultipartFormDataContent())
                            {
                                var imageContent = new ByteArrayContent(buffer);
                                imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse(IMAGE_CONTENT_TYPE);
                                content.Add(imageContent, "image", fileName ?? "image.png");

                                using (var request = new HttpRequestMessage(HttpMethod.Post, IMGUR_API_URL))
                                {
                                    request.Headers.Add("Authorization", $"Client-ID {clientId}");
                                    request.Content = content;
                                    var response = await HttpClient.SendAsync(request);
                                    var responseBody = await response.Content.ReadAsStringAsync();

                                    if (response.IsSuccessStatusCode)
                                    {
                                        dynamic jsonResponse = JsonConvert.DeserializeObject(responseBody);
                                        string link = jsonResponse?.data?.link;
                                        if (!string.IsNullOrEmpty(link))
                                        {
                                            return link;
                                        }
                                    }
                                }
                            }
                        }
                        catch (HttpRequestException ex)
                        {
                            if (currentRetry >= maximumNumberOfRetries - 1) throw;
                        }

                        if (currentRetry < maximumNumberOfRetries - 1)
                        {
                            await Task.Delay(delay);
                            delay = TimeSpan.FromSeconds(delay.TotalSeconds * 2);
                        }
                        currentRetry++;
                    }
                    throw new InvalidOperationException("Failed to upload image to Imgur after multiple retries.");
                }
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException($"Imgur upload failed: {exception.Message}", exception);
            }
        }

        private string GetImgurClientId()
        {
            try
            {
                string appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MarketMinds", "appsettings.json");

                if (!File.Exists(appSettingsPath))
                {
                    string currentDir = AppDomain.CurrentDomain.BaseDirectory;
                    string targetDirName = "MarketMinds";
                    string rootPath = Path.GetPathRoot(currentDir);
                    while (currentDir != null && currentDir != rootPath && !Directory.Exists(Path.Combine(currentDir, targetDirName)))
                    {
                        currentDir = Directory.GetParent(currentDir)?.FullName;
                    }

                    if (currentDir != null && Directory.Exists(Path.Combine(currentDir, targetDirName)))
                    {
                        appSettingsPath = Path.Combine(currentDir, targetDirName, "appsettings.json");
                    }
                    else
                    {
                        string mainProjectDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..");
                        if (Directory.Exists(Path.Combine(mainProjectDir, targetDirName)))
                        {
                            appSettingsPath = Path.Combine(mainProjectDir, targetDirName, "appsettings.json");
                        }
                    }
                }

                if (File.Exists(appSettingsPath))
                {
                    string json = File.ReadAllText(appSettingsPath);
                    dynamic settings = JsonConvert.DeserializeObject(json);
                    return settings?.ImgurSettings?.ClientId?.ToString();
                }
                throw new Exception("Imgur Client ID not found in appsettings.json");
            }
            catch (Exception exception)
            {
                throw new Exception($"Failed to get Imgur Client ID: {exception.Message}", exception);
            }
        }

        public async Task<bool> UploadImageAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                throw new ArgumentException("File path is null or does not exist.", nameof(filePath));
            }
            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    string uploadedUrl = await UploadImage(fileStream, Path.GetFileName(filePath));
                    return !string.IsNullOrEmpty(uploadedUrl);
                }
            }
            catch (Exception exception)
            {
                throw new Exception($"Failed to upload image to Imgur: {exception.Message}", exception);
            }
        }

        public Image CreateImageFromPath(string path)
        {
            return new Image(path);
        }

        public string FormatImagesString(List<Image> images)
        {
            return images != null ? string.Join("\n", images.Select(image => image.Url)) : string.Empty;
        }

        public List<Image> ParseImagesString(string imagesString)
        {
            if (string.IsNullOrEmpty(imagesString))
            {
                return new List<Image>();
            }

            return imagesString.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(url => !string.IsNullOrEmpty(url) && Uri.IsWellFormedUriString(url, UriKind.Absolute))
                .Select(url => new Image(url))
                .ToList();
        }

        public async Task<string> AddImageToCollection(Stream imageStream, string fileName, string currentImagesString)
        {
            string imgurLink = await UploadImage(imageStream, fileName);
            if (!string.IsNullOrEmpty(imgurLink))
            {
                var existingImages = ParseImagesString(currentImagesString).Select(image => image.Url).ToList();
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