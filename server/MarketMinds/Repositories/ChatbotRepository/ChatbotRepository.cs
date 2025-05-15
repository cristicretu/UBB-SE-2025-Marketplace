using Server.DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;

namespace MarketMinds.Repositories.ChatbotRepository
{
    public class ChatbotRepository : IChatbotRepository
    {
        private readonly ApplicationDbContext databaseContext;
        private readonly static int BUYER_TYPE_VALUE = 1;

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
            if (userId <= 0)
            {
                return null;
            }

            return await databaseContext.Users
                .FirstOrDefaultAsync(user => user.Id == userId);
        }

        public async Task<Basket> GetUserBasketAsync(int userId)
        {
            return await databaseContext.Baskets
                .FirstOrDefaultAsync(basket => basket.BuyerId == userId);
        }

        public async Task<List<BasketItem>> GetBasketItemsAsync(int basketId)
        {
            return await databaseContext.BasketItems
                .Where(item => item.BasketId == basketId)
                .ToListAsync();
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
                .ToListAsync();
        }

        public async Task<List<Order>> GetSellerOrdersAsync(int userId)
        {
            return await databaseContext.Orders
                .Where(order => order.SellerId == userId)
                .OrderByDescending(order => order.Id)
                .ToListAsync();
        }

        public async Task<string> GetUserContextAsync(int userId)
        {
            return "Please use the ChatbotService for user context.";
        }
    }
}
