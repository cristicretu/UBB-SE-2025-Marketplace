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
                
                Console.WriteLine($"DEBUG: AuctionProductsRepository.GetProducts - Fetched {products.Count} products");
                
                // Log price information for debugging
                foreach (var product in products)
                {
                    Console.WriteLine($"DEBUG: AuctionProduct {product.Id}: StartPrice={product.StartPrice} (type: {product.StartPrice.GetType().Name}), CurrentPrice={product.CurrentPrice} (type: {product.CurrentPrice.GetType().Name})");
                    
                    if (product.Bids != null && product.Bids.Any())
                    {
                        foreach (var bid in product.Bids)
                        {
                            Console.WriteLine($"DEBUG: Bid {bid.Id} on product {product.Id}: Price={bid.Price} (type: {bid.Price.GetType().Name})");
                        }
                    }
                }
                
                // Load the bidder information for each product's bids
                foreach (var product in products)
                {
                    // Load seller information
                    var seller = context.Users.FirstOrDefault(u => u.Id == product.SellerId);
                    if (seller != null)
                    {
                        product.Seller = seller;
                        Console.WriteLine($"DEBUG: AuctionProductsRepository.GetProducts - Loaded seller for product {product.Id}: Id={seller.Id}, Username={seller.Username}");
                    }
                    else
                    {
                        Console.WriteLine($"DEBUG: AuctionProductsRepository.GetProducts - Could not find seller with ID {product.SellerId} for product {product.Id}");
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
                
                // Debug statement to check if categories are loaded
                Console.WriteLine("========== DEBUG: AuctionProductsRepository.GetProducts ==========");
                foreach (var product in products)
                {
                    Console.WriteLine($"Product ID: {product.Id}, Title: {product.Title}");
                    Console.WriteLine($"Category ID: {product.CategoryId}, Category object: {(product.Category != null ? "Loaded" : "NULL")}");
                    if (product.Category != null)
                    {
                        Console.WriteLine($"Category Name: {product.Category.Name}, Description: {product.Category.Description}");
                    }
                }
                Console.WriteLine("================================================================");

                return products;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"ERROR: GetProducts exception: {exception.Message}");
                if (exception.InnerException != null)
                {
                    Console.WriteLine($"ERROR: Inner exception: {exception.InnerException.Message}");
                }
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
            try
            {
                context.AuctionProducts.Add(product);
                context.SaveChanges();
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

                // Load seller information
                var seller = context.Users.FirstOrDefault(u => u.Id == product.SellerId);
                if (seller != null)
                {
                    product.Seller = seller;
                    Console.WriteLine($"DEBUG: AuctionProductsRepository.GetProductByID - Loaded seller: Id={seller.Id}, Username={seller.Username}");
                }
                else
                {
                    Console.WriteLine($"DEBUG: AuctionProductsRepository.GetProductByID - Could not find seller with ID {product.SellerId}");
                }

                // Log condition data
                Console.WriteLine($"DEBUG: AuctionProductsRepository.GetProductByID - Product {id}, ConditionId={product.ConditionId}");
                if (product.Condition != null)
                {
                    Console.WriteLine($"DEBUG: AuctionProductsRepository.GetProductByID - Condition loaded: Id={product.Condition.Id}, Name={product.Condition.Name}, Description={product.Condition.Description}");
                }
                else
                {
                    Console.WriteLine($"DEBUG: AuctionProductsRepository.GetProductByID - Condition is NULL despite Include!");
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
                Console.WriteLine($"DEBUG: AuctionProductsRepository.GetProducts - Starting to fetch products with pagination (offset: {offset}, count: {count})");
                
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
                
                Console.WriteLine($"DEBUG: AuctionProductsRepository.GetProducts - Fetched {products.Count} products with pagination");
                
                // Load the bidder information for each product's bids
                foreach (var product in products)
                {
                    // Load seller information
                    var seller = context.Users.FirstOrDefault(u => u.Id == product.SellerId);
                    if (seller != null)
                    {
                        product.Seller = seller;
                        Console.WriteLine($"DEBUG: AuctionProductsRepository.GetProducts - Loaded seller for product {product.Id}: Id={seller.Id}, Username={seller.Username}");
                    }
                    else
                    {
                        Console.WriteLine($"DEBUG: AuctionProductsRepository.GetProducts - Could not find seller with ID {product.SellerId} for product {product.Id}");
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
                Console.WriteLine($"ERROR: GetProducts with pagination exception: {exception.Message}");
                if (exception.InnerException != null)
                {
                    Console.WriteLine($"ERROR: Inner exception: {exception.InnerException.Message}");
                }
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
    }
}