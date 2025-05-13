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
                return context.BuyProducts
                    .Include(p => p.Seller)
                    .Include(p => p.Condition)
                    .Include(p => p.Category)
                    .Include(p => p.Images)
                    .Include(p => p.ProductTags)
                        .ThenInclude(pt => pt.Tag)
                    .ToList();
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
                return context.BuyProducts
                    .Include(p => p.Seller)
                    .Include(p => p.Condition)
                    .Include(p => p.Category)
                    .Include(p => p.Images)
                    .Include(p => p.ProductTags)
                        .ThenInclude(pt => pt.Tag)
                    .FirstOrDefault(p => p.Id == productId);
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
                context.Entry(product).State = EntityState.Modified;
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
