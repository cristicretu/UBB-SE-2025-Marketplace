using MarketMinds.Shared.Services.ImagineUploadService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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
            if (image == null || image.Length == 0)
            {
                return BadRequest("No image file provided");
            }

            try
            {
                using (var stream = image.OpenReadStream())
                {
                    var url = await _imageUploadService.UploadImage(stream, image.FileName);
                    
                    if (string.IsNullOrEmpty(url))
                    {
                        return StatusCode(500, "Failed to upload image");
                    }
                    
                    return Ok(new { url });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image");
                return StatusCode(500, $"Image upload failed: {ex.Message}");
            }
        }
    }
} 