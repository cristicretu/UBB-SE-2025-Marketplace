using MarketMinds.Shared.Services.ImagineUploadService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace MarketMinds.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ImagesController : ControllerBase
    {
        private readonly IImageUploadService _imageUploadService;
        private readonly ILogger<ImagesController> _logger;

        public ImagesController(
            IImageUploadService imageUploadService,
            ILogger<ImagesController> logger)
        {
            _imageUploadService = imageUploadService;
            _logger = logger;
        }

        [HttpPost("Upload")]
        public async Task<IActionResult> Upload(IFormFile image)
        {
            _logger.LogInformation("Received image upload request");

            if (image == null || image.Length == 0)
            {
                _logger.LogWarning("No image file provided");
                return BadRequest(new { message = "No image file provided" });
            }

            try
            {
                _logger.LogInformation("Processing image: {FileName}, Size: {Size} bytes",
                    image.FileName, image.Length);

                using (var stream = image.OpenReadStream())
                {
                    var url = await _imageUploadService.UploadImage(stream, image.FileName);

                    if (string.IsNullOrEmpty(url))
                    {
                        _logger.LogError("Image upload failed - no URL returned");
                        return StatusCode(500, new { message = "Failed to upload image - no URL returned" });
                    }

                    _logger.LogInformation("Image uploaded successfully: {Url}", url);
                    return Ok(new { url = url });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image");
                return StatusCode(500, new { message = $"Image upload failed: {ex.Message}" });
            }
        }
    }
}