using System.Net;
using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Models.DTOs;
using MarketMinds.Shared.Models.DTOs.Mappers;
using MarketMinds.Shared.IRepository;

namespace MarketMinds.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BuyProductsController : ControllerBase
    {
        private readonly IBuyProductsRepository _buyProductsRepository;

        public BuyProductsController(IBuyProductsRepository buyProductsRepository)
        {
            _buyProductsRepository = buyProductsRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<BuyProductDTO>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetBuyProducts([FromQuery] int offset = 0, [FromQuery] int count = 0)
        {
            try
            {
                List<BuyProduct> products;
                
                if (count > 0)
                {
                    // Use pagination
                    products = _buyProductsRepository.GetProducts(offset, count);
                }
                else
                {
                    // Get all products (backward compatibility)
                    products = _buyProductsRepository.GetProducts();
                }
                
                var dtos = BuyProductMapper.ToDTOList(products);
                return Ok(dtos);
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

        [HttpGet("count")]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetBuyProductsCount()
        {
            try
            {
                var count = _buyProductsRepository.GetProductCount();
                return Ok(count);
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

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BuyProductDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetBuyProductById(int id)
        {
            try
            {
                var product = _buyProductsRepository.GetProductByID(id);
                var buyProductDTO = BuyProductMapper.ToDTO(product);
                return Ok(buyProductDTO);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
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
        [ProducesResponseType(typeof(BuyProductDTO), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult CreateBuyProduct([FromBody] BuyProduct product)
        {
            if (product == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (product.Id != 0)
            {
                return BadRequest("Product ID should not be provided when creating a new product.");
            }

            try
            {
                _buyProductsRepository.AddProduct(product);
                var buyProductDTO = BuyProductMapper.ToDTO(product);
                return CreatedAtAction(nameof(GetBuyProductById), new { id = product.Id }, buyProductDTO);
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
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while creating the product.");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult UpdateBuyProduct(int id, [FromBody] BuyProduct product)
        {
            if (product == null || !ModelState.IsValid)
            {
                return BadRequest("Product data is invalid.");
            }

            try
            {
                // Check if this is a partial update with just stock info
                if (product.Id == id && product.Stock < 0)
                {
                    // This is a stock decrease operation
                    var existingProduct = _buyProductsRepository.GetProductByID(id);
                    int decreaseAmount = -product.Stock; // Convert negative to positive

                    // Calculate new stock, ensuring it doesn't go below 0
                    existingProduct.Stock = Math.Max(0, existingProduct.Stock - decreaseAmount);

                    // Update the product
                    _buyProductsRepository.UpdateProduct(existingProduct);
                    return Ok(new
                    {
                        Message = $"Stock decreased by {decreaseAmount}",
                        ProductId = id,
                        NewStock = existingProduct.Stock
                    });
                }
                else if (id != product.Id)
                {
                    return BadRequest("ID mismatch.");
                }

                // Standard full update
                _buyProductsRepository.UpdateProduct(product);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
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
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while updating the product.");
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult DeleteBuyProduct(int id)
        {
            try
            {
                var productToDelete = _buyProductsRepository.GetProductByID(id);
                _buyProductsRepository.DeleteProduct(productToDelete);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
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
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while deleting the product.");
            }
        }

        /// <summary>
        /// Updates the stock quantity for a product
        /// </summary>
        [HttpPost("stock/{id}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult UpdateStock(int id, [FromBody] StockUpdateRequest request)
        {
            try
            {
                if (request == null || id <= 0 || request.Quantity < 0)
                {
                    return BadRequest(new { Message = "Invalid product data" });
                }

                // Get the product
                var product = _buyProductsRepository.GetProductByID(id);

                // Store old stock value for reporting
                int oldStock = product.Stock;

                // Calculate new stock (ensuring it doesn't go below 0)
                int newStock = Math.Max(0, product.Stock - request.Quantity);

                // Update the product's stock
                product.Stock = newStock;
                _buyProductsRepository.UpdateProduct(product);

                return Ok(new
                {
                    Message = $"Stock updated successfully for product {id}",
                    ProductId = id,
                    OldStock = oldStock,
                    NewStock = newStock,
                    DecreasedBy = request.Quantity
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ApplicationException ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, $"An error occurred while updating stock: {ex.Message}");
            }
        }

        public class StockUpdateRequest
        {
            public int Quantity { get; set; }
        }
    }
}
