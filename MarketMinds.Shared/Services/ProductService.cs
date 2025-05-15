using System;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using SharedClassLibrary.Shared;
using SharedClassLibrary.IRepository;
using SharedClassLibrary.ProxyRepository;
using SharedClassLibrary.Helper;

namespace SharedClassLibrary.Service
{
    /// <summary>
    /// Service for managing dummy product operations.
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IProductRepository productRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductService"/> class with a specified database provider.
        /// </summary>
        public ProductService()
        {
            this.productRepository = new ProductProxyRepository(AppConfig.GetBaseApiUrl());
        }

        /// <inheritdoc/>
        public async Task UpdateProductAsync(int id, string name, double price, int sellerId, string productType, DateTime startDate, DateTime endDate)
        {
            // Validate inputs
            if (id <= 0)
            {
                throw new ArgumentException("Product ID must be positive", nameof(id));
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Product name cannot be empty", nameof(name));
            }
            if (price < 0)
            {
                throw new ArgumentException("Price cannot be negative", nameof(price));
            }

            if (sellerId < 0)
            {
                throw new ArgumentException("Seller ID cannot be negative", nameof(sellerId));
            }
            if (string.IsNullOrWhiteSpace(productType))
            {
                throw new ArgumentException("Product type cannot be empty", nameof(productType));
            }

            // Only validate start and end dates for borrowed products
            if (productType == "borrowed")
            {
                if (startDate > endDate)
                {
                    throw new ArgumentException("Start date cannot be after end date", nameof(startDate));
                }
            }
            await productRepository.UpdateProductAsync(id, name, price, sellerId, productType, startDate, endDate);
        }

        /// <inheritdoc/>
        public async Task<Product> GetProductByIdAsync(int productId)
        {
            // Use the repository to fetch the product from the database
            return await productRepository.GetProductByIdAsync(productId);
        }

        /// <inheritdoc/>
        public async Task<string> GetSellerNameAsync(int sellerId)
        {
            // Use the repository to fetch the seller name from the database
            return await productRepository.GetSellerNameAsync(sellerId);
        }

        /// <inheritdoc/>
        public async Task<List<Product>> GetBorrowableProductsAsync()
        {
            // Use the repository to fetch borrowable products from the database
            return await productRepository.GetBorrowableProductsAsync();
        }
    }
}
