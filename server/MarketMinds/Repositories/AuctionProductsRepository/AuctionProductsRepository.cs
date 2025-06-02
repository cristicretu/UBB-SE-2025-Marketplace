using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;
using Server.DataAccessLayer;
using Microsoft.EntityFrameworkCore;


namespace MarketMinds.Repositories.AuctionProductsRepository
{
    public class AuctionProductsRepository : IAuctionProductsRepository
    {
        private readonly ApplicationDbContext context;

        public AuctionProductsRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        public List<AuctionProduct> GetProducts()
        {
            try
            {
                Console.WriteLine("DEBUG: AuctionProductsRepository.GetProducts - Starting to fetch products");

                var products = context.AuctionProducts
                    .Include(product => product.Condition)
                    .Include(product => product.Category)
                    .Include(product => product.Bids)
                    .Include(product => product.Images)
                    .ToList();

                foreach (var product in products)
                {
                    var productTags = context.Set<AuctionProductProductTag>()
                        .Where(apt => apt.ProductId == product.Id)
                        .Include(apt => apt.Tag)
                        .Select(apt => apt.Tag)
                        .ToList();
                    product.Tags = productTags;
                }
                // Load the bidder information for each product's bids and tags
                foreach (var product in products)
                {
                    // Load seller information
                    var seller = context.Users.FirstOrDefault(u => u.Id == product.SellerId);
                    if (seller != null)
                    {
                        product.Seller = seller;
                    }

                    foreach (var bid in product.Bids)
                    {
                        var bidderUser = context.Users.FirstOrDefault(u => u.Id == bid.BidderId);
                        if (bidderUser != null)
                        {
                            bid.Bidder = bidderUser;
                        }
                    }

                    // Load tags for this product (for paginated results)
                    var productTags = context.Set<AuctionProductProductTag>()
                        .Where(apt => apt.ProductId == product.Id)
                        .Include(apt => apt.Tag)
                        .Select(apt => apt.Tag)
                        .ToList();
                    product.Tags = productTags;
                }

                return products;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"ERROR: GetProducts exception: {exception.Message}");
                throw new Exception($"Failed to retrieve auction products: {exception.Message}", exception);
            }
        }

        public void DeleteProduct(AuctionProduct product)
        {
            try
            {
                context.AuctionProducts.Remove(product);
                context.SaveChanges();
            }
            catch (Exception exception)
            {
                throw new Exception($"Failed to delete auction product: {exception.Message}", exception);
            }
        }

        public void AddProduct(AuctionProduct product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            try
            {
                // Store the tags before adding the product
                var tags = product.Tags?.ToList();
                product.Tags = null; // Clear the tags to avoid EF trying to create them

                context.AuctionProducts.Add(product);
                context.SaveChanges();

                // Now add the tag relationships after the product has been saved and has an ID
                if (tags != null && tags.Any())
                {
                    foreach (var tag in tags)
                    {
                        var auctionProductTag = new AuctionProductProductTag
                        {
                            ProductId = product.Id,
                            TagId = tag.Id
                        };
                        context.Set<AuctionProductProductTag>().Add(auctionProductTag);
                    }
                    context.SaveChanges();
                }
            }
            catch (Exception exception)
            {
                throw new Exception($"Failed to add auction product: {exception.Message}", exception);
            }
        }


        public void UpdateProduct(AuctionProduct product)
        {
            try
            {
                var existingProduct = context.AuctionProducts.Find(product.Id);
                if (existingProduct == null)
                {
                    throw new KeyNotFoundException($"AuctionProduct with ID {product.Id} not found for update.");
                }

                context.Entry(existingProduct).CurrentValues.SetValues(product);

                if (product.Images != null && product.Images.Any())
                {

                    foreach (var image in product.Images)
                    {
                        if (image.Id == 0)
                        {
                            image.ProductId = product.Id;
                            context.Set<ProductImage>().Add(image);
                        }
                    }
                }

                context.SaveChanges();
            }
            catch (Exception exception)
            {
                throw new Exception($"Failed to update auction product: {exception.Message}", exception);
            }
        }

        public AuctionProduct GetProductByID(int id)
        {
            try
            {
                var product = context.AuctionProducts
                    .Include(product => product.Condition)
                    .Include(product => product.Category)
                    .Include(product => product.Bids)
                    .Include(product => product.Images)
                    .FirstOrDefault(product => product.Id == id);

                if (product == null)
                {
                    throw new KeyNotFoundException($"AuctionProduct with ID {id} not found.");
                }

                // Load tags for this product
                var productTags = context.Set<AuctionProductProductTag>()
                    .Where(apt => apt.ProductId == product.Id)
                    .Include(apt => apt.Tag)
                    .Select(apt => apt.Tag)
                    .ToList();
                product.Tags = productTags;

                // Load seller information
                var seller = context.Users.FirstOrDefault(u => u.Id == product.SellerId);
                if (seller != null)
                {
                    product.Seller = seller;
                }

                // Load the bidder information for each bid
                foreach (var bid in product.Bids)
                {
                    // Since we're ignoring the Bidder navigation property in the entity configuration,
                    // we need to manually load the User information and set it
                    var bidderUser = context.Users.FirstOrDefault(u => u.Id == bid.BidderId);
                    if (bidderUser != null)
                    {
                        bid.Bidder = bidderUser;
                    }
                }

                return product;
            }
            catch (KeyNotFoundException keyNotFoundException)
            {
                throw new KeyNotFoundException($"AuctionProduct with ID {id} not found.", keyNotFoundException);
            }
            catch (Exception exception)
            {
                throw new Exception($"Failed to retrieve auction product by ID: {exception.Message}", exception);
            }
        }

        public List<AuctionProduct> GetProducts(int offset, int count)
        {
            try
            {
                var query = context.AuctionProducts
                    .Include(product => product.Condition)
                    .Include(product => product.Category)
                    .Include(product => product.Bids)
                    .Include(product => product.Images)
                    .OrderBy(p => p.Id); // Ensure consistent ordering for pagination

                List<AuctionProduct> products;

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

                // Load the bidder information for each product's bids
                foreach (var product in products)
                {
                    // Load seller information
                    var seller = context.Users.FirstOrDefault(u => u.Id == product.SellerId);
                    if (seller != null)
                    {
                        product.Seller = seller;
                    }

                    foreach (var bid in product.Bids)
                    {
                        var bidderUser = context.Users.FirstOrDefault(u => u.Id == bid.BidderId);
                        if (bidderUser != null)
                        {
                            bid.Bidder = bidderUser;
                        }
                    }
                }

                return products;
            }
            catch (Exception exception)
            {
                throw new Exception($"Failed to retrieve auction products with pagination: {exception.Message}", exception);
            }
        }

        public int GetProductCount()
        {
            try
            {
                return context.AuctionProducts.Count();
            }
            catch (Exception exception)
            {
                Console.WriteLine($"ERROR: GetProductCount exception: {exception.Message}");
                throw new Exception($"Failed to get auction products count: {exception.Message}", exception);
            }
        }

        public List<AuctionProduct> GetFilteredProducts(int offset, int count, List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null)
        {
            try
            {
                Console.WriteLine($"DEBUG: AuctionProductsRepository.GetFilteredProducts - Starting to fetch filtered products (offset: {offset}, count: {count}, conditions: {string.Join(",", conditionIds ?? new List<int>())}, categories: {string.Join(",", categoryIds ?? new List<int>())}, maxPrice: {maxPrice}, searchTerm: {searchTerm})");

                var query = context.AuctionProducts
                    .Include(product => product.Condition)
                    .Include(product => product.Category)
                    .Include(product => product.Bids)
                    .Include(product => product.Images)
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
                    query = query.Where(p => p.CurrentPrice <= maxPrice.Value);
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

                List<AuctionProduct> products;

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

                Console.WriteLine($"DEBUG: AuctionProductsRepository.GetFilteredProducts - Fetched {products.Count} filtered products");

                // Load the bidder information for each product's bids
                foreach (var product in products)
                {
                    // Load seller information
                    var seller = context.Users.FirstOrDefault(u => u.Id == product.SellerId);
                    if (seller != null)
                    {
                        product.Seller = seller;
                        Console.WriteLine($"DEBUG: AuctionProductsRepository.GetFilteredProducts - Loaded seller for product {product.Id}: Id={seller.Id}, Username={seller.Username}");
                    }
                    else
                    {
                        Console.WriteLine($"DEBUG: AuctionProductsRepository.GetFilteredProducts - Could not find seller with ID {product.SellerId} for product {product.Id}");
                    }

                    foreach (var bid in product.Bids)
                    {
                        var bidderUser = context.Users.FirstOrDefault(u => u.Id == bid.BidderId);
                        if (bidderUser != null)
                        {
                            bid.Bidder = bidderUser;
                        }
                    }
                }

                return products;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"ERROR: GetFilteredProducts exception: {exception.Message}");
                if (exception.InnerException != null)
                {
                    Console.WriteLine($"ERROR: Inner exception: {exception.InnerException.Message}");
                }
                throw new Exception($"Failed to retrieve filtered auction products: {exception.Message}", exception);
            }
        }

        public int GetFilteredProductCount(List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null)
        {
            try
            {
                var query = context.AuctionProducts.AsQueryable();

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
                    query = query.Where(p => p.CurrentPrice <= maxPrice.Value);
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
            catch (Exception exception)
            {
                Console.WriteLine($"ERROR: GetFilteredProductCount exception: {exception.Message}");
                throw new Exception($"Failed to get filtered auction products count: {exception.Message}", exception);
            }
        }

        public List<AuctionProduct> GetFilteredProducts(int offset, int count, List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null, int? sellerId = null)
        {
            try
            {
                Console.WriteLine($"DEBUG: AuctionProductsRepository.GetFilteredProducts with seller filter - Starting to fetch filtered products (offset: {offset}, count: {count}, conditions: {string.Join(",", conditionIds ?? new List<int>())}, categories: {string.Join(",", categoryIds ?? new List<int>())}, maxPrice: {maxPrice}, searchTerm: {searchTerm}, sellerId: {sellerId})");

                var query = context.AuctionProducts
                    .Include(product => product.Condition)
                    .Include(product => product.Category)
                    .Include(product => product.Bids)
                    .Include(product => product.Images)
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
                    query = query.Where(p => p.CurrentPrice <= maxPrice.Value);
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

                List<AuctionProduct> products;

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

                Console.WriteLine($"DEBUG: AuctionProductsRepository.GetFilteredProducts with seller filter - Fetched {products.Count} filtered products");

                // Load the bidder information for each product's bids
                foreach (var product in products)
                {
                    // Load seller information
                    var seller = context.Users.FirstOrDefault(u => u.Id == product.SellerId);
                    if (seller != null)
                    {
                        product.Seller = seller;
                        Console.WriteLine($"DEBUG: AuctionProductsRepository.GetFilteredProducts with seller filter - Loaded seller for product {product.Id}: Id={seller.Id}, Username={seller.Username}");
                    }
                    else
                    {
                        Console.WriteLine($"DEBUG: AuctionProductsRepository.GetFilteredProducts with seller filter - Could not find seller with ID {product.SellerId} for product {product.Id}");
                    }

                    foreach (var bid in product.Bids)
                    {
                        var bidderUser = context.Users.FirstOrDefault(u => u.Id == bid.BidderId);
                        if (bidderUser != null)
                        {
                            bid.Bidder = bidderUser;
                        }
                    }
                }

                return products;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"ERROR: GetFilteredProducts with seller filter exception: {exception.Message}");
                if (exception.InnerException != null)
                {
                    Console.WriteLine($"ERROR: Inner exception: {exception.InnerException.Message}");
                }
                throw new Exception($"Failed to retrieve filtered auction products with seller filter: {exception.Message}", exception);
            }
        }

        public int GetFilteredProductCount(List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null, int? sellerId = null)
        {
            try
            {
                var query = context.AuctionProducts.AsQueryable();

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
                    query = query.Where(p => p.CurrentPrice <= maxPrice.Value);
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
            catch (Exception exception)
            {
                Console.WriteLine($"ERROR: GetFilteredProductCount with seller filter exception: {exception.Message}");
                throw new Exception($"Failed to get filtered auction products count with seller filter: {exception.Message}", exception);
            }
        }

        public async Task<double> GetMaxPriceAsync()
        {
            try
            {
                var maxPrice = await context.AuctionProducts
                    .MaxAsync(p => (double?)p.CurrentPrice);

                return maxPrice ?? 0.0; // Return 0 if no products found
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting max price for AuctionProducts: {ex.Message}");
                return 0.0; // Return 0 on error
            }
        }
    }
}