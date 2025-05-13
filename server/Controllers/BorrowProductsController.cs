using System.Net;
using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Models.DTOs;
using MarketMinds.Shared.IRepository;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BorrowProductsController : ControllerBase
    {
        private readonly IBorrowProductsRepository borrowProductsRepository;
        private readonly static int NULL_PRODUCT_ID = 0;

        public BorrowProductsController(IBorrowProductsRepository borrowProductsRepository)
        {
            this.borrowProductsRepository = borrowProductsRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<BorrowProduct>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetBorrowProducts()
        {
            try
            {
                var products = borrowProductsRepository.GetProducts();
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BorrowProduct), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetBorrowProductById(int id)
        {
            try
            {
                var product = borrowProductsRepository.GetProductByID(id);
                return Ok(product);
            }
            catch (KeyNotFoundException keyNotFoundException)
            {
                return NotFound(keyNotFoundException.Message);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred.");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(BorrowProduct), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [Consumes("application/json")]
        public IActionResult CreateBorrowProduct([FromBody] CreateBorrowProductDTO productDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var product = new BorrowProduct
                {
                    Title = productDTO.Title,
                    Description = productDTO.Description,
                    SellerId = productDTO.SellerId,
                    CategoryId = productDTO.CategoryId ?? NULL_PRODUCT_ID,
                    ConditionId = productDTO.ConditionId ?? NULL_PRODUCT_ID,
                    TimeLimit = productDTO.TimeLimit,
                    StartDate = productDTO.StartDate,
                    EndDate = productDTO.EndDate,
                    DailyRate = productDTO.DailyRate,
                    IsBorrowed = productDTO.IsBorrowed
                };

                borrowProductsRepository.AddProduct(product);

                if (productDTO.Images != null && productDTO.Images.Any())
                {
                    foreach (var imgDTO in productDTO.Images)
                    {
                        var img = new BorrowProductImage
                        {
                            Url = imgDTO.Url,
                            ProductId = product.Id
                        };

                        borrowProductsRepository.AddImageToProduct(product.Id, img);
                    }
                }

                return CreatedAtAction(nameof(GetBorrowProductById), new { id = product.Id }, product);
            }
            catch (ArgumentException argumentException)
            {
                return BadRequest(argumentException.Message);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while creating the product.");
            }
        }

        [HttpPost("{id}/images")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult AddImageToProduct(int id, [FromBody] BorrowProductImage image)
        {
            if (image == null || string.IsNullOrWhiteSpace(image.Url))
            {
                return BadRequest("Image URL cannot be empty.");
            }

            try
            {
                var product = borrowProductsRepository.GetProductByID(id);
                
                borrowProductsRepository.AddImageToProduct(id, image);
                
                return NoContent();
            }
            catch (KeyNotFoundException keyNotFoundException)
            {
                return NotFound(keyNotFoundException.Message);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while adding the image.");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult UpdateBorrowProduct(int id, [FromBody] BorrowProduct product)
        {
            if (product == null || id != product.Id || !ModelState.IsValid)
            {
                return BadRequest("Product data is invalid or ID mismatch.");
            }

            try
            {
                borrowProductsRepository.UpdateProduct(product);
                return NoContent();
            }
            catch (KeyNotFoundException keyNotFoundException)
            {
                return NotFound(keyNotFoundException.Message);
            }
            catch (ArgumentException argumentException)
            {
                return BadRequest(argumentException.Message);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while updating the product.");
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult DeleteBorrowProduct(int id)
        {
            try
            {
                var productToDelete = borrowProductsRepository.GetProductByID(id);
                borrowProductsRepository.DeleteProduct(productToDelete);
                return NoContent();
            }
            catch (KeyNotFoundException keyNotFoundException)
            {
                return NotFound(keyNotFoundException.Message);
            }
            catch (ArgumentException argumentException)
            {
                return BadRequest(argumentException.Message);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while deleting the product.");
            }
        }
    }
}