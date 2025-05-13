using System;
using System.Net;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;

namespace MarketMinds.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductTagController : ControllerBase
    {
        private readonly IProductTagRepository _productTagRepository;

        public ProductTagController(IProductTagRepository productTagRepository)
        {
            _productTagRepository = productTagRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ProductTag>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetAllProductTags()
        {
            try
            {
                var allTags = _productTagRepository.GetAllProductTags();
                return Ok(allTags);
            }
            catch (ApplicationException ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProductTag), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult CreateProductTag([FromBody] ProductTagRequest productTagRequest)
        {
            if (productTagRequest == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var newTag = _productTagRepository.CreateProductTag(productTagRequest.DisplayTitle);

                return CreatedAtAction(
                    nameof(GetAllProductTags),
                    new { id = newTag.Id },
                    newTag);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ApplicationException ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while creating the tag.");
            }
        }

        [HttpDelete("{title}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult DeleteProductTag(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return BadRequest("Tag title is required.");
            }

            try
            {
                _productTagRepository.DeleteProductTag(title);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                // Idempotent delete: return success even if the tag doesn't exist
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ApplicationException ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while deleting the tag.");
            }
        }
    }

    public class ProductTagRequest
    {
        public string DisplayTitle { get; set; } = null!;
    }
}
