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
    }
}