using System.Net;
using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;

namespace MarketMinds.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductCategoryController : ControllerBase
    {
        private readonly IProductCategoryRepository productCategoryRepository;

        public ProductCategoryController(IProductCategoryRepository productCategoryRepository)
        {
            this.productCategoryRepository = productCategoryRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<Category>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetAllProductCategories()
        {
            try
            {
                var allCategories = productCategoryRepository.GetAllProductCategories();
                return Ok(allCategories);
            }
            catch (Exception exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(Category), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult CreateProductCategory([FromBody] ProductCategoryRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body cannot be null");
            }

            try
            {
                var newCategory = productCategoryRepository.CreateProductCategory(
                    request.DisplayTitle,
                    request.Description);

                return CreatedAtAction(
                    nameof(GetAllProductCategories),
                    new { id = newCategory.Id },
                    newCategory);
            }
            catch (ArgumentException argumentException)
            {
                return BadRequest(argumentException.Message);
            }
            catch (Exception exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while creating the category.");
            }
        }

        [HttpDelete("{title}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult DeleteProductCategory(string productCategoryName)
        {
            if (string.IsNullOrWhiteSpace(productCategoryName))
            {
                return BadRequest("Category title is required.");
            }

            try
            {
                productCategoryRepository.DeleteProductCategory(productCategoryName);
                return NoContent();
            }
            catch (KeyNotFoundException keyNotFoundException)
            {
                return NotFound(keyNotFoundException.Message);
            }
            catch (Exception exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while deleting the category.");
            }
        }
    }

    public class ProductCategoryRequest
    {
        public string DisplayTitle { get; set; } = null!;
        public string? Description { get; set; }
    }
}