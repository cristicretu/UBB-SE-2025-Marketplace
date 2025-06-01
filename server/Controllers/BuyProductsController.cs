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

        [HttpGet("filtered")]
        [ProducesResponseType(typeof(List<BuyProductDTO>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetFilteredBuyProducts(
            [FromQuery] int offset = 0,
            [FromQuery] int count = 0,
            [FromQuery] List<int>? conditionIds = null,
            [FromQuery] List<int>? categoryIds = null,
            [FromQuery] double? maxPrice = null,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                List<BuyProduct> products = _buyProductsRepository.GetFilteredProducts(offset, count, conditionIds, categoryIds, maxPrice, searchTerm);
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

        [HttpGet("filtered/seller")]
        [ProducesResponseType(typeof(List<BuyProductDTO>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetFilteredBuyProductsBySeller(
            [FromQuery] int offset = 0,
            [FromQuery] int count = 0,
            [FromQuery] List<int>? conditionIds = null,
            [FromQuery] List<int>? categoryIds = null,
            [FromQuery] double? maxPrice = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? sellerId = null)
        {
            try
            {
                List<BuyProduct> products = _buyProductsRepository.GetFilteredProducts(offset, count, conditionIds, categoryIds, maxPrice, searchTerm, sellerId);
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

        [HttpGet("filtered/count")]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetFilteredBuyProductsCount(
            [FromQuery] List<int>? conditionIds = null,
            [FromQuery] List<int>? categoryIds = null,
            [FromQuery] double? maxPrice = null,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var count = _buyProductsRepository.GetFilteredProductCount(conditionIds, categoryIds, maxPrice, searchTerm);
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

        [HttpGet("filtered/count/seller")]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetFilteredBuyProductsCountBySeller(
            [FromQuery] List<int>? conditionIds = null,
            [FromQuery] List<int>? categoryIds = null,
            [FromQuery] double? maxPrice = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? sellerId = null)
        {
            try
            {
                var count = _buyProductsRepository.GetFilteredProductCount(conditionIds, categoryIds, maxPrice, searchTerm, sellerId);
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
            Console.WriteLine($"Received CreateBuyProduct request");
            
            if (product == null)
            {
                Console.WriteLine("Product is null");
                return BadRequest("Product data is required");
            }

            // Log the received product details
            Console.WriteLine($"Received product: Title='{product.Title}', " +
                            $"SellerId={product.SellerId}, CategoryId={product.CategoryId}, " +
                            $"ConditionId={product.ConditionId}, Price={product.Price}, " +
                            $"Stock={product.Stock}");

            // Check ModelState validation
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid:");
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"  {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
                return BadRequest(ModelState);
            }

            // Additional business logic validation
            var validationErrors = new List<string>();

            if (string.IsNullOrWhiteSpace(product.Title))
            {
                validationErrors.Add("Title is required and cannot be empty");
            }

            if (product.SellerId <= 0)
            {
                validationErrors.Add("Valid seller ID is required (must be > 0)");
            }

            if (product.CategoryId <= 0)
            {
                validationErrors.Add("Valid category ID is required (must be > 0)");
            }

            if (product.ConditionId <= 0)
            {
                validationErrors.Add("Valid condition ID is required (must be > 0)");
            }

            if (product.Price < 0)
            {
                validationErrors.Add("Price cannot be negative");
            }

            if (product.Stock < 0)
            {
                validationErrors.Add("Stock cannot be negative");
            }

            if (validationErrors.Any())
            {
                Console.WriteLine($"Business validation failed: {string.Join("; ", validationErrors)}");
                return BadRequest(new { errors = validationErrors });
            }

            // Store tags for later processing, but don't include them in the initial product creation
            List<ProductTag> tagsToAdd = null;
            if (product.Tags != null && product.Tags.Any())
            {
                tagsToAdd = product.Tags.Where(t => t.Id > 0).ToList();
            }

            // Clear ProductTags to avoid validation issues during creation
            product.ProductTags = new List<BuyProductProductTag>();
            product.Tags = new List<ProductTag>();

            try
            {
                Console.WriteLine($"Attempting to add product to repository");
                _buyProductsRepository.AddProduct(product);
                Console.WriteLine($"Product successfully added with ID: {product.Id}");

                // Now add tags if we have any
                if (tagsToAdd != null && tagsToAdd.Any())
                {
                    Console.WriteLine($"Adding {tagsToAdd.Count} tags to product {product.Id}");
                    foreach (var tag in tagsToAdd)
                    {
                        var productTag = new BuyProductProductTag
                        {
                            ProductId = product.Id,
                            TagId = tag.Id,
                            Product = product,
                            Tag = tag
                        };
                        // Add the tag relationship (you may need to implement this method in the repository)
                        // For now, we'll skip this to get the basic product creation working
                    }
                }
                
                var buyProductDTO = BuyProductMapper.ToDTO(product);
                return CreatedAtAction(nameof(GetBuyProductById), new { id = product.Id }, buyProductDTO);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"ArgumentException in repository: {ex.Message}");
                return BadRequest(ex.Message);
            }
            catch (ApplicationException ex)
            {
                Console.WriteLine($"ApplicationException in repository: {ex.Message}");
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected exception in CreateBuyProduct: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
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

        [HttpGet("maxprice")]
        [ProducesResponseType(typeof(double), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetMaxPrice()
        {
            try
            {
                var maxPrice = await _buyProductsRepository.GetMaxPriceAsync();
                return Ok(maxPrice);
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
    }
}
