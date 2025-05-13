using System.Threading.Tasks;
using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.IRepository
{
    public interface IUserRepository
    {
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
        Task<User> RegisterUserAsync(string username, string email, string passwordHash);
        Task<User> FindUserByUsernameAsync(string username);
    }
} 