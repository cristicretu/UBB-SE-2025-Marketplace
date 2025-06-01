using System.Net;
using Microsoft.AspNetCore.Mvc;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Models.DTOs;
using MarketMinds.Shared.IRepository;
using Microsoft.Extensions.DependencyInjection;  // for RequestServices
using Server.DataAccessLayer;                  // if you need EF context

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
        public IActionResult GetBorrowProducts([FromQuery] int offset = 0, [FromQuery] int count = 0)
        {
            try
            {
                List<BorrowProduct> products;

                if (count > 0)
                {
                    // Use pagination
                    products = borrowProductsRepository.GetProducts(offset, count);
                }
                else
                {
                    // Get all products (backward compatibility)
                    products = borrowProductsRepository.GetProducts();
                }

                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, $"An internal error occurred: {ex.Message}");
            }
        }

        [HttpGet("count")]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetBorrowProductsCount()
        {
            try
            {
                var count = borrowProductsRepository.GetProductCount();
                return Ok(count);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, $"Failed to get borrow products count: {ex.Message}");
            }
        }

        [HttpGet("filtered")]
        [ProducesResponseType(typeof(List<BorrowProduct>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetFilteredBorrowProducts(
            [FromQuery] int offset = 0,
            [FromQuery] int count = 0,
            [FromQuery] List<int>? conditionIds = null,
            [FromQuery] List<int>? categoryIds = null,
            [FromQuery] double? maxPrice = null,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                List<BorrowProduct> products = borrowProductsRepository.GetFilteredProducts(offset, count, conditionIds, categoryIds, maxPrice, searchTerm);
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, $"Failed to retrieve filtered borrow products: {ex.Message}");
            }
        }

        [HttpGet("filtered/count")]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetFilteredBorrowProductsCount(
            [FromQuery] List<int>? conditionIds = null,
            [FromQuery] List<int>? categoryIds = null,
            [FromQuery] double? maxPrice = null,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var count = borrowProductsRepository.GetFilteredProductCount(conditionIds, categoryIds, maxPrice, searchTerm);
                return Ok(count);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, $"Failed to get filtered borrow products count: {ex.Message}");
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
            catch (KeyNotFoundException knf)
            {
                return NotFound(knf.Message);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, $"An internal error occurred: {ex.Message}");
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
                return BadRequest(ModelState);

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

                // Process tags - convert DTO tags to ProductTag objects
                if (productDTO.Tags != null && productDTO.Tags.Any())
                {
                    product.Tags = productDTO.Tags.Select(tagDto => new ProductTag
                    {
                        Id = tagDto.Id,
                        Title = tagDto.Title
                    }).ToList();
                }

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

                return CreatedAtAction(nameof(GetBorrowProductById),
                                       new { id = product.Id },
                                       product);
            }
            catch (ArgumentException aex)
            {
                return BadRequest(aex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                                  $"An internal error occurred while creating the product: {ex.Message}");
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
                return BadRequest("Image URL cannot be empty.");

            try
            {
                var product = borrowProductsRepository.GetProductByID(id);
                borrowProductsRepository.AddImageToProduct(id, image);
                return NoContent();
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(knf.Message);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                                  $"An internal error occurred while adding the image: {ex.Message}");
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
                return BadRequest("Product data is invalid or ID mismatch.");

            try
            {
                borrowProductsRepository.UpdateProduct(product);
                return NoContent();
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(knf.Message);
            }
            catch (ArgumentException aex)
            {
                return BadRequest(aex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                                  $"An internal error occurred while updating the product: {ex.Message}");
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
            catch (KeyNotFoundException knf)
            {
                return NotFound(knf.Message);
            }
            catch (ArgumentException aex)
            {
                return BadRequest(aex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                                  $"An internal error occurred while deleting the product: {ex.Message}");
            }
        }

        //
        // ─── NEW "BORROW" ENDPOINT ────────────────────────────────────────────────────
        //

        /// <summary>
        /// Buyer clicks "Borrow": enqueue them and, if the product is free,
        /// immediately assign it to the earliest waiter.
        /// </summary>
        [HttpPost("{productId}/borrow/user/{userId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BorrowProduct(
            int productId,
            int userId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            if (productId <= 0 || userId <= 0 || endDate <= startDate)
                return BadRequest("Invalid IDs or date range.");

            // Resolve the waitlist repo from DI without changing your constructor
            var waitListRepo = HttpContext.RequestServices
                                           .GetRequiredService<IWaitListRepository>();

            // 1) Add user to waitlist
            await waitListRepo.AddUserToWaitlist(userId, productId);

            // 2) Immediately assign if free
            TryAssignNext(productId, waitListRepo);

            return NoContent();
        }

        private void TryAssignNext(int productId, IWaitListRepository waitListRepo)
        {
            var product = borrowProductsRepository.GetProductByID(productId);
            if (product == null) return;

            if (product.EndDate.HasValue && product.EndDate.Value > DateTime.UtcNow)
                return;

            var queue = waitListRepo.GetUsersInWaitlistOrdered(productId)
                                   .GetAwaiter().GetResult();
            var next = queue.FirstOrDefault();
            if (next == null)
            {
                product.StartDate = null;
                product.EndDate = null;
                product.IsBorrowed = false;
            }
            else
            {
                product.StartDate = next.JoinedTime;
                const int DefaultBorrowDays = 7;
                product.EndDate = next.JoinedTime.AddDays(DefaultBorrowDays);
                product.IsBorrowed = true;

                waitListRepo.RemoveUserFromWaitlist(next.UserID, productId)
                            .GetAwaiter().GetResult();
            }

            borrowProductsRepository.UpdateProduct(product);
        }

        [HttpGet("maxprice")]
        [ProducesResponseType(typeof(double), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetMaxPrice()
        {
            try
            {
                var maxPrice = await borrowProductsRepository.GetMaxPriceAsync();
                return Ok(maxPrice);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, $"Failed to get max price: {ex.Message}");
            }
        }

    }
}
