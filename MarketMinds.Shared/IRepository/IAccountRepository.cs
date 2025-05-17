using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.IRepository
{
    public interface IAccountRepository
    {
        Task<List<Order>> CreateOrderFromBasketAsync(int userId, int basketId, double discountAmount = 0);
        Task<User> GetUserByIdAsync(int userId);
        Task<List<UserOrder>> GetUserOrdersAsync(int userId);
        Task<double> GetBasketTotalAsync(int userId, int basketId);
        Task<bool> UpdateUserAsync(User user);
    }
}