using System.Net;
using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;

namespace MarketMinds.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductConditionController : ControllerBase
    {
        private readonly IProductConditionRepository productConditionRepository;

        public ProductConditionController(IProductConditionRepository productConditionRepository)
        {
            this.productConditionRepository = productConditionRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<Condition>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetAllProductConditions()
        {
            try
            {
                var allConditions = productConditionRepository.GetAllProductConditions();
                return Ok(allConditions);
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(Condition), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult CreateProductCondition([FromBody] ProductConditionRequest productConditionRequest)
        {
            if (productConditionRequest == null)
            {
                return BadRequest("Request body cannot be null");
            }

            try
            {
                var newCondition = productConditionRepository.CreateProductCondition(
                    productConditionRequest.DisplayTitle,
                    productConditionRequest.Description);

                return CreatedAtAction(
                    nameof(GetAllProductConditions),
                    new { id = newCondition.Id },
                    newCondition);
            }
            catch (ArgumentException argumentException)
            {
                return BadRequest(argumentException.Message);
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while creating the condition.");
            }
        }

        [HttpDelete("{title}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult DeleteProductCondition(string productConditionName)
        {
            if (string.IsNullOrWhiteSpace(productConditionName))
            {
                return BadRequest("Condition title is required.");
            }

            try
            {
                productConditionRepository.DeleteProductCondition(productConditionName);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                // If the condition doesn't exist, return success anyway (idempotent delete)
                return NoContent();
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("being used by one or more products"))
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while deleting the condition.");
            }
        }
    }

    public class ProductConditionRequest
    {
        public string DisplayTitle { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}