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
    public class AuctionProductsController : ControllerBase
    {
        private readonly IAuctionProductsRepository auctionProductsRepository;
        private readonly static int NULL_PRODUCT_ID = 0;
        private readonly static DateTime MINIMUM_SQL_DATETIME = new DateTime(1753, 1, 1);

        public AuctionProductsController(IAuctionProductsRepository auctionProductsRepository)
        {
            this.auctionProductsRepository = auctionProductsRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<AuctionProductDTO>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        public IActionResult GetAuctionProducts([FromQuery] int offset = 0, [FromQuery] int count = 0)
        {
            try
            {
                List<AuctionProduct> products;

                if (count > 0)
                {
                    // Use pagination
                    products = auctionProductsRepository.GetProducts(offset, count);
                }
                else
                {
                    // Get all products (backward compatibility)
                    products = auctionProductsRepository.GetProducts();
                }

                Console.WriteLine("========== DEBUG: AuctionProductsController.GetAuctionProducts ==========");
                Console.WriteLine($"Retrieved {products.Count} products from repository (offset: {offset}, count: {count})");

                var dtos = AuctionProductMapper.ToDTOList(products);

                Console.WriteLine($"Mapped to {dtos.Count} DTOs");
                foreach (var dto in dtos)
                {
                    Console.WriteLine($"DTO ID: {dto.Id}, Title: {dto.Title}");
                    Console.WriteLine($"DTO Category: {(dto.Category != null ? "Present" : "NULL")}");
                    if (dto.Category != null)
                    {
                        Console.WriteLine($"DTO Category ID: {dto.Category.Id}, DisplayTitle: {dto.Category.DisplayTitle}");
                    }
                }
                Console.WriteLine("=====================================================================");

                return Ok(dtos);
            }
            catch (Exception exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, $"Failed to retrieve auction products: {exception.Message}");
            }
        }

        [HttpGet("count")]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetAuctionProductsCount()
        {
            try
            {
                var count = auctionProductsRepository.GetProductCount();
                return Ok(count);
            }
            catch (Exception exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, $"Failed to get auction products count: {exception.Message}");
            }
        }

        [HttpGet("filtered")]
        [ProducesResponseType(typeof(List<AuctionProductDTO>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetFilteredAuctionProducts(
            [FromQuery] int offset = 0,
            [FromQuery] int count = 0,
            [FromQuery] List<int>? conditionIds = null,
            [FromQuery] List<int>? categoryIds = null,
            [FromQuery] double? maxPrice = null,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                List<AuctionProduct> products = auctionProductsRepository.GetFilteredProducts(offset, count, conditionIds, categoryIds, maxPrice, searchTerm);
                var dtos = AuctionProductMapper.ToDTOList(products);
                return Ok(dtos);
            }
            catch (Exception exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, $"Failed to retrieve filtered auction products: {exception.Message}");
            }
        }

        [HttpGet("filtered/count")]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetFilteredAuctionProductsCount(
            [FromQuery] List<int>? conditionIds = null,
            [FromQuery] List<int>? categoryIds = null,
            [FromQuery] double? maxPrice = null,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var count = auctionProductsRepository.GetFilteredProductCount(conditionIds, categoryIds, maxPrice, searchTerm);
                return Ok(count);
            }
            catch (Exception exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, $"Failed to get filtered auction products count: {exception.Message}");
            }
        }

        [HttpGet("filtered/seller")]
        [ProducesResponseType(typeof(List<AuctionProductDTO>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetFilteredAuctionProductsBySeller(
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
                List<AuctionProduct> products = auctionProductsRepository.GetFilteredProducts(offset, count, conditionIds, categoryIds, maxPrice, searchTerm, sellerId);
                var dtos = AuctionProductMapper.ToDTOList(products);
                return Ok(dtos);
            }
            catch (Exception exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, $"Failed to retrieve filtered auction products by seller: {exception.Message}");
            }
        }

        [HttpGet("filtered/count/seller")]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetFilteredAuctionProductsCountBySeller(
            [FromQuery] List<int>? conditionIds = null,
            [FromQuery] List<int>? categoryIds = null,
            [FromQuery] double? maxPrice = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? sellerId = null)
        {
            try
            {
                var count = auctionProductsRepository.GetFilteredProductCount(conditionIds, categoryIds, maxPrice, searchTerm, sellerId);
                return Ok(count);
            }
            catch (Exception exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, $"Failed to get filtered auction products count by seller: {exception.Message}");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AuctionProductDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        public IActionResult GetAuctionProductById(int id)
        {
            try
            {
                var product = auctionProductsRepository.GetProductByID(id);

                if (product == null)
                {
                    return NotFound($"Auction product with ID {id} not found.");
                }

                Console.WriteLine($"SERVER: GetAuctionProductById - Found auction {id} with {product.Bids.Count} bids");

                // Log seller information
                Console.WriteLine($"SERVER: GetAuctionProductById - SellerId: {product.SellerId}");
                if (product.Seller != null)
                {
                    Console.WriteLine($"SERVER: GetAuctionProductById - Seller loaded: Id={product.Seller.Id}, Username={product.Seller.Username}, Email={product.Seller.Email}");
                }
                else
                {
                    Console.WriteLine($"SERVER: GetAuctionProductById - Seller is NULL despite having SellerId={product.SellerId}");
                }

                // Log condition information
                Console.WriteLine($"SERVER: GetAuctionProductById - ConditionId: {product.ConditionId}");
                if (product.Condition != null)
                {
                    Console.WriteLine($"SERVER: GetAuctionProductById - Condition: Id={product.Condition.Id}, Name={product.Condition.Name}, Description={product.Condition.Description}");
                }
                else
                {
                    Console.WriteLine($"SERVER: GetAuctionProductById - Condition is NULL");
                }

                foreach (var bid in product.Bids)
                {
                    Console.WriteLine($"SERVER: GetAuctionProductById - Bid {bid.Id}: BidderId={bid.BidderId}, Bidder.Username={bid.Bidder?.Username ?? "NULL"}");
                }

                var auctionProductDTO = AuctionProductMapper.ToDTO(product);

                // Log DTO seller information
                if (auctionProductDTO.Seller != null)
                {
                    Console.WriteLine($"SERVER: GetAuctionProductById - DTO Seller: Id={auctionProductDTO.Seller.Id}, Username={auctionProductDTO.Seller.Username}");
                }
                else
                {
                    Console.WriteLine($"SERVER: GetAuctionProductById - DTO Seller is NULL");
                }

                // Log DTO condition information
                if (auctionProductDTO.Condition != null)
                {
                    Console.WriteLine($"SERVER: GetAuctionProductById - DTO Condition: Id={auctionProductDTO.Condition.Id}, DisplayTitle={auctionProductDTO.Condition.DisplayTitle}, Description={auctionProductDTO.Condition.Description}");
                }
                else
                {
                    Console.WriteLine($"SERVER: GetAuctionProductById - DTO Condition is NULL");
                }

                return Ok(auctionProductDTO);
            }
            catch (KeyNotFoundException knfEx)
            {
                return NotFound(knfEx.Message);
            }
            catch (Exception exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, $"Failed to retrieve auction product with ID {id}: {exception.Message}");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(AuctionProductDTO), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult CreateAuctionProduct([FromBody] AuctionProduct product)
        {
            Console.WriteLine($"TRACE: CreateAuctionProduct API endpoint received EndTime: {product.EndTime}");

            if (product?.Id != NULL_PRODUCT_ID)
            {
                return BadRequest("Product ID should not be provided when creating a new product.");
            }

            try
            {
                InitializeDates(product);
                var incomingImages = product.Images?.ToList() ?? new List<ProductImage>();
                product.Images = new List<ProductImage>();

                auctionProductsRepository.AddProduct(product);

                if (incomingImages.Any())
                {
                    foreach (var image in incomingImages)
                    {
                        image.ProductId = product.Id;
                        product.Images.Add(image);
                    }

                    auctionProductsRepository.UpdateProduct(product);
                }

                var auctionProductDTO = AuctionProductMapper.ToDTO(product);
                return CreatedAtAction(nameof(GetAuctionProductById), new { id = product.Id }, auctionProductDTO);
            }
            catch (Exception exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while creating the product.");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult UpdateAuctionProduct(int id, [FromBody] AuctionProduct product)
        {
            Console.WriteLine($"SERVER: UpdateAuctionProduct called with id={id}");
            Console.WriteLine($"SERVER: Received product: ID={product?.Id}, Title='{product?.Title}', EndTime={product?.EndTime}");
            
            if (product == null)
            {
                Console.WriteLine($"SERVER: UpdateAuctionProduct - Product is null");
                return BadRequest("Product data is required.");
            }
            
            if (id != product.Id)
            {
                Console.WriteLine($"SERVER: UpdateAuctionProduct - ID mismatch: route id={id}, product id={product.Id}");
                return BadRequest("ID mismatch between route and product.");
            }

            try
            {
                Console.WriteLine($"SERVER: UpdateAuctionProduct - Before InitializeDates: EndTime={product.EndTime}");
                InitializeDates(product);
                Console.WriteLine($"SERVER: UpdateAuctionProduct - After InitializeDates: EndTime={product.EndTime}");
                
                Console.WriteLine($"SERVER: UpdateAuctionProduct - Calling repository.UpdateProduct");
                auctionProductsRepository.UpdateProduct(product);
                Console.WriteLine($"SERVER: UpdateAuctionProduct - Update successful");
                return NoContent();
            }
            catch (Exception exception)
            {
                Console.WriteLine($"SERVER: UpdateAuctionProduct - Exception: {exception.GetType().Name}: {exception.Message}");
                if (exception.InnerException != null)
                {
                    Console.WriteLine($"SERVER: UpdateAuctionProduct - Inner exception: {exception.InnerException.GetType().Name}: {exception.InnerException.Message}");
                }
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while updating the product.");
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult DeleteAuctionProduct(int id)
        {
            try
            {
                var productToDelete = auctionProductsRepository.GetProductByID(id);
                auctionProductsRepository.DeleteProduct(productToDelete);
                return NoContent();
            }
            catch (Exception exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "An internal error occurred while deleting the product.");
            }
        }

        [HttpPost("{id}/bids")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult PlaceBid(int id, [FromBody] CreateBidDTO bidDTO)
        {
            try
            {
                Console.WriteLine($"SERVER: PlaceBid - Received bid request for auction {id} from bidder {bidDTO.BidderId} amount {bidDTO.Amount}");

                // First check if the product exists
                var product = auctionProductsRepository.GetProductByID(id);
                if (product == null)
                {
                    Console.WriteLine($"SERVER: PlaceBid - Auction {id} not found");
                    return NotFound($"Auction product with ID {id} not found.");
                }

                Console.WriteLine($"SERVER: PlaceBid - Found auction: {product.Title}, CurrentPrice={product.CurrentPrice}, EndTime={product.EndTime}");

                // Check if the auction has ended
                if (DateTime.Now >= product.EndTime)
                {
                    Console.WriteLine($"SERVER: PlaceBid - Auction has ended, EndTime={product.EndTime}, Now={DateTime.Now}");
                    return BadRequest("Cannot place bid on an ended auction.");
                }

                // Validate bid amount is higher than current price
                if (bidDTO.Amount <= product.CurrentPrice)
                {
                    Console.WriteLine($"SERVER: PlaceBid - Bid amount {bidDTO.Amount} not higher than current price {product.CurrentPrice}");
                    return BadRequest($"Bid amount must be higher than the current price (${product.CurrentPrice}).");
                }

                // Create and add the bid
                var bid = new Bid
                {
                    BidderId = bidDTO.BidderId,
                    ProductId = id,
                    Price = bidDTO.Amount,
                    Timestamp = DateTime.Now
                };

                Console.WriteLine($"SERVER: PlaceBid - Adding bid to auction {id}");
                product.Bids.Add(bid);
                product.CurrentPrice = bidDTO.Amount;

                Console.WriteLine($"SERVER: PlaceBid - Updating product in repository");

                try
                {
                    auctionProductsRepository.UpdateProduct(product);
                    Console.WriteLine($"SERVER: PlaceBid - Bid successfully placed");
                    return Ok(new { Success = true, Message = $"Bid of ${bidDTO.Amount} placed successfully." });
                }
                catch (Exception dbEx)
                {
                    // Check for the foreign key constraint error for bidder_id
                    if (dbEx.InnerException != null &&
                        dbEx.InnerException.Message.Contains("FK_Bids_Buyers"))
                    {
                        Console.WriteLine($"SERVER: PlaceBid - User role error: User {bidDTO.BidderId} is not a buyer");
                        return BadRequest("Your account doesn't have permission to place bids. Only buyer accounts can place bids.");
                    }

                    // If it's not a buyer role error, re-throw
                    throw;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"SERVER: PlaceBid - Error: {exception.GetType().Name}: {exception.Message}");
                if (exception.InnerException != null)
                {
                    Console.WriteLine($"SERVER: PlaceBid - Inner error: {exception.InnerException.GetType().Name}: {exception.InnerException.Message}");
                }
                return StatusCode((int)HttpStatusCode.InternalServerError, $"An internal error occurred: {exception.Message}");
            }
        }

        private void InitializeDates(AuctionProduct product)
        {
            var now = DateTime.Now;

            Console.WriteLine($"TRACE: InitializeDates received EndTime: {product.EndTime}");
            Console.WriteLine($"TRACE: Conditions - Default: {product.EndTime == default}, < MINIMUM_SQL_DATETIME: {product.EndTime < MINIMUM_SQL_DATETIME}, < now: {product.EndTime < now}");

            if (product.StartTime == default || product.StartTime < MINIMUM_SQL_DATETIME)
            {
                product.StartTime = now;
            }

            // Only set a default EndTime if it's invalid (default value, earlier than min SQL date, or in the past)
            if (product.EndTime == default || product.EndTime < MINIMUM_SQL_DATETIME || product.EndTime < now)
            {
                Console.WriteLine($"TRACE: Setting default EndTime (original: {product.EndTime})");
                product.EndTime = now.AddDays(7);
            }
            else
            {
                Console.WriteLine($"TRACE: Keeping original EndTime: {product.EndTime}");
            }

            // Still enforce that EndTime is after StartTime
            if (product.EndTime < product.StartTime)
            {
                Console.WriteLine($"TRACE: EndTime is before StartTime, adjusting (was: {product.EndTime})");
                product.EndTime = product.StartTime.AddDays(7);
            }

            Console.WriteLine($"TRACE: InitializeDates final EndTime: {product.EndTime}");

            if (product.StartPrice <= 0 && product.CurrentPrice > 0)
            {
                product.StartPrice = product.CurrentPrice;
            }
            else if (product.CurrentPrice <= 0 && product.StartPrice > 0)
            {
                product.CurrentPrice = product.StartPrice;
            }
        }

        [HttpGet("maxprice")]
        [ProducesResponseType(typeof(double), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetMaxPrice()
        {
            try
            {
                var maxPrice = await auctionProductsRepository.GetMaxPriceAsync();
                return Ok(maxPrice);
            }
            catch (Exception exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, $"Failed to get max price: {exception.Message}");
            }
        }
    }
}