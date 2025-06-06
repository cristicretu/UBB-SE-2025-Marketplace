﻿using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.IRepository
{
    public interface IWaitListRepository
    {
        Task AddUserToWaitlist(int userId, int productId);
        Task AddUserToWaitlist(int userId, int productId, DateTime? preferredEndDate);
        Task<List<UserWaitList>> GetUsersInWaitlist(int productId);
        Task<List<UserWaitList>> GetUsersInWaitlistOrdered(int productId);
        Task<int> GetUserWaitlistPosition(int userId, int productId);
        Task<List<UserWaitList>> GetUserWaitlists(int userId);
        Task<int> GetWaitlistSize(int productId);
        Task<bool> IsUserInWaitlist(int userId, int productId);
        Task RemoveUserFromWaitlist(int userId, int productId);
        Task<UserWaitList?> GetUserWaitlistEntry(int userId, int productId);
    }
}
