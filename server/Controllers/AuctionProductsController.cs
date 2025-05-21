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
        public IActionResult GetAuctionProducts()
        {
            try
            {
                var products = auctionProductsRepository.GetProducts();
                
                Console.WriteLine("========== DEBUG: AuctionProductsController.GetAuctionProducts ==========");
                Console.WriteLine($"Retrieved {products.Count} products from repository");
                
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
                
                var auctionProductDTO = AuctionProductMapper.ToDTO(product);
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
            if (id != product?.Id)
            {
                return BadRequest("ID mismatch between route and product.");
            }
            
            try
            {
                InitializeDates(product);
                auctionProductsRepository.UpdateProduct(product);
                return NoContent();
            }
            catch (Exception exception)
            {
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
                var product = auctionProductsRepository.GetProductByID(id);
                
                var bid = new Bid
                {
                    BidderId = bidDTO.BidderId,
                    ProductId = id,
                    Price = bidDTO.Amount,
                    Timestamp = DateTime.Now
                };
                product.Bids.Add(bid);
                product.CurrentPrice = bidDTO.Amount;
                auctionProductsRepository.UpdateProduct(product);
                
                return Ok(new { Success = true, Message = $"Bid of ${bidDTO.Amount} placed successfully." });
            }
            catch (Exception exception)
            {
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
    }
}