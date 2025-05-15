using MarketMinds.Shared.Models;
namespace MarketMinds.Shared.Services
{
    public interface IAccountPageService
    {
        Task<User> GetUserAsync(int userId);
        Task<User> GetCurrentLoggedInUserAsync(User currentUser);
        Task<List<UserOrder>> GetUserOrdersAsync(int userId);
    }
}