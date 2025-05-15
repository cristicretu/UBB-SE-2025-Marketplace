// <copyright file="ProductApiController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using SharedClassLibrary.DataTransferObjects;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// API controller for managing dummy product data.
    /// </summary>
    [Authorize]
    [Route("api/products")]
    [ApiController]
    public class ProductApiController : ControllerBase
    {
        private readonly IProductRepository productRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductApiController"/> class.
        /// </summary>
        /// <param name="productRepository">The dummy product repository dependency.</param>
        public ProductApiController(IProductRepository productRepository)
        {
            this.productRepository = productRepository;
        }

        /// <summary>
        /// Asynchronously adds a new dummy product.
        /// </summary>
        /// <param name="request">The request containing the dummy product data.</param>
        /// <returns>An action result indicating success or failure.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> AddProduct([FromBody] ProductRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Name) || request.Price <= 0)
            {
                return this.BadRequest("Valid dummy product data is required.");
            }

            try
            {
                await this.productRepository.AddProductAsync(
                    request.Name,
                    request.Price,
                    request.SellerID,
                    request.ProductType,
                    request.StartDate,
                    request.EndDate);

                return this.Created();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while adding dummy product: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously updates an existing dummy product.
        /// </summary>
        /// <param name="id">The ID of the dummy product to update.</param>
        /// <param name="request">The request containing the updated dummy product data.</param>
        /// <returns>An action result indicating success or failure.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateProduct(int id, [FromBody] ProductRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Name) || request.Price <= 0)
            {
                return this.BadRequest("Valid dummy product data is required.");
            }

            try
            {
                await this.productRepository.UpdateProductAsync(
                    id,
                    request.Name,
                    request.Price,
                    request.SellerID,
                    request.ProductType,
                    request.StartDate,
                    request.EndDate);

                return this.NoContent();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while updating dummy product with ID {id}: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously deletes a dummy product by ID.
        /// </summary>
        /// <param name="id">The ID of the dummy product to delete.</param>
        /// <returns>An action result indicating success or failure.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            try
            {
                await this.productRepository.DeleteProduct(id);
                return this.NoContent();
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while deleting dummy product with ID {id}: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously retrieves a dummy product by ID.
        /// </summary>
        /// <param name="productId">The ID of the dummy product to retrieve.</param>
        /// <returns>The dummy product.</returns>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Product>> GetProductById(int productId)
        {
            try
            {
                var product = await this.productRepository.GetProductByIdAsync(productId);
                if (product == null)
                {
                    return this.NotFound($"Dummy product with ID {productId} not found.");
                }

                return this.Ok(product);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving dummy product with ID {productId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously retrieves the seller name for a given seller ID.
        /// </summary>
        /// <param name="sellerId">The ID of the seller.</param>
        /// <returns>The seller name.</returns>
        [HttpGet("seller/{sellerId}/name")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<string>> GetSellerName(int sellerId)
        {
            try
            {
                var sellerName = await this.productRepository.GetSellerNameAsync(sellerId);
                return this.Ok(sellerName);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving seller name for ID {sellerId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all products that are available for borrowing from the waitlist
        /// </summary>
        /// <returns>A list of borrowable products</returns>
        [HttpGet("borrowable")]
        [ProducesResponseType(typeof(List<Product>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<Product>>> GetBorrowableProducts()
        {
            try
            {
                var products = await this.productRepository.GetBorrowableProductsAsync();
                return this.Ok(products);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving borrowable products: {ex.Message}");
            }
        }
    }
}