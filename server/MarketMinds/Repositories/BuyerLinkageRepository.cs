using MarketMinds.Shared.Models;
using MarketMinds.Shared.Repositories;
using MarketMinds.Shared.Services;
using Microsoft.EntityFrameworkCore;
using Server.DataAccessLayer;
using Server.DataModels;
using BuyerLinkageStatus = MarketMinds.Shared.Services.BuyerLinkageStatus;

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
        /// Creates a new pending linkage request between two buyers
        /// </summary>
        /// <param name="requestingBuyerId">Requesting buyer ID</param>
        /// <param name="receivingBuyerId">Receiving buyer ID</param>
        /// <returns>The created buyer linkage request</returns>
        public async Task<BuyerLinkage> CreateLinkageRequestAsync(int requestingBuyerId, int receivingBuyerId)
        {
            if (requestingBuyerId == receivingBuyerId)
            {
                throw new ArgumentException("A buyer cannot link to themselves");
            }

            // Check if any linkage already exists (pending or approved)
            var existingLinkage = await GetLinkageEntityAsync(requestingBuyerId, receivingBuyerId);
            if (existingLinkage != null)
            {
                throw new InvalidOperationException("A linkage request already exists between these buyers");
            }

            // Create entity with requesting and receiving buyer IDs (maintain order)
            var linkageEntity = new BuyerLinkageEntity
            {
                RequestingBuyerId = requestingBuyerId,
                ReceivingBuyerId = receivingBuyerId,
                IsApproved = false // Start as pending
            };

            _context.BuyerLinkages.Add(linkageEntity);
            await _context.SaveChangesAsync();

            // Convert entity to model
            return new BuyerLinkage(requestingBuyerId, receivingBuyerId);
        }

        /// <summary>
        /// Creates a new linkage between two buyers (DEPRECATED - use CreateLinkageRequestAsync)
        /// </summary>
        /// <param name="buyerId1">First buyer ID</param>
        /// <param name="buyerId2">Second buyer ID</param>
        /// <returns>The created buyer linkage</returns>
        public async Task<BuyerLinkage> CreateLinkageAsync(int buyerId1, int buyerId2)
        {
            // For backward compatibility, redirect to the new request-based system
            return await CreateLinkageRequestAsync(buyerId1, buyerId2);
        }

        /// <summary>
        /// Approves a pending linkage request
        /// </summary>
        /// <param name="requestingBuyerId">Requesting buyer ID</param>
        /// <param name="receivingBuyerId">Receiving buyer ID</param>
        /// <returns>True if request was approved, false if not found</returns>
        public async Task<bool> ApproveLinkageRequestAsync(int requestingBuyerId, int receivingBuyerId)
        {
            var linkageEntity = await _context.BuyerLinkages
                .FirstOrDefaultAsync(bl => bl.RequestingBuyerId == requestingBuyerId && bl.ReceivingBuyerId == receivingBuyerId);

            if (linkageEntity == null || linkageEntity.IsApproved)
            {
                return false; // Request not found or already approved
            }

            linkageEntity.IsApproved = true;
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Removes a linkage or linkage request between two buyers
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
        /// Checks if two buyers are linked (approved linkage only)
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
        /// Gets the linkage status between two buyers
        /// </summary>
        /// <param name="currentBuyerId">Current buyer ID</param>
        /// <param name="targetBuyerId">Target buyer ID</param>
        /// <returns>The linkage status</returns>
        public async Task<BuyerLinkageStatus> GetLinkageStatusAsync(int currentBuyerId, int targetBuyerId)
        {
            if (currentBuyerId == targetBuyerId)
            {
                return BuyerLinkageStatus.None;
            }

            var linkageEntity = await GetLinkageEntityAsync(currentBuyerId, targetBuyerId);
            if (linkageEntity == null)
            {
                return BuyerLinkageStatus.None;
            }

            if (linkageEntity.IsApproved)
            {
                return BuyerLinkageStatus.Linked;
            }

            // Check if current user is the one who sent the request or received it
            if (linkageEntity.RequestingBuyerId == currentBuyerId)
            {
                return BuyerLinkageStatus.PendingSent;
            }
            else
            {
                return BuyerLinkageStatus.PendingReceived;
            }
        }

        /// <summary>
        /// Gets a specific linkage between two buyers (approved only)
        /// </summary>
        /// <param name="buyerId1">First buyer ID</param>
        /// <param name="buyerId2">Second buyer ID</param>
        /// <returns>The buyer linkage if it exists and is approved, null otherwise</returns>
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
        /// Gets all approved linkages for a specific buyer
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
        /// Gets all linked buyer IDs for a specific buyer (approved linkages only)
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
        /// Helper method to get a linkage entity between two buyers (checks both directions)
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

            // Check both directions since we maintain the requesting/receiving order
            return await _context.BuyerLinkages
                .FirstOrDefaultAsync(bl => 
                    (bl.RequestingBuyerId == buyerId1 && bl.ReceivingBuyerId == buyerId2) ||
                    (bl.RequestingBuyerId == buyerId2 && bl.ReceivingBuyerId == buyerId1));
        }
    }
} 