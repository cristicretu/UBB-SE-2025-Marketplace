using System.Collections.Generic;
using SharedClassLibrary.Domain;

namespace SharedClassLibrary.IRepository
{
    public interface IWaitListRepository
    {
        Task AddUserToWaitlist(int userId, int productId);
        Task<List<UserWaitList>> GetUsersInWaitlist(int productId);
        Task<List<UserWaitList>> GetUsersInWaitlistOrdered(int productId);
        Task<int> GetUserWaitlistPosition(int userId, int productId);
        Task<List<UserWaitList>> GetUserWaitlists(int userId);
        Task<int> GetWaitlistSize(int productId);
        Task<bool> IsUserInWaitlist(int userId, int productId);
        Task RemoveUserFromWaitlist(int userId, int productId);
    }
}
