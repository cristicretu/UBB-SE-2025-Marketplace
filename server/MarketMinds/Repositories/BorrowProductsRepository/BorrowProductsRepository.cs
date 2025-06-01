using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;
using Server.DataAccessLayer;
using Microsoft.EntityFrameworkCore;

namespace Server.MarketMinds.Repositories.BorrowProductsRepository
{
    public class BorrowProductsRepository : IBorrowProductsRepository
    {
        private readonly ApplicationDbContext context;

        public BorrowProductsRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        public List<BorrowProduct> GetProducts()
        {
            var products = context.BorrowProducts
                .Include(product => product.Condition)
                .Include(product => product.Category)
                .ToList();

            foreach (var product in products)
            {
                LoadProductRelationships(product);
            }

            return products;
        }

        public List<BorrowProduct> GetProducts(int offset, int count)
        {
            var query = context.BorrowProducts
                .Include(product => product.Condition)
                .Include(product => product.Category)
                .OrderBy(p => p.Id); // Ensure consistent ordering for pagination

            List<BorrowProduct> products;

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

            foreach (var product in products)
            {
                LoadProductRelationships(product);
            }

            return products;
        }

        public int GetProductCount()
        {
            return context.BorrowProducts.Count();
        }

        public List<BorrowProduct> GetFilteredProducts(int offset, int count, List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null)
        {
            try
            {
                var query = context.BorrowProducts
                    .Include(product => product.Condition)
                    .Include(product => product.Category)
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
                    query = query.Where(p => p.DailyRate <= maxPrice.Value);
                }

                // Apply search term filter if provided
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    var lowerSearchTerm = searchTerm.ToLower();
                    query = query.Where(p =>
                        p.Title.ToLower().Contains(lowerSearchTerm) ||
                        p.Description.ToLower().Contains(lowerSearchTerm));
                }

                // Ensure consistent ordering for pagination
                query = query.OrderBy(p => p.Id);

                List<BorrowProduct> products;

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

                foreach (var product in products)
                {
                    LoadProductRelationships(product);
                }

                return products;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting filtered BorrowProducts (offset: {offset}, count: {count}, conditions: {string.Join(",", conditionIds ?? new List<int>())}, categories: {string.Join(",", categoryIds ?? new List<int>())}): {ex.Message}");
                throw;
            }
        }

        public int GetFilteredProductCount(List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null)
        {
            try
            {
                var query = context.BorrowProducts.AsQueryable();

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
                    query = query.Where(p => p.DailyRate <= maxPrice.Value);
                }

                // Apply search term filter if provided
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    var lowerSearchTerm = searchTerm.ToLower();
                    query = query.Where(p =>
                        p.Title.ToLower().Contains(lowerSearchTerm) ||
                        p.Description.ToLower().Contains(lowerSearchTerm));
                }

                return query.Count();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting filtered BorrowProducts count (conditions: {string.Join(",", conditionIds ?? new List<int>())}, categories: {string.Join(",", categoryIds ?? new List<int>())}, maxPrice: {maxPrice}, searchTerm: {searchTerm}): {ex.Message}");
                throw;
            }
        }

        public List<BorrowProduct> GetFilteredProducts(int offset, int count, List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null, int? sellerId = null)
        {
            try
            {
                var query = context.BorrowProducts
                    .Include(product => product.Condition)
                    .Include(product => product.Category)
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
                    query = query.Where(p => p.DailyRate <= maxPrice.Value);
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

                // Ensure consistent ordering for pagination
                query = query.OrderBy(p => p.Id);

                List<BorrowProduct> products;

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

                foreach (var product in products)
                {
                    LoadProductRelationships(product);
                }

                return products;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting filtered BorrowProducts with seller filter (offset: {offset}, count: {count}, conditions: {string.Join(",", conditionIds ?? new List<int>())}, categories: {string.Join(",", categoryIds ?? new List<int>())}, sellerId: {sellerId}): {ex.Message}");
                throw;
            }
        }

        public int GetFilteredProductCount(List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null, int? sellerId = null)
        {
            try
            {
                var query = context.BorrowProducts.AsQueryable();

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
                    query = query.Where(p => p.DailyRate <= maxPrice.Value);
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

                return query.Count();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting filtered BorrowProducts count with seller filter (conditions: {string.Join(",", conditionIds ?? new List<int>())}, categories: {string.Join(",", categoryIds ?? new List<int>())}, maxPrice: {maxPrice}, searchTerm: {searchTerm}, sellerId: {sellerId}): {ex.Message}");
                throw;
            }
        }

        public void DeleteProduct(BorrowProduct product)
        {
            context.BorrowProducts.Remove(product);
            context.SaveChanges();
        }

        public void AddProduct(BorrowProduct product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            // Store tags for processing after product creation
            var tagsToProcess = new List<ProductTag>();
            if (product.Tags != null && product.Tags.Any())
            {
                tagsToProcess = product.Tags.ToList();
            }

            if (product.ProductTags != null && product.ProductTags.Any())
            {
                foreach (var productTag in product.ProductTags)
                {
                    if (productTag.TagId > 0)
                    {
                        var existingTag = context.ProductTags.Find(productTag.TagId);
                        if (existingTag != null)
                        {
                            productTag.Tag = existingTag;
                        }
                    }
                }
            }

            context.BorrowProducts.Add(product);
            context.SaveChanges();

            // Process tags after product is saved and has an ID
            if (tagsToProcess.Any())
            {
                foreach (var tag in tagsToProcess)
                {
                    var borrowProductTag = new BorrowProductProductTag
                    {
                        ProductId = product.Id,
                        TagId = tag.Id
                    };
                    context.BorrowProductProductTags.Add(borrowProductTag);
                }
                context.SaveChanges();

            }
        }

        public void UpdateProduct(BorrowProduct product)
        {
            var existingProduct = context.BorrowProducts.Find(product.Id);
            if (existingProduct == null)
            {
                throw new KeyNotFoundException($"BorrowProduct with ID {product.Id} not found for update.");
            }

            context.Entry(existingProduct).CurrentValues.SetValues(product);

            if (product.Images != null && product.Images.Any())
            {
                foreach (var image in product.Images)
                {
                    if (image.Id == 0)
                    {
                        image.ProductId = product.Id;
                        context.Set<BorrowProductImage>().Add(image);
                    }
                }
            }

            context.SaveChanges();
        }

        public BorrowProduct GetProductByID(int id)
        {
            var product = context.BorrowProducts
                .Include(product => product.Condition)
                .Include(product => product.Category)
                .FirstOrDefault(product => product.Id == id);

            if (product == null)
            {
                throw new KeyNotFoundException($"BorrowProduct with ID {id} not found.");
            }

            if (product.SellerId != null)
            {
                var seller = context.Users.FirstOrDefault(u => u.Id == product.SellerId);
                if (seller != null)
                {
                    product.Seller = seller;
                }
            }

            LoadProductRelationships(product);

            return product;
        }

        public void AddImageToProduct(int productId, BorrowProductImage image)
        {
            image.ProductId = productId;
            context.BorrowProductImages.Add(image);
            context.SaveChanges();
        }

        private void LoadProductRelationships(BorrowProduct product)
        {
            context.Entry(product)
                .Collection(product => product.Images)
                .Load();

            context.Entry(product)
                .Collection(product => product.ProductTags)
                .Load();

            if (product.ProductTags != null && product.ProductTags.Any())
            {
                foreach (var productTag in product.ProductTags)
                {
                    context.Entry(productTag)
                        .Reference(pt => pt.Tag)
                        .Load();
                }
            }

            // Load tags into the Tags property from ProductTags
            var tags = context.BorrowProductProductTags
                .Where(bppt => bppt.ProductId == product.Id)
                .Include(bppt => bppt.Tag)
                .Select(bppt => bppt.Tag)
                .ToList();

            product.Tags = tags;
        }

        public async Task<double> GetMaxPriceAsync()
        {
            try
            {
                var maxPrice = await context.BorrowProducts
                    .MaxAsync(p => (double?)p.DailyRate);
                
                return maxPrice ?? 0.0; // Return 0 if no products found
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting max price for BorrowProducts: {ex.Message}");
                return 0.0; // Return 0 on error
            }
        }
    }
}