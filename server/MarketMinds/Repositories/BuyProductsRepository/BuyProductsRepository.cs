using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;
using Server.DataAccessLayer;
using Microsoft.EntityFrameworkCore;

namespace MarketMinds.Repositories.BuyProductsRepository
{
    public class BuyProductsRepository : IBuyProductsRepository
    {
        private readonly ApplicationDbContext context;

        public BuyProductsRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        public void AddProduct(BuyProduct product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            try
            {
                context.BuyProducts.Add(product);
                context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"EF Core AddProduct Error: {ex.InnerException?.Message ?? ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General AddProduct Error: {ex.Message}");
                throw;
            }
        }

        public void DeleteProduct(BuyProduct product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            try
            {
                context.BuyProducts.Remove(product);
                context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"EF Core DeleteProduct Error: {ex.InnerException?.Message ?? ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General DeleteProduct Error: {ex.Message}");
                throw;
            }
        }

        public List<BuyProduct> GetProducts()
        {
            try
            {
                var products = context.BuyProducts
                    .Include(p => p.Condition)
                    .Include(p => p.Category)
                    .Include(p => p.Images)
                    .Include(p => p.ProductTags)
                        .ThenInclude(pt => pt.Tag)
                    .ToList();

                // Manually load seller data for each product
                foreach (var product in products)
                {
                    var seller = context.Sellers
                        .Include(s => s.User)
                        .FirstOrDefault(s => s.Id == product.SellerId);

                    if (seller != null)
                    {
                        // Set the seller as the User (this workaround preserves compatibility with existing code)
                        product.Seller = seller.User;
                    }
                }

                return products;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting BuyProducts: {ex.Message}");
                throw;
            }
        }

        public BuyProduct GetProductByID(int productId)
        {
            try
            {
                var product = context.BuyProducts
                    .Include(p => p.Condition)
                    .Include(p => p.Category)
                    .Include(p => p.Images)
                    .Include(p => p.ProductTags)
                        .ThenInclude(pt => pt.Tag)
                    .FirstOrDefault(p => p.Id == productId);

                if (product != null)
                {
                    // Manually load seller data
                    var seller = context.Sellers
                        .Include(s => s.User)
                        .FirstOrDefault(s => s.Id == product.SellerId);

                    if (seller != null)
                    {
                        // Set the seller as the User (this workaround preserves compatibility with existing code)
                        product.Seller = seller.User;
                    }
                }

                return product;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting BuyProduct by ID {productId}: {ex.Message}");
                throw;
            }
        }

        public void UpdateProduct(BuyProduct product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            try
            {
                var existingProduct = context.BuyProducts.Find(product.Id);
                if (existingProduct == null)
                {
                    throw new KeyNotFoundException($"BuyProduct with ID {product.Id} not found for update.");
                }

                // Update all fields including stock
                existingProduct.Title = product.Title;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.SellerId = product.SellerId;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.ConditionId = product.ConditionId;
                existingProduct.Stock = product.Stock;

                context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine($"Concurrency error updating product {product.Id}: {ex.Message}");
                throw;
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"EF Core UpdateProduct Error: {ex.InnerException?.Message ?? ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General UpdateProduct Error for ID {product.Id}: {ex.Message}");
                throw;
            }
        }

        public void AddImageToProduct(int productId, BuyProductImage image)
        {
            try
            {
                image.ProductId = productId;
                context.BuyProductImages.Add(image);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding image to product ID {productId}: {ex.Message}");
                throw;
            }
        }
    }
}
