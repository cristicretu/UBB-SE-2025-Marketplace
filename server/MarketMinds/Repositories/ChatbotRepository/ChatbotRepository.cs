using Server.DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;
using System.Data;

namespace MarketMinds.Repositories.ChatbotRepository
{
    public class ChatbotRepository : IChatbotRepository
    {
        private readonly ApplicationDbContext databaseContext;
        private readonly static int BUYER_TYPE_VALUE = 1;
        private readonly static int DEFAULT_PAGE_SIZE = 10;

        public ChatbotRepository(
            ApplicationDbContext newDatabaseContext)
        {
            this.databaseContext = newDatabaseContext;
        }

        public async Task<string> GetBotResponseAsync(string userMessage, int? userId = null)
        {
            return "I'm sorry, I couldn't process your request at this time.";
        }

        public async Task<User> GetUserAsync(int userId)
        {
            if (userId < 0)
            {
                return null;
            }

            return await databaseContext.Users
                .FirstOrDefaultAsync(user => user.Id == userId);
        }

        public async Task<Basket> GetUserBasketAsync(int userId)
        {
            // Baskets are replaced with ShoppingCart
            return null;
        }

        public async Task<List<BasketItem>> GetBasketItemsAsync(int basketId)
        {
            // BasketItems are replaced with ShoppingCart items
            return new List<BasketItem>();
        }

        public async Task<List<Product>> GetShoppingCartItemsAsync(int userId)
        {
            try
            {
                var cartItems = await databaseContext.BuyerCartItems
                    .Where(item => item.BuyerId == userId)
                    .ToListAsync();

                List<Product> products = new List<Product>();
                foreach (var cartItem in cartItems)
                {
                    var product = await databaseContext.BuyProducts.FindAsync(cartItem.ProductId);
                    if (product != null)
                    {
                        // Create a copy with quantity info from cart
                        var productWithQuantity = new BuyProduct(
                            product.Title,
                            product.Description,
                            product.SellerId,
                            product.ConditionId,
                            product.CategoryId,
                            product.Price
                        );
                        
                        productWithQuantity.Id = product.Id;
                        productWithQuantity.Stock = cartItem.Quantity;
                        products.Add(productWithQuantity);
                    }
                }

                return products;
            }
            catch (Exception)
            {
                return new List<Product>();
            }
        }

        public async Task<BuyProduct> GetBuyProductAsync(int productId)
        {
            return await databaseContext.BuyProducts
                .FirstOrDefaultAsync(product => product.Id == productId);
        }

        public async Task<List<Review>> GetReviewsGivenByUserAsync(int userId)
        {
            return await databaseContext.Reviews
                .Where(review => review.BuyerId == userId)
                .ToListAsync();
        }

        public async Task<List<Review>> GetReviewsReceivedByUserAsync(int userId)
        {
            return await databaseContext.Reviews
                .Where(review => review.SellerId == userId)
                .ToListAsync();
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            return await databaseContext.Users
                .FirstOrDefaultAsync(user => user.Id == userId);
        }

        public async Task<List<Order>> GetBuyerOrdersAsync(int userId)
        {
            return await databaseContext.Orders
                .Where(order => order.BuyerId == userId)
                .OrderByDescending(order => order.Id)
                .Take(DEFAULT_PAGE_SIZE)
                .ToListAsync();
        }

        public async Task<List<Order>> GetSellerOrdersAsync(int userId)
        {
            return await databaseContext.Orders
                .Where(order => order.SellerId == userId)
                .OrderByDescending(order => order.Id)
                .Take(DEFAULT_PAGE_SIZE)
                .ToListAsync();
        }

        public async Task<List<TrackedOrder>> GetTrackedOrdersAsync(int userId)
        {
            try
            {
                // First get user's orders
                var orders = await databaseContext.Orders
                    .Where(o => o.BuyerId == userId)
                    .ToListAsync();

                var trackedOrders = new List<TrackedOrder>();
                foreach (var order in orders)
                {
                    var trackedOrder = await databaseContext.TrackedOrders
                        .FirstOrDefaultAsync(to => to.OrderID == order.Id);
                    
                    if (trackedOrder != null)
                    {
                        trackedOrders.Add(trackedOrder);
                    }
                }

                return trackedOrders;
            }
            catch (Exception)
            {
                return new List<TrackedOrder>();
            }
        }

        public async Task<List<UserWaitList>> GetUserWaitlistsAsync(int userId)
        {
            try
            {
                return await databaseContext.UserWaitList
                    .Where(uwl => uwl.UserID == userId)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<UserWaitList>();
            }
        }

        public async Task<List<AuctionProduct>> GetUserAuctionProductsAsync(int userId)
        {
            try
            {
                return await databaseContext.AuctionProducts
                    .Where(ap => ap.SellerId == userId)
                    .Take(DEFAULT_PAGE_SIZE)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<AuctionProduct>();
            }
        }

        public async Task<List<Bid>> GetUserBidsAsync(int userId)
        {
            try
            {
                return await databaseContext.Bids
                    .Where(b => b.BidderId == userId)
                    .OrderByDescending(b => b.Timestamp)
                    .Take(DEFAULT_PAGE_SIZE)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<Bid>();
            }
        }

        public async Task<string> GetUserContextAsync(int userId)
        {
            return "Please use the ChatbotService for user context.";
        }
    }
}
