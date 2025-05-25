using MarketMinds.Shared.Models;
using MarketMinds.Shared.Repositories;
using MarketMinds.Shared.IRepository;
using Microsoft.EntityFrameworkCore;
using Server.DataAccessLayer;
using Server.DataModels;

namespace Server.MarketMinds.Repositories
{
    /// <summary>
    /// Server-side repository implementation for managing buyer-seller follows
    /// Uses the existing FollowingEntity table and IBuyerRepository for follow operations
    /// </summary>
    public class BuyerSellerFollowRepository : IBuyerSellerFollowRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IBuyerRepository _buyerRepository;

        /// <summary>
        /// Initializes a new instance of the BuyerSellerFollowRepository class
        /// </summary>
        /// <param name="context">Database context</param>
        /// <param name="buyerRepository">Buyer repository for follow operations</param>
        public BuyerSellerFollowRepository(ApplicationDbContext context, IBuyerRepository buyerRepository)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _buyerRepository = buyerRepository ?? throw new ArgumentNullException(nameof(buyerRepository));
        }

        /// <summary>
        /// Creates a new follow relationship where a buyer follows a seller
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <param name="sellerId">Seller ID</param>
        /// <returns>The created buyer-seller follow</returns>
        public async Task<BuyerSellerFollow> CreateFollowAsync(int buyerId, int sellerId)
        {
            if (buyerId <= 0 || sellerId <= 0)
            {
                throw new ArgumentException("Invalid buyer or seller ID");
            }

            if (buyerId == sellerId)
            {
                throw new ArgumentException("A buyer cannot follow themselves");
            }

            // Check if follow relationship already exists
            var alreadyFollowing = await _buyerRepository.IsFollowing(buyerId, sellerId);
            if (alreadyFollowing)
            {
                throw new InvalidOperationException("Buyer is already following this seller");
            }

            // Check if buyer and seller exist
            var buyerExists = await _buyerRepository.CheckIfBuyerExists(buyerId);
            var sellerExists = await _context.Sellers.AnyAsync(s => s.Id == sellerId);

            if (!buyerExists)
            {
                throw new ArgumentException($"Buyer with ID {buyerId} not found");
            }

            if (!sellerExists)
            {
                throw new ArgumentException($"Seller with ID {sellerId} not found");
            }

            // Use existing follow functionality
            await _buyerRepository.FollowSeller(buyerId, sellerId);

            return new BuyerSellerFollow(buyerId, sellerId);
        }

        /// <summary>
        /// Removes a follow relationship between a buyer and seller
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <param name="sellerId">Seller ID</param>
        /// <returns>True if follow was removed, false if it didn't exist</returns>
        public async Task<bool> RemoveFollowAsync(int buyerId, int sellerId)
        {
            if (buyerId <= 0 || sellerId <= 0)
            {
                return false;
            }

            // Check if follow relationship exists
            var isFollowing = await _buyerRepository.IsFollowing(buyerId, sellerId);
            if (!isFollowing)
            {
                return false;
            }

            // Use existing unfollow functionality
            await _buyerRepository.UnfollowSeller(buyerId, sellerId);
            return true;
        }

        /// <summary>
        /// Checks if a buyer is following a seller
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <param name="sellerId">Seller ID</param>
        /// <returns>True if buyer is following seller, false otherwise</returns>
        public async Task<bool> IsFollowingAsync(int buyerId, int sellerId)
        {
            if (buyerId <= 0 || sellerId <= 0)
            {
                return false;
            }

            return await _buyerRepository.IsFollowing(buyerId, sellerId);
        }

        /// <summary>
        /// Gets a specific follow relationship between a buyer and seller
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <param name="sellerId">Seller ID</param>
        /// <returns>The buyer-seller follow if it exists, null otherwise</returns>
        public async Task<BuyerSellerFollow?> GetFollowAsync(int buyerId, int sellerId)
        {
            var isFollowing = await IsFollowingAsync(buyerId, sellerId);
            return isFollowing ? new BuyerSellerFollow(buyerId, sellerId) : null;
        }

        /// <summary>
        /// Gets all follows for a specific buyer (all sellers they follow)
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <returns>List of buyer-seller follows for the specified buyer</returns>
        public async Task<IEnumerable<BuyerSellerFollow>> GetBuyerFollowsAsync(int buyerId)
        {
            if (buyerId <= 0)
            {
                return new List<BuyerSellerFollow>();
            }

            var followedSellerIds = await _buyerRepository.GetFollowingUsersIds(buyerId);
            return followedSellerIds.Select(sellerId => new BuyerSellerFollow(buyerId, sellerId));
        }

        /// <summary>
        /// Gets all followers for a specific seller (all buyers following them)
        /// </summary>
        /// <param name="sellerId">Seller ID</param>
        /// <returns>List of buyer-seller follows for the specified seller</returns>
        public async Task<IEnumerable<BuyerSellerFollow>> GetSellerFollowersAsync(int sellerId)
        {
            if (sellerId <= 0)
            {
                return new List<BuyerSellerFollow>();
            }

            // Query FollowingEntity table directly for followers of this seller
            var followerIds = await _context.Followings
                .Where(f => f.SellerId == sellerId)
                .Select(f => f.BuyerId)
                .ToListAsync();

            return followerIds.Select(buyerId => new BuyerSellerFollow(buyerId, sellerId));
        }

        /// <summary>
        /// Gets all seller IDs that a buyer is following
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <returns>List of seller IDs that the buyer is following</returns>
        public async Task<IEnumerable<int>> GetFollowedSellerIdsAsync(int buyerId)
        {
            if (buyerId <= 0)
            {
                return new List<int>();
            }

            return await _buyerRepository.GetFollowingUsersIds(buyerId);
        }

        /// <summary>
        /// Gets all buyer IDs that are following a seller
        /// </summary>
        /// <param name="sellerId">Seller ID</param>
        /// <returns>List of buyer IDs that are following the seller</returns>
        public async Task<IEnumerable<int>> GetFollowerBuyerIdsAsync(int sellerId)
        {
            if (sellerId <= 0)
            {
                return new List<int>();
            }

            return await _context.Followings
                .Where(f => f.SellerId == sellerId)
                .Select(f => f.BuyerId)
                .ToListAsync();
        }
    }
} 