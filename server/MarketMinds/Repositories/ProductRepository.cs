// <copyright file="ProductRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.Repository
{
using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.DBConnection;
using SharedClassLibrary.Domain;
using SharedClassLibrary.IRepository;
using SharedClassLibrary.Shared;

    /// <summary>
    /// Represents a repository for managing products in the database.
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        private readonly MarketPlaceDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public ProductRepository(MarketPlaceDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Adds a new product to the database.
        /// </summary>
        /// <param name="name">The name of the product.</param>
        /// <param name="price">The price of the product.</param>
        /// <param name="sellerId">The ID of the seller.</param>
        /// <param name="productType">The type of the product.</param>
        /// <param name="startDate">The start date of the product.</param>
        /// <param name="endDate">The end date of the product.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddProductAsync(string name, double price, int sellerId, string productType, DateTime startDate, DateTime endDate)
        {
            Product product = new Product
            {
                ProductId = 0,
                Name = name,
                Description = string.Empty,
                Price = price,
                SellerId = sellerId,
                Stock = 0,
                ProductType = productType,
                StartDate = startDate,
                EndDate = endDate,
            };

            await this.dbContext.Products.AddAsync(product);
            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Updates a product in the database.
        /// </summary>
        /// <param name="id">The ID of the product to update.</param>
        /// <param name="name">The new name of the product.</param>
        /// <param name="price">The new price of the product.</param>
        /// <param name="sellerId">The ID of the seller.</param>
        /// <param name="productType">The type of the product.</param>
        /// <param name="startDate">The start date of the product.</param>
        /// <param name="endDate">The end date of the product.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when the product is not found.</exception>
        public async Task UpdateProductAsync(int id, string name, double price, int sellerId, string productType, DateTime startDate, DateTime endDate)
        {
            Product? productToUpdate = await this.dbContext.Products.FindAsync(id)
                    ?? throw new Exception($"UpdateProductAsync:Product not found for the product id: {id}");

            productToUpdate.Name = name;
            productToUpdate.Price = price;
            productToUpdate.SellerId = sellerId;
            productToUpdate.ProductType = productType;
            productToUpdate.StartDate = startDate;
            productToUpdate.EndDate = endDate;

            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a product from the database.
        /// </summary>
        /// <param name="id">The ID of the product to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when the product is not found.</exception>
        public async Task DeleteProduct(int id)
        {
            Product? productToDelete = await this.dbContext.Products.FindAsync(id)
                    ?? throw new Exception($"DeleteProduct: Product not found for the product id: {id}");

            this.dbContext.Products.Remove(productToDelete);
            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Gets the name of the seller.
        /// </summary>
        /// <param name="sellerId">The ID of the seller.</param>
        /// <returns>The name of the seller.</returns>
        /// <exception cref="Exception">Thrown when the seller is not found.</exception>
        public async Task<string> GetSellerNameAsync(int? sellerId)
        {
            if (sellerId == null)
                    {
                throw new Exception($"GetSellerNameAsync: Seller ID is null");
                    }

            Seller? seller = await this.dbContext.Sellers.FindAsync(sellerId)
                    ?? throw new Exception($"GetSellerNameAsync: Seller not found for the seller id: {sellerId}");

            return seller.StoreName;
        }

        /// <summary>
        /// Gets a product by its ID.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <returns>The product.</returns>
        /// <exception cref="Exception">Thrown when the product is not found.</exception>
        public async Task<Product> GetProductByIdAsync(int productId)
        {
            Product? product = await this.dbContext.Products.FindAsync(productId)
                    ?? throw new Exception($"GetProductByIdAsync: Product not found for the product id: {productId}");

            return product;
        }

        /// <summary>
        /// Gets all borrowable products from the waitlist
        /// </summary>
        /// <returns>A list of products that are available for borrowing</returns>
        public async Task<List<Product>> GetBorrowableProductsAsync()
        {
            // Get the products with these IDs
            var products = await this.dbContext.Products
                .Where(p => (p.ProductType.ToLower() == "borrowed" || p.ProductType.ToLower() == "borrowable"))
                .ToListAsync();

            return products;
        }
    }
}