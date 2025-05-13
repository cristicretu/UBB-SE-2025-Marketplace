using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.IRepository
{
    public interface IChatbotRepository
    {
        Task<string> GetBotResponseAsync(string userMessage, int? userId = null);
        
        Task<string> GetUserContextAsync(int userId);
        
        Task<User> GetUserAsync(int userId);
        Task<Basket> GetUserBasketAsync(int userId);
        Task<List<BasketItem>> GetBasketItemsAsync(int basketId);
        Task<BuyProduct> GetBuyProductAsync(int productId);
        Task<List<Review>> GetReviewsGivenByUserAsync(int userId);
        Task<List<Review>> GetReviewsReceivedByUserAsync(int userId);
        Task<User> GetUserByIdAsync(int userId);
        Task<List<Order>> GetBuyerOrdersAsync(int userId);
        Task<List<Order>> GetSellerOrdersAsync(int userId);
    }
} 