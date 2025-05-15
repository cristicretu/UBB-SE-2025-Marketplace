// <copyright file="BuyerRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Server.DataModels;
    using Server.DBConnection;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.IRepository;

    /// <summary>
    /// Repository class for managing buyer-related database operations.
    /// </summary>
    /// <param name="dbContext">The database context instance.</param>
    public class BuyerRepository : IBuyerRepository
    {
        private readonly MarketPlaceDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuyerRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context instance.</param>
        public BuyerRepository(MarketPlaceDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public async Task LoadBuyerInfo(Buyer buyerEntity)
        {
            int buyerId = buyerEntity.Id;
            Buyer buyer = await this.dbContext.Buyers.FindAsync(buyerId)
                                ?? throw new Exception("LoadBuyerInfo: Buyer not found");

            int billingAddressId = await this.dbContext.Buyers.Where(buyer => buyer.Id == buyerId)
                                .Select(buyer => EF.Property<int>(buyer, "BillingAddressId"))
                                .FirstOrDefaultAsync();
            int shippingAddressId = await this.dbContext.Buyers.Where(buyer => buyer.Id == buyerId)
                                .Select(buyer => EF.Property<int>(buyer, "ShippingAddressId"))
                                .FirstOrDefaultAsync();

            User user = await this.dbContext.Users.FindAsync(buyerId)
                                ?? throw new Exception("LoadBuyerInfo: User not found");

            buyerEntity.Badge = buyer.Badge;
            buyerEntity.Wishlist = await this.GetWishlist(buyerId);
            buyerEntity.Linkages = await this.GetBuyerLinkages(buyerId);
            buyerEntity.TotalSpending = buyer.TotalSpending;
            buyerEntity.NumberOfPurchases = buyer.NumberOfPurchases;
            buyerEntity.Discount = buyer.Discount;
            buyerEntity.UseSameAddress = buyer.UseSameAddress;
            buyerEntity.FollowingUsersIds = await this.GetFollowingUsersIds(buyerId);
            buyerEntity.User = user;
            buyerEntity.FirstName = buyer.FirstName;
            buyerEntity.LastName = buyer.LastName;
            buyerEntity.BillingAddress = await this.LoadAddress(billingAddressId)
                                ?? throw new Exception("LoadBuyerInfo: Billing address not found");
            if (buyer.UseSameAddress)
            {
                buyerEntity.ShippingAddress = buyer.BillingAddress;
            }
            else
            {
                buyerEntity.ShippingAddress = await this.LoadAddress(shippingAddressId)
                                ?? throw new Exception("LoadBuyerInfo: Shipping address not found");
            }

            // SyncedBuyerIds is not used in the application, so it is not loaded
        }

        /// <inheritdoc/>
        public async Task SaveInfo(Buyer buyerEntity)
        {
            if (!await this.CheckIfBuyerExists(buyerEntity.Id)) // This likely uses a different context or check logic? Be careful if it uses the same context.
            {
                throw new Exception("SaveInfo: Buyer not found");
            }

            // --- Address Handling ---

            // 1. Handle Billing Address: Check if it's already tracked locally
            var trackedBillingAddress = this.dbContext.Set<Address>().Local.FirstOrDefault(address => address.Id == buyerEntity.BillingAddress.Id && address.Id != 0);
            if (trackedBillingAddress != null)
            {
                // If tracked, copy updated values from the incoming entity to the tracked entity
                this.dbContext.Entry(trackedBillingAddress).CurrentValues.SetValues(buyerEntity.BillingAddress);

                // **Crucially, update the buyerEntity to point to the tracked instance**
                buyerEntity.BillingAddress = trackedBillingAddress;
            }
            else
            {
                // If not tracked locally, tell EF Core to handle this instance.
                // Update will mark it as Added (if Id=0) or Modified (if Id!=0 and detached).
                this.dbContext.Update(buyerEntity.BillingAddress);
            }

            // 2. Handle Shipping Address:
            if (buyerEntity.UseSameAddress)
            {
                // **Ensure ShippingAddress points to the exact same instance as BillingAddress**
                // This instance is now either the pre-tracked one or the one Update() will handle.
                buyerEntity.ShippingAddress = buyerEntity.BillingAddress;
            }
            else
            {
                // If using a different shipping address, handle its tracking status
                var trackedShippingAddress = this.dbContext.Set<Address>().Local.FirstOrDefault(address => address.Id == buyerEntity.ShippingAddress.Id && address.Id != 0);
                if (trackedShippingAddress != null)
                {
                    this.dbContext.Entry(trackedShippingAddress).CurrentValues.SetValues(buyerEntity.ShippingAddress);

                    // **Update buyerEntity to point to the tracked instance**
                    buyerEntity.ShippingAddress = trackedShippingAddress;
                }
                else
                {
                    // If not tracked locally, let EF Core handle this instance.
                    this.dbContext.Update(buyerEntity.ShippingAddress);
                }
            }

            // --- Buyer Update ---
            // Now that the Address navigation properties point to instances EF Core
            // can manage without conflict, update the Buyer entity itself.
            this.dbContext.Buyers.Update(buyerEntity);

            // --- Save ---
            await this.dbContext.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<BuyerWishlist> GetWishlist(int buyerId)
        {
            List<BuyerWishlistItemsEntity> buyerWishlistItems = await this.dbContext.BuyersWishlistItems.Where(wishlistItem => wishlistItem.BuyerId == buyerId).ToListAsync();
            BuyerWishlist buyerWishlist = new BuyerWishlist();
            foreach (BuyerWishlistItemsEntity item in buyerWishlistItems)
            {
                buyerWishlist.Items.Add(new BuyerWishlistItem(item.ProductId));
            }

            return buyerWishlist;
        }

        /// <inheritdoc/>
        public async Task<List<BuyerLinkage>> GetBuyerLinkages(int buyerId)
        {
            List<BuyerLinkageEntity> buyerLinkagesEntities = await this.dbContext.BuyerLinkages
                .Where(linkageEntity => linkageEntity.RequestingBuyerId == buyerId || linkageEntity.ReceivingBuyerId == buyerId).ToListAsync();
            List<BuyerLinkage> buyerLinkages = new List<BuyerLinkage>();
            foreach (BuyerLinkageEntity linkageEntity in buyerLinkagesEntities)
            {
                BuyerLinkage buyerLinkage = ReadBuyerLinkage(linkageEntity, buyerId);
                buyerLinkages.Add(buyerLinkage);
            }

            return buyerLinkages;
        }

        /// <inheritdoc/>
        public async Task CreateLinkageRequest(int requestingBuyerId, int receivingBuyerId)
        {
            BuyerLinkageEntity linkageEntity = new BuyerLinkageEntity
            {
                RequestingBuyerId = requestingBuyerId,
                ReceivingBuyerId = receivingBuyerId,
                IsApproved = false,
            };
            this.dbContext.BuyerLinkages.Add(linkageEntity);
            await this.dbContext.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task UpdateLinkageRequest(int requestingBuyerId, int receivingBuyerId)
        {
            BuyerLinkageEntity? linkageEntity = await this.dbContext.BuyerLinkages
                .FirstOrDefaultAsync(linkage => linkage.RequestingBuyerId == requestingBuyerId && linkage.ReceivingBuyerId == receivingBuyerId);

            if (linkageEntity == null)
            {
                return;
            }

            linkageEntity.IsApproved = true;
            await this.dbContext.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteLinkageRequest(int requestingBuyerId, int receivingBuyerId)
        {
            BuyerLinkageEntity? linkageEntity = await this.dbContext.BuyerLinkages
                .FirstOrDefaultAsync(linkage => linkage.RequestingBuyerId == requestingBuyerId && linkage.ReceivingBuyerId == receivingBuyerId);

            if (linkageEntity == null)
            {
                return false;
            }

            this.dbContext.BuyerLinkages.Remove(linkageEntity);
            await this.dbContext.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
        public async Task<List<Buyer>> FindBuyersWithShippingAddress(Address shippingAddress)
        {
            return await this.dbContext.Buyers
                .Where(buyer => buyer.ShippingAddress.Id == shippingAddress.Id)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task CreateBuyer(Buyer buyerEntity)
        {
            this.dbContext.Buyers.Add(buyerEntity);
            await this.dbContext.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<List<int>> GetFollowingUsersIds(int buyerId)
        {
            return await this.dbContext.Followings
                .Where(following => following.BuyerId == buyerId)
                .Select(following => following.SellerId)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<List<Seller>> GetFollowedSellers(List<int>? followingUsersIds)
        {
            if (followingUsersIds == null || followingUsersIds.Count == 0)
            {
                return new List<Seller>();
            }

            return await this.dbContext.Sellers
                .Where(seller => followingUsersIds.Contains(seller.Id))
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<List<Seller>> GetAllSellers()
        {
            return await this.dbContext.Sellers.ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<List<Product>> GetProductsFromSeller(int sellerId)
        {
            return await this.dbContext.Products.Where(product => product.SellerId == sellerId).ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> CheckIfBuyerExists(int buyerId)
        {
            return await this.dbContext.Buyers.AnyAsync(buyer => buyer.Id == buyerId);
        }

        /// <inheritdoc/>
        public async Task<bool> IsFollowing(int buyerId, int sellerId)
        {
            return await this.dbContext.Followings.AnyAsync(following => following.BuyerId == buyerId && following.SellerId == sellerId);
        }

        /// <inheritdoc/>
        public async Task FollowSeller(int buyerId, int sellerId)
        {
            this.dbContext.Followings.Add(new FollowingEntity { BuyerId = buyerId, SellerId = sellerId });
            await this.dbContext.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task UnfollowSeller(int buyerId, int sellerId)
        {
            this.dbContext.Followings.Remove(new FollowingEntity { BuyerId = buyerId, SellerId = sellerId });
            await this.dbContext.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<int> GetTotalCount()
        {
            return await this.dbContext.Buyers.CountAsync();
        }

        /// <inheritdoc/>
        public async Task UpdateAfterPurchase(Buyer buyerEntity)
        {
            this.dbContext.Buyers.Update(buyerEntity);
            await this.dbContext.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task RemoveWishilistItem(int buyerId, int productId)
        {
            this.dbContext.BuyersWishlistItems.Remove(new BuyerWishlistItemsEntity { BuyerId = buyerId, ProductId = productId });
            await this.dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Loads address information from the database.
        /// </summary>
        /// <param name="addressId">The ID of the address to load.</param>
        /// <returns>A task containing the loaded address or null if not found.</returns>
        public async Task<Address?> LoadAddress(int addressId)
        {
            return await this.dbContext.Addresses.FindAsync(addressId);
        }

        /// <summary>
        /// Reads buyer linkage information from a SQL data reader.
        /// </summary>
        /// <param name="linkageEntity">The linkage entity containing linkage information.</param>
        /// <param name="buyerId">The ID of the current user.</param>
        /// <returns>A BuyerLinkage object containing the read information.</returns>
        private static BuyerLinkage ReadBuyerLinkage(BuyerLinkageEntity linkageEntity, int buyerId)
        {
            int requestingBuyerId = linkageEntity.RequestingBuyerId;
            int receivingBuyerId = linkageEntity.ReceivingBuyerId;
            bool isApproved = linkageEntity.IsApproved;
            int linkedBuyerId = requestingBuyerId;
            BuyerLinkageStatus buyerLinkageStatus = BuyerLinkageStatus.Confirmed;

            if (requestingBuyerId == buyerId)
            {
                linkedBuyerId = receivingBuyerId;
                if (!isApproved)
                {
                    buyerLinkageStatus = BuyerLinkageStatus.PendingSelf;
                }
            }
            else if (receivingBuyerId == buyerId)
            {
                linkedBuyerId = requestingBuyerId;
                if (!isApproved)
                {
                    buyerLinkageStatus = BuyerLinkageStatus.PendingOther;
                }
            }

            return new BuyerLinkage
            {
                Buyer = new Buyer
                {
                    User = new User { UserId = linkedBuyerId },
                },
                Status = buyerLinkageStatus,
            };
        }

        /// <inheritdoc/>
        public async Task<List<Address>> GetAllAddressesAsync()
        {
            return await this.dbContext.Addresses.ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<Address> GetAddressByIdAsync(int id)
        {
            var address = await this.dbContext.Addresses.FindAsync(id);
            if (address == null)
            {
                throw new KeyNotFoundException($"Address with ID {id} not found.");
            }
            return address;
        }

        /// <inheritdoc/>
        public async Task AddAddressAsync(Address address)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            await this.dbContext.Addresses.AddAsync(address);
            await this.dbContext.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task UpdateAddressAsync(Address address)
        {
            var existingAddress = await this.dbContext.Addresses.FindAsync(address.Id);
            if (existingAddress == null)
            {
                throw new KeyNotFoundException($"Address with ID {address.Id} not found.");
            }

            existingAddress.StreetLine = address.StreetLine;
            existingAddress.City = address.City;
            existingAddress.Country = address.Country;
            existingAddress.PostalCode = address.PostalCode;

            this.dbContext.Addresses.Update(existingAddress);
            await this.dbContext.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task DeleteAddressAsync(Address address)
        {
            var existingAddress = await this.dbContext.Addresses.FindAsync(address.Id);
            if (existingAddress == null)
            {
                throw new KeyNotFoundException($"Address with ID {address.Id} not found.");
            }

            this.dbContext.Addresses.Remove(existingAddress);
            await this.dbContext.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> AddressExistsAsync(int id)
        {
            return await this.dbContext.Addresses.AnyAsync(a => a.Id == id);
        }
    }
}