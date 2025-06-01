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

            // Validate required fields
            if (string.IsNullOrWhiteSpace(product.Title))
            {
                throw new ArgumentException("Product title is required", nameof(product.Title));
            }

            if (product.SellerId <= 0)
            {
                throw new ArgumentException("Valid seller ID is required", nameof(product.SellerId));
            }

            if (product.CategoryId <= 0)
            {
                throw new ArgumentException("Valid category ID is required", nameof(product.CategoryId));
            }

            if (product.ConditionId <= 0)
            {
                throw new ArgumentException("Valid condition ID is required", nameof(product.ConditionId));
            }

            Console.WriteLine($"Repository: Adding product with Title='{product.Title}', " +
                            $"SellerId={product.SellerId}, CategoryId={product.CategoryId}, " +
                            $"ConditionId={product.ConditionId}, Price={product.Price}, Stock={product.Stock}");

            // Validate foreign keys exist in database
            try
            {
                var sellerExists = context.Users.Any(u => u.Id == product.SellerId);
                if (!sellerExists)
                {
                    throw new ArgumentException($"Seller with ID {product.SellerId} does not exist", nameof(product.SellerId));
                }

                var categoryExists = context.ProductCategories.Any(c => c.Id == product.CategoryId);
                if (!categoryExists)
                {
                    throw new ArgumentException($"Category with ID {product.CategoryId} does not exist", nameof(product.CategoryId));
                }

                var conditionExists = context.ProductConditions.Any(c => c.Id == product.ConditionId);
                if (!conditionExists)
                {
                    throw new ArgumentException($"Condition with ID {product.ConditionId} does not exist", nameof(product.ConditionId));
                }

                Console.WriteLine("Repository: Foreign key validation passed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Repository: Foreign key validation failed: {ex.Message}");
                throw;
            }

            if (product.ProductTags != null && product.ProductTags.Any())
            {
                foreach (var productTag in product.ProductTags)
                {
                    // Ensure the Tag exists in the database
                    if (productTag.TagId > 0)
                    {
                        var existingTag = context.ProductTags.Find(productTag.TagId);
                        if (existingTag != null)
                        {
                            productTag.Tag = existingTag; // Attach the tracked entity
                        }
                        else
                        {
                            Console.WriteLine($"Warning: Tag with ID {productTag.TagId} not found in database");
                        }
                    }
                }
            }

            try
            {
                Console.WriteLine("Repository: Adding product to context");
                context.BuyProducts.Add(product);
                
                Console.WriteLine("Repository: Saving changes to database");
                context.SaveChanges();
                
                Console.WriteLine($"Repository: Product successfully saved with ID {product.Id}");
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"EF Core AddProduct Error: {ex.InnerException?.Message ?? ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Log more details about the validation errors
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception details: {ex.InnerException}");
                }
                
                throw new ApplicationException($"Database error while adding product: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General AddProduct Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
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
                    .Where(p => p.Stock > 0) // Filter out products with no stock
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

        public List<BuyProduct> GetProducts(int offset, int count)
        {
            try
            {
                var query = context.BuyProducts
                    .Include(p => p.Condition)
                    .Include(p => p.Category)
                    .Include(p => p.Images)
                    .Include(p => p.ProductTags)
                        .ThenInclude(pt => pt.Tag)
                    .Where(p => p.Stock > 0) // Filter out products with no stock
                    .OrderBy(p => p.Id); // Ensure consistent ordering for pagination

                List<BuyProduct> products;

                if (count > 0)
                {
                    // Apply pagination
                    products = query.Skip(offset).Take(count).ToList();
                }
                else
                {
                    // Return all products if count is 0
                    products = query.ToList();
                }

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
                Console.WriteLine($"Error getting BuyProducts with pagination (offset: {offset}, count: {count}): {ex.Message}");
                throw;
            }
        }

        public int GetProductCount()
        {
            try
            {
                return context.BuyProducts.Where(p => p.Stock > 0).Count();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting BuyProducts count: {ex.Message}");
                throw;
            }
        }

        public List<BuyProduct> GetFilteredProducts(int offset, int count, List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null)
        {
            try
            {
                var query = context.BuyProducts
                    .Include(p => p.Condition)
                    .Include(p => p.Category)
                    .Include(p => p.Images)
                    .Include(p => p.ProductTags)
                        .ThenInclude(pt => pt.Tag)
                    .AsQueryable();

                // Apply condition filter if provided
                if (conditionIds != null && conditionIds.Any())
                {
                    query = query.Where(p => conditionIds.Contains(p.ConditionId));
                }

                // Apply category filter if provided
                if (categoryIds != null && categoryIds.Any())
                {
                    query = query.Where(p => categoryIds.Contains(p.CategoryId));
                }

                // Apply maximum price filter if provided
                if (maxPrice.HasValue)
                {
                    query = query.Where(p => p.Price <= maxPrice.Value);
                }

                // Apply search term filter if provided
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    var lowerSearchTerm = searchTerm.ToLower();
                    query = query.Where(p =>
                        p.Title.ToLower().Contains(lowerSearchTerm) ||
                        p.Description.ToLower().Contains(lowerSearchTerm));
                }

                // Filter out products with no stock
                query = query.Where(p => p.Stock > 0);

                // Ensure consistent ordering for pagination
                query = query.OrderBy(p => p.Id);

                List<BuyProduct> products;

                if (count > 0)
                {
                    // Apply pagination
                    products = query.Skip(offset).Take(count).ToList();
                }
                else
                {
                    // Return all filtered products if count is 0
                    products = query.ToList();
                }

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
                Console.WriteLine($"Error getting filtered BuyProducts (offset: {offset}, count: {count}, conditions: {string.Join(",", conditionIds ?? new List<int>())}, categories: {string.Join(",", categoryIds ?? new List<int>())}): {ex.Message}");
                throw;
            }
        }

        public int GetFilteredProductCount(List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null)
        {
            try
            {
                var query = context.BuyProducts.AsQueryable();

                // Apply condition filter if provided
                if (conditionIds != null && conditionIds.Any())
                {
                    query = query.Where(p => conditionIds.Contains(p.ConditionId));
                }

                // Apply category filter if provided
                if (categoryIds != null && categoryIds.Any())
                {
                    query = query.Where(p => categoryIds.Contains(p.CategoryId));
                }

                // Apply maximum price filter if provided
                if (maxPrice.HasValue)
                {
                    query = query.Where(p => p.Price <= maxPrice.Value);
                }

                // Apply search term filter if provided
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    var lowerSearchTerm = searchTerm.ToLower();
                    query = query.Where(p =>
                        p.Title.ToLower().Contains(lowerSearchTerm) ||
                        p.Description.ToLower().Contains(lowerSearchTerm));
                }

                // Filter out products with no stock
                query = query.Where(p => p.Stock > 0);

                return query.Count();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting filtered BuyProducts count (conditions: {string.Join(",", conditionIds ?? new List<int>())}, categories: {string.Join(",", categoryIds ?? new List<int>())}, maxPrice: {maxPrice}, searchTerm: {searchTerm}): {ex.Message}");
                throw;
            }
        }

        public List<BuyProduct> GetFilteredProducts(int offset, int count, List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null, int? sellerId = null)
        {
            try
            {
                var query = context.BuyProducts
                    .Include(p => p.Condition)
                    .Include(p => p.Category)
                    .Include(p => p.Images)
                    .Include(p => p.ProductTags)
                        .ThenInclude(pt => pt.Tag)
                    .AsQueryable();

                // Apply condition filter if provided
                if (conditionIds != null && conditionIds.Any())
                {
                    query = query.Where(p => conditionIds.Contains(p.ConditionId));
                }

                // Apply category filter if provided
                if (categoryIds != null && categoryIds.Any())
                {
                    query = query.Where(p => categoryIds.Contains(p.CategoryId));
                }

                // Apply maximum price filter if provided
                if (maxPrice.HasValue)
                {
                    query = query.Where(p => p.Price <= maxPrice.Value);
                }

                // Apply search term filter if provided
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    var lowerSearchTerm = searchTerm.ToLower();
                    query = query.Where(p =>
                        p.Title.ToLower().Contains(lowerSearchTerm) ||
                        p.Description.ToLower().Contains(lowerSearchTerm));
                }

                // Apply seller filter if provided
                if (sellerId.HasValue)
                {
                    query = query.Where(p => p.SellerId == sellerId.Value);
                }

                // Filter out products with no stock
                query = query.Where(p => p.Stock > 0);

                // Ensure consistent ordering for pagination
                query = query.OrderBy(p => p.Id);

                List<BuyProduct> products;

                if (count > 0)
                {
                    // Apply pagination
                    products = query.Skip(offset).Take(count).ToList();
                }
                else
                {
                    // Return all filtered products if count is 0
                    products = query.ToList();
                }

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
                Console.WriteLine($"Error getting filtered BuyProducts (offset: {offset}, count: {count}, conditions: {string.Join(",", conditionIds ?? new List<int>())}, categories: {string.Join(",", categoryIds ?? new List<int>())}, sellerId: {sellerId}): {ex.Message}");
                throw;
            }
        }

        public int GetFilteredProductCount(List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null, int? sellerId = null)
        {
            try
            {
                var query = context.BuyProducts.AsQueryable();

                // Apply condition filter if provided
                if (conditionIds != null && conditionIds.Any())
                {
                    query = query.Where(p => conditionIds.Contains(p.ConditionId));
                }

                // Apply category filter if provided
                if (categoryIds != null && categoryIds.Any())
                {
                    query = query.Where(p => categoryIds.Contains(p.CategoryId));
                }

                // Apply maximum price filter if provided
                if (maxPrice.HasValue)
                {
                    query = query.Where(p => p.Price <= maxPrice.Value);
                }

                // Apply search term filter if provided
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    var lowerSearchTerm = searchTerm.ToLower();
                    query = query.Where(p =>
                        p.Title.ToLower().Contains(lowerSearchTerm) ||
                        p.Description.ToLower().Contains(lowerSearchTerm));
                }

                // Apply seller filter if provided
                if (sellerId.HasValue)
                {
                    query = query.Where(p => p.SellerId == sellerId.Value);
                }

                // Filter out products with no stock
                query = query.Where(p => p.Stock > 0);

                return query.Count();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting filtered BuyProducts count (conditions: {string.Join(",", conditionIds ?? new List<int>())}, categories: {string.Join(",", categoryIds ?? new List<int>())}, maxPrice: {maxPrice}, searchTerm: {searchTerm}, sellerId: {sellerId}): {ex.Message}");
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

        public async Task<double> GetMaxPriceAsync()
        {
            try
            {
                var maxPrice = await context.BuyProducts
                    .Where(p => p.Stock > 0) // Only products with stock
                    .MaxAsync(p => (double?)p.Price);
                
                return maxPrice ?? 0.0; // Return 0 if no products found
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting max price for BuyProducts: {ex.Message}");
                return 0.0; // Return 0 on error
            }
        }
    }
}
