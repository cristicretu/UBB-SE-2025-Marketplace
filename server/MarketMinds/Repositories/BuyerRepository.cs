// <copyright file="BuyerRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using global::MarketMinds.Shared.IRepository;
    using global::MarketMinds.Shared.Models;
    using Microsoft.EntityFrameworkCore;
    using Server.DataAccessLayer;
    using Server.DataModels;

    /// <summary>
    /// Repository class for managing buyer-related database operations.
    /// </summary>
    /// <param name="dbContext">The database context instance.</param>
    public class BuyerRepository : IBuyerRepository
    {
        private readonly ApplicationDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuyerRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context instance.</param>
        public BuyerRepository(ApplicationDbContext dbContext)
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
            Console.WriteLine("=== REPOSITORY: SaveInfo called ===");
            Console.WriteLine($"REPOSITORY: Buyer ID: {buyerEntity.Id}");
            Console.WriteLine($"REPOSITORY: FirstName: '{buyerEntity.FirstName}', LastName: '{buyerEntity.LastName}'");
            Console.WriteLine($"REPOSITORY: PhoneNumber: '{buyerEntity.User?.PhoneNumber}', UseSameAddress: {buyerEntity.UseSameAddress}");
            
            if (buyerEntity.BillingAddress != null)
            {
                Console.WriteLine($"REPOSITORY: Input Billing Address - Street: '{buyerEntity.BillingAddress.StreetLine}', City: '{buyerEntity.BillingAddress.City}', Country: '{buyerEntity.BillingAddress.Country}', PostalCode: '{buyerEntity.BillingAddress.PostalCode}', Id: {buyerEntity.BillingAddress.Id}");
            }
            if (buyerEntity.ShippingAddress != null)
            {
                Console.WriteLine($"REPOSITORY: Input Shipping Address - Street: '{buyerEntity.ShippingAddress.StreetLine}', City: '{buyerEntity.ShippingAddress.City}', Country: '{buyerEntity.ShippingAddress.Country}', PostalCode: '{buyerEntity.ShippingAddress.PostalCode}', Id: {buyerEntity.ShippingAddress.Id}");
            }

            var buyerId = buyerEntity.Id;

            Console.WriteLine("REPOSITORY: Checking if buyer exists...");
            var buyerExists = await dbContext.Buyers.AnyAsync(b => b.Id == buyerId);
            
            if (!buyerExists)
            {
                Console.WriteLine("REPOSITORY: Buyer does not exist, creating new buyer");
                dbContext.Buyers.Add(buyerEntity);
            }
            else
            {
                Console.WriteLine("REPOSITORY: Buyer exists, proceeding with save");
                
                Console.WriteLine("REPOSITORY: Starting address handling...");
                
                // Handle addresses properly based on UseSameAddress flag
                if (buyerEntity.UseSameAddress)
                {
                    Console.WriteLine("REPOSITORY: UseSameAddress is true, making addresses the same");
                    
                    // When using same address, both should point to the same address record
                    if (buyerEntity.BillingAddress != null)
                    {
                        var billingAddress = buyerEntity.BillingAddress;
                        
                        // Update or create the billing address
                        var trackedBilling = dbContext.ChangeTracker.Entries<Address>()
                            .FirstOrDefault(e => e.Entity.Id == billingAddress.Id)?.Entity;
                            
                        if (trackedBilling != null)
                        {
                            Console.WriteLine($"REPOSITORY: Found tracked billing address, updating values");
                            trackedBilling.StreetLine = billingAddress.StreetLine;
                            trackedBilling.City = billingAddress.City;
                            trackedBilling.Country = billingAddress.Country;
                            trackedBilling.PostalCode = billingAddress.PostalCode;
                            buyerEntity.BillingAddress = trackedBilling;
                        }
                        else
                        {
                            Console.WriteLine($"REPOSITORY: No tracked billing address found, calling dbContext.Update()");
                            dbContext.Update(billingAddress);
                            Console.WriteLine($"REPOSITORY: Called dbContext.Update() for billing address with ID: {billingAddress.Id}");
                        }
                        
                        // Make shipping address point to the same record
                        buyerEntity.ShippingAddress = buyerEntity.BillingAddress;
                        Console.WriteLine($"REPOSITORY: Set shipping address to same as billing address (ID: {buyerEntity.BillingAddress.Id})");
                    }
                }
                else
                {
                    Console.WriteLine("REPOSITORY: UseSameAddress is false, handling separate addresses");
                    
                    // Handle billing address
                    if (buyerEntity.BillingAddress != null)
                    {
                        Console.WriteLine("REPOSITORY: Handling billing address...");
                        var billingAddress = buyerEntity.BillingAddress;
                        
                        var trackedBilling = dbContext.ChangeTracker.Entries<Address>()
                            .FirstOrDefault(e => e.Entity.Id == billingAddress.Id)?.Entity;
                            
                        if (trackedBilling != null)
                        {
                            Console.WriteLine($"REPOSITORY: Found tracked billing address, updating values");
                            Console.WriteLine($"REPOSITORY: Tracked billing address before update - Street: '{trackedBilling.StreetLine}', City: '{trackedBilling.City}', Country: '{trackedBilling.Country}', PostalCode: '{trackedBilling.PostalCode}'");
                            trackedBilling.StreetLine = billingAddress.StreetLine;
                            trackedBilling.City = billingAddress.City;
                            trackedBilling.Country = billingAddress.Country;
                            trackedBilling.PostalCode = billingAddress.PostalCode;
                            Console.WriteLine($"REPOSITORY: Tracked billing address after update - Street: '{trackedBilling.StreetLine}', City: '{trackedBilling.City}', Country: '{trackedBilling.Country}', PostalCode: '{trackedBilling.PostalCode}'");
                            buyerEntity.BillingAddress = trackedBilling;
                        }
                        else
                        {
                            Console.WriteLine($"REPOSITORY: No tracked billing address found, calling dbContext.Update()");
                            dbContext.Update(billingAddress);
                            Console.WriteLine($"REPOSITORY: Called dbContext.Update() for billing address with ID: {billingAddress.Id}");
                        }
                    }
                    
                    // Handle shipping address - CREATE NEW if it's different from billing
                    if (buyerEntity.ShippingAddress != null)
                    {
                        Console.WriteLine("REPOSITORY: Handling shipping address...");
                        var shippingAddress = buyerEntity.ShippingAddress;
                        
                        // Check if shipping and billing addresses are actually different
                        bool addressesAreDifferent = buyerEntity.BillingAddress == null ||
                            shippingAddress.StreetLine != buyerEntity.BillingAddress.StreetLine ||
                            shippingAddress.City != buyerEntity.BillingAddress.City ||
                            shippingAddress.Country != buyerEntity.BillingAddress.Country ||
                            shippingAddress.PostalCode != buyerEntity.BillingAddress.PostalCode;
                            
                        if (addressesAreDifferent)
                        {
                            Console.WriteLine("REPOSITORY: Shipping address is different from billing, creating new address record");
                            // Create a completely new address for shipping
                            var newShippingAddress = new Address
                            {
                                StreetLine = shippingAddress.StreetLine,
                                City = shippingAddress.City,
                                Country = shippingAddress.Country,
                                PostalCode = shippingAddress.PostalCode
                            };
                            
                            dbContext.Addresses.Add(newShippingAddress);
                            buyerEntity.ShippingAddress = newShippingAddress;
                            Console.WriteLine($"REPOSITORY: Created new shipping address - Street: '{newShippingAddress.StreetLine}', City: '{newShippingAddress.City}', Country: '{newShippingAddress.Country}', PostalCode: '{newShippingAddress.PostalCode}'");
                        }
                        else
                        {
                            Console.WriteLine("REPOSITORY: Shipping address is same as billing, reusing billing address");
                            buyerEntity.ShippingAddress = buyerEntity.BillingAddress;
                        }
                    }
                }

                Console.WriteLine("REPOSITORY: Updating buyer entity...");
                dbContext.Buyers.Update(buyerEntity);
                Console.WriteLine("REPOSITORY: Called dbContext.Buyers.Update()");
            }

            Console.WriteLine("REPOSITORY: Final state before SaveChangesAsync:");
            Console.WriteLine($"REPOSITORY: Final Buyer - FirstName: '{buyerEntity.FirstName}', LastName: '{buyerEntity.LastName}', UseSameAddress: {buyerEntity.UseSameAddress}");
            Console.WriteLine($"REPOSITORY: Final Billing Address - Street: '{buyerEntity.BillingAddress?.StreetLine}', City: '{buyerEntity.BillingAddress?.City}', Country: '{buyerEntity.BillingAddress?.Country}', PostalCode: '{buyerEntity.BillingAddress?.PostalCode}', Id: {buyerEntity.BillingAddress?.Id}");
            Console.WriteLine($"REPOSITORY: Final Shipping Address - Street: '{buyerEntity.ShippingAddress?.StreetLine}', City: '{buyerEntity.ShippingAddress?.City}', Country: '{buyerEntity.ShippingAddress?.Country}', PostalCode: '{buyerEntity.ShippingAddress?.PostalCode}', Id: {buyerEntity.ShippingAddress?.Id}");

            Console.WriteLine("REPOSITORY: Calling SaveChangesAsync...");
            await dbContext.SaveChangesAsync();
            Console.WriteLine("REPOSITORY: SaveChangesAsync completed successfully");
            Console.WriteLine("=== REPOSITORY: SaveInfo completed successfully ===");
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
            var buyProducts = await this.dbContext.BuyProducts.Where(product => product.SellerId == sellerId).ToListAsync();
            return buyProducts.Cast<Product>().ToList();
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
            BuyerWishlistItemsEntity? wishlistItem = await this.dbContext.BuyersWishlistItems
                .FirstOrDefaultAsync(item => item.BuyerId == buyerId && item.ProductId == productId);
            if (wishlistItem != null)
            {
                this.dbContext.BuyersWishlistItems.Remove(wishlistItem);
                await this.dbContext.SaveChangesAsync();
            }
        }

        /// <inheritdoc/>
        public async Task AddItemToWishlist(int buyerId, int productId)
        {
            // Check if the item already exists to avoid duplicates
            bool itemExists = await this.dbContext.BuyersWishlistItems
                                      .AnyAsync(item => item.BuyerId == buyerId && item.ProductId == productId);

            if (!itemExists)
            {
                BuyerWishlistItemsEntity wishlistItem = new BuyerWishlistItemsEntity
                {
                    BuyerId = buyerId,
                    ProductId = productId,
                };
                this.dbContext.BuyersWishlistItems.Add(wishlistItem);
                await this.dbContext.SaveChangesAsync();
            }
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
                    User = new User { Id = linkedBuyerId },
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