using MarketMinds.Shared.Models;
using MarketMinds.Shared.Repositories;
using Microsoft.EntityFrameworkCore;
using Server.DataAccessLayer;
using Server.DataModels;

namespace Server.MarketMinds.Repositories
{
    /// <summary>
    /// Server-side repository implementation for managing buyer linkages
    /// </summary>
    public class BuyerLinkageRepository : IBuyerLinkageRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the BuyerLinkageRepository class
        /// </summary>
        /// <param name="context">Database context</param>
        public BuyerLinkageRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Creates a new linkage between two buyers
        /// </summary>
        /// <param name="buyerId1">First buyer ID</param>
        /// <param name="buyerId2">Second buyer ID</param>
        /// <returns>The created buyer linkage</returns>
        public async Task<BuyerLinkage> CreateLinkageAsync(int buyerId1, int buyerId2)
        {
            if (buyerId1 == buyerId2)
            {
                throw new ArgumentException("A buyer cannot link to themselves");
            }

            // Check if linkage already exists
            var existingLinkage = await GetLinkageAsync(buyerId1, buyerId2);
            if (existingLinkage != null)
            {
                throw new InvalidOperationException("Buyers are already linked");
            }

            // Create entity with consistent ordering (requesting = smaller ID, receiving = larger ID)
            var requestingBuyerId = Math.Min(buyerId1, buyerId2);
            var receivingBuyerId = Math.Max(buyerId1, buyerId2);

            var linkageEntity = new BuyerLinkageEntity
            {
                RequestingBuyerId = requestingBuyerId,
                ReceivingBuyerId = receivingBuyerId,
                IsApproved = true // For now, auto-approve linkages
            };

            _context.BuyerLinkages.Add(linkageEntity);
            await _context.SaveChangesAsync();

            // Convert entity to model
            return new BuyerLinkage(requestingBuyerId, receivingBuyerId);
        }

        /// <summary>
        /// Removes a linkage between two buyers
        /// </summary>
        /// <param name="buyerId1">First buyer ID</param>
        /// <param name="buyerId2">Second buyer ID</param>
        /// <returns>True if linkage was removed, false if it didn't exist</returns>
        public async Task<bool> RemoveLinkageAsync(int buyerId1, int buyerId2)
        {
            var linkageEntity = await GetLinkageEntityAsync(buyerId1, buyerId2);
            if (linkageEntity == null)
            {
                return false;
            }

            _context.BuyerLinkages.Remove(linkageEntity);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Checks if two buyers are linked
        /// </summary>
        /// <param name="buyerId1">First buyer ID</param>
        /// <param name="buyerId2">Second buyer ID</param>
        /// <returns>True if buyers are linked, false otherwise</returns>
        public async Task<bool> AreBuyersLinkedAsync(int buyerId1, int buyerId2)
        {
            if (buyerId1 == buyerId2)
            {
                return false; // A buyer is not "linked" to themselves
            }

            var linkageEntity = await GetLinkageEntityAsync(buyerId1, buyerId2);
            return linkageEntity != null && linkageEntity.IsApproved;
        }

        /// <summary>
        /// Gets a specific linkage between two buyers
        /// </summary>
        /// <param name="buyerId1">First buyer ID</param>
        /// <param name="buyerId2">Second buyer ID</param>
        /// <returns>The buyer linkage if it exists, null otherwise</returns>
        public async Task<BuyerLinkage?> GetLinkageAsync(int buyerId1, int buyerId2)
        {
            var linkageEntity = await GetLinkageEntityAsync(buyerId1, buyerId2);
            if (linkageEntity == null || !linkageEntity.IsApproved)
            {
                return null;
            }

            return new BuyerLinkage(linkageEntity.RequestingBuyerId, linkageEntity.ReceivingBuyerId);
        }

        /// <summary>
        /// Gets all linkages for a specific buyer
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <returns>List of buyer linkages involving the specified buyer</returns>
        public async Task<IEnumerable<BuyerLinkage>> GetBuyerLinkagesAsync(int buyerId)
        {
            var linkageEntities = await _context.BuyerLinkages
                .Where(bl => (bl.RequestingBuyerId == buyerId || bl.ReceivingBuyerId == buyerId) && bl.IsApproved)
                .ToListAsync();

            return linkageEntities.Select(entity => new BuyerLinkage(entity.RequestingBuyerId, entity.ReceivingBuyerId));
        }

        /// <summary>
        /// Gets all linked buyer IDs for a specific buyer
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <returns>List of buyer IDs that are linked to the specified buyer</returns>
        public async Task<IEnumerable<int>> GetLinkedBuyerIdsAsync(int buyerId)
        {
            var linkageEntities = await _context.BuyerLinkages
                .Where(bl => (bl.RequestingBuyerId == buyerId || bl.ReceivingBuyerId == buyerId) && bl.IsApproved)
                .ToListAsync();

            var linkedBuyerIds = new List<int>();
            foreach (var entity in linkageEntities)
            {
                if (entity.RequestingBuyerId == buyerId)
                {
                    linkedBuyerIds.Add(entity.ReceivingBuyerId);
                }
                else
                {
                    linkedBuyerIds.Add(entity.RequestingBuyerId);
                }
            }

            return linkedBuyerIds;
        }

        /// <summary>
        /// Helper method to get a linkage entity with consistent ordering
        /// </summary>
        /// <param name="buyerId1">First buyer ID</param>
        /// <param name="buyerId2">Second buyer ID</param>
        /// <returns>The linkage entity if found, null otherwise</returns>
        private async Task<BuyerLinkageEntity?> GetLinkageEntityAsync(int buyerId1, int buyerId2)
        {
            if (buyerId1 == buyerId2)
            {
                return null;
            }

            // Use consistent ordering to match how we store linkages
            var requestingBuyerId = Math.Min(buyerId1, buyerId2);
            var receivingBuyerId = Math.Max(buyerId1, buyerId2);

            return await _context.BuyerLinkages
                .FirstOrDefaultAsync(bl => bl.RequestingBuyerId == requestingBuyerId && bl.ReceivingBuyerId == receivingBuyerId);
        }
    }
} 