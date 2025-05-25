// <copyright file="BuyerService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketMinds.Shared.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using MarketMinds.Shared.Helper;
    using MarketMinds.Shared.Models;
    using MarketMinds.Shared.IRepository;

    /// <summary>
    /// Represents the different segments of buyer data that can be loaded.
    /// </summary>
    [Flags]
    public enum BuyerDataSegments
    {
        /// <summary>
        /// Basic information about the buyer including name, addresses, and purchase history.
        /// </summary>
        BasicInfo = 1,

        /// <summary>
        /// User account information including email and phone number.
        /// </summary>
        User = 2,

        /// <summary>
        /// The buyer's wishlist of products.
        /// </summary>
        Wishlist = 4,

        /// <summary>
        /// The buyer's linkages with other buyers.
        /// </summary>
        Linkages = 8,
    }

    /// <summary>
    /// Service class for managing buyer-related business operations.
    /// </summary>
    /// <param name="buyerRepo">The buyer repository instance.</param>
    /// <param name="userRepo">The user repository instance.</param>
    public class BuyerService : IBuyerService
    {
        // Constants for Badge Progress Calculation (moved from BuyerConfiguration)
        private const decimal SpendingWeight = 0.8m;
        private const decimal PurchasesWeight = 0.2m;
        private const decimal SpendingBase = 1000.0m;
        private const decimal PurchasesBase = 100.0m;
        private const decimal MinimumBadgeProgress = 1.0m;
        private const decimal MaxBadgeProgress = 100.0m;

        private readonly IBuyerRepository buyerRepo;
        private readonly IAccountRepository userRepo;
        private IUserValidator userValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuyerService"/> class.
        /// </summary>
        /// <param name="buyerRepo">The repository used to access buyer data.</param>
        /// <param name="userRepo">The repository used to access user data.</param>
        public BuyerService(IBuyerRepository buyerRepo, IAccountRepository userRepo)
        {
            this.buyerRepo = buyerRepo;
            this.userRepo = userRepo;
            this.userValidator = new UserValidator();
        }

        /// <inheritdoc/>
        public async Task<Buyer> GetBuyerByUser(User user)
        {
            var buyer = new Buyer();
            User? dbUser = await this.userRepo.GetUserById(user.Id);
            buyer.User = dbUser; // this can be null but it will not be, very unlikely
            await this.LoadBuyer(buyer, BuyerDataSegments.BasicInfo | BuyerDataSegments.Wishlist | BuyerDataSegments.Linkages);
            return buyer;
        }

        /// <inheritdoc/>
        public async Task CreateBuyer(Buyer buyer)
        {
            this.ValidateBuyerInfo(buyer);
            await this.buyerRepo.CreateBuyer(buyer);
            await this.userRepo.UpdateUserPhoneNumber(buyer.User);
        }

        /// <inheritdoc/>
        public async Task SaveInfo(Buyer buyer)
        {
            this.ValidateBuyerInfo(buyer);
            await this.buyerRepo.SaveInfo(buyer);
            await this.userRepo.UpdateUserPhoneNumber(buyer.User);
        }

        /// <inheritdoc/>
        public async Task<List<Buyer>> FindBuyersWithShippingAddress(Address currentBuyerShippingAddress)
        {
            if (currentBuyerShippingAddress.Country == null)
            {
                return new List<Buyer>();
            }

            var buyersWithMatchingAddress = await this.buyerRepo.FindBuyersWithShippingAddress(currentBuyerShippingAddress);
            foreach (var buyer in buyersWithMatchingAddress)
            {
                await this.LoadBuyer(buyer, BuyerDataSegments.BasicInfo | BuyerDataSegments.User);
            }

            return buyersWithMatchingAddress;
        }

        /// <inheritdoc/>
        public async Task LoadBuyer(Buyer buyer, BuyerDataSegments buyerDataSegments)
        {
            if ((buyerDataSegments & BuyerDataSegments.BasicInfo) == BuyerDataSegments.BasicInfo)
            {
                await this.buyerRepo.LoadBuyerInfo(buyer);
            }

            if ((buyerDataSegments & BuyerDataSegments.User) == BuyerDataSegments.User)
            {
                await this.userRepo.LoadUserPhoneNumberAndEmailById(buyer.User);
            }

            if ((buyerDataSegments & BuyerDataSegments.Wishlist) == BuyerDataSegments.Wishlist)
            {
                buyer.Wishlist = await this.buyerRepo.GetWishlist(buyer.Id);
            }

            if ((buyerDataSegments & BuyerDataSegments.Linkages) == BuyerDataSegments.Linkages)
            {
                buyer.Linkages = await this.buyerRepo.GetBuyerLinkages(buyer.Id);
                var linkedBuyerList = new List<Buyer>();

                foreach (var linkage in buyer.Linkages)
                {
                    // Get the other buyer ID from the linkage
                    var otherBuyerId = linkage.GetOtherBuyerId(buyer.Id);
                    if (otherBuyerId.HasValue)
                    {
                        // Create a buyer with just the ID and load their info
                        var linkedBuyer = new Buyer { Id = otherBuyerId.Value };
                        await this.buyerRepo.LoadBuyerInfo(linkedBuyer);
                        linkedBuyerList.Add(linkedBuyer);
                    }
                    // Get the other buyer ID from the linkage
                    var otherBuyerId = linkage.GetOtherBuyerId(buyer.Id);
                    if (otherBuyerId.HasValue)
                    {
                        // Create a buyer with just the ID and load their info
                        var linkedBuyer = new Buyer { Id = otherBuyerId.Value };
                        await this.buyerRepo.LoadBuyerInfo(linkedBuyer);
                        linkedBuyerList.Add(linkedBuyer);
                    }
                }

                // For the main buyer, ensure we have basic info and user data
                if (!((buyerDataSegments & BuyerDataSegments.BasicInfo) == BuyerDataSegments.BasicInfo))
                {
                    await this.buyerRepo.LoadBuyerInfo(buyer);
                }

                if (!((buyerDataSegments & BuyerDataSegments.User) == BuyerDataSegments.User))
                {
                    await this.userRepo.LoadUserPhoneNumberAndEmailById(buyer.User);
                }

                // Load data for linked buyers
                foreach (var linkBuyer in linkedBuyerList)
                {
                    await this.LoadBuyer(
                        linkBuyer,
                        BuyerDataSegments.BasicInfo | BuyerDataSegments.User | BuyerDataSegments.Wishlist);
                }
            }
        }

        /// <inheritdoc/>
        public async Task CreateLinkageRequest(Buyer userBuyer, Buyer linkedBuyer)
        {
            await this.buyerRepo.CreateLinkageRequest(userBuyer.Id, linkedBuyer.Id);
        }

        /// <inheritdoc/>
        public async Task BreakLinkage(Buyer userBuyer, Buyer linkedBuyer)
        {
            _ = await this.buyerRepo.DeleteLinkageRequest(userBuyer.Id, linkedBuyer.Id) ||
                await this.buyerRepo.DeleteLinkageRequest(linkedBuyer.Id, userBuyer.Id);
        }

        /// <inheritdoc/>
        public async Task CancelLinkageRequest(Buyer userBuyer, Buyer linkedBuyer)
        {
            await this.buyerRepo.DeleteLinkageRequest(userBuyer.Id, linkedBuyer.Id);
        }

        /// <inheritdoc/>
        public async Task AcceptLinkageRequest(Buyer userBuyer, Buyer linkedBuyer)
        {
            await this.buyerRepo.UpdateLinkageRequest(linkedBuyer.Id, userBuyer.Id);
        }

        /// <inheritdoc/>
        public async Task RefuseLinkageRequest(Buyer userBuyer, Buyer linkedBuyer)
        {
            await this.buyerRepo.DeleteLinkageRequest(linkedBuyer.Id, userBuyer.Id);
        }

        /// <inheritdoc/>
        public async Task<List<int>> GetFollowingUsersIDs(int buyerId)
        {
            return await this.buyerRepo.GetFollowingUsersIds(buyerId);
        }

        /// <inheritdoc/>
        public async Task UpdateAfterPurchase(Buyer buyer, decimal purchaseAmount)
        {
            // Update buyer stats
            buyer.TotalSpending += purchaseAmount;
            buyer.NumberOfPurchases++;

            // Update badge based on total spending
            if (buyer.TotalSpending >= BuyerConfiguration.PlatinumThreshold)
            {
                buyer.Badge = BuyerBadge.PLATINUM;
            }
            else if (buyer.TotalSpending >= BuyerConfiguration.GoldThreshold)
            {
                buyer.Badge = BuyerBadge.GOLD;
            }
            else if (buyer.TotalSpending >= BuyerConfiguration.SilverThreshold)
            {
                buyer.Badge = BuyerBadge.SILVER;
            }
            else
            {
                buyer.Badge = BuyerBadge.BRONZE;
            }

            // Update discount based on badge
            buyer.Discount = buyer.Badge switch
            {
                BuyerBadge.PLATINUM => BuyerConfiguration.PlatinumDiscount,
                BuyerBadge.GOLD => BuyerConfiguration.GoldDiscount,
                BuyerBadge.SILVER => BuyerConfiguration.SilverDiscount,
                _ => BuyerConfiguration.BronzeDiscount
            };

            // Persist changes
            await this.buyerRepo.UpdateAfterPurchase(buyer);
        }

        /// <inheritdoc/>
        public async Task<List<Product>> GetProductsFromFollowedSellers(List<int> followedSellersIDs)
        {
            List<Product> followedSellersProducts = new List<Product>();
            foreach (var sellerId in followedSellersIDs)
            {
                var sellerProducts = await this.buyerRepo.GetProductsFromSeller(sellerId);
                followedSellersProducts.AddRange(sellerProducts); // Aggregate products from all followed sellers
            }

            return followedSellersProducts;
        }

        /// <inheritdoc/>
        public async Task<List<Seller>> GetFollowedSellers(List<int> followingUserIds)
        {
            return await this.buyerRepo.GetFollowedSellers(followingUserIds);
        }

        /// <inheritdoc/>
        public async Task<List<Seller>> GetAllSellers()
        {
            return await this.buyerRepo.GetAllSellers();
        }

        /// <inheritdoc/>
        public async Task RemoveWishilistItem(Buyer buyer, int productId)
        {
            if (buyer == null)
            {
                throw new ArgumentNullException(nameof(buyer));
            }

            if (buyer.Id <= 0)
            {
                throw new ArgumentException("Buyer ID must be greater than zero.", nameof(buyer.Id));
            }

            if (productId <= 0)
            {
                throw new ArgumentException("Product ID must be greater than zero.", nameof(productId));
            }

            await this.buyerRepo.RemoveWishilistItem(buyer.Id, productId);
        }

        /// <inheritdoc/>
        public async Task AddWishlistItem(Buyer buyer, int productId)
        {
            if (buyer == null)
            {
                throw new ArgumentNullException(nameof(buyer));
            }

            if (buyer.Id <= 0)
            {
                throw new ArgumentException("Buyer ID must be greater than zero.", nameof(buyer.Id));
            }

            if (productId <= 0)
            {
                throw new ArgumentException("Product ID must be greater than zero.", nameof(productId));
            }

            await this.buyerRepo.AddItemToWishlist(buyer.Id, productId);
        }

        /// <inheritdoc/>
        public async Task<List<Product>> GetProductsForViewProfile(int sellerId)
        {
            List<Product> viewProfileProducts = new List<Product>();
            var sellerProducts = await this.buyerRepo.GetProductsFromSeller(sellerId);
            viewProfileProducts.AddRange(sellerProducts);
            return viewProfileProducts;
        }

        /// <inheritdoc/>
        public async Task<bool> CheckIfBuyerExists(int buyerId)
        {
            return await this.buyerRepo.CheckIfBuyerExists(buyerId);
        }

        /// <inheritdoc/>
        public async Task<bool> IsFollowing(int buyerId, int sellerId)
        {
            return await this.buyerRepo.IsFollowing(buyerId, sellerId);
        }

        /// <inheritdoc/>
        public async Task FollowSeller(int buyerId, int sellerId)
        {
            await this.buyerRepo.FollowSeller(buyerId, sellerId);
        }

        /// <inheritdoc/>
        public async Task UnfollowSeller(int buyerId, int sellerId)
        {
            await this.buyerRepo.UnfollowSeller(buyerId, sellerId);
        }

        /// <inheritdoc/>
        public async Task<Buyer?> GetBuyerByIdAsync(int buyerId)
        {
            try
            {
                if (buyerId <= 0)
                {
                    return null;
                }

                // Check if buyer exists first
                var exists = await this.buyerRepo.CheckIfBuyerExists(buyerId);
                if (!exists)
                {
                    return null;
                }

                // Create a new buyer with the ID and load the info including User data
                var buyer = new Buyer { Id = buyerId };
                
                // First get the user associated with this buyer
                buyer.User = await this.userRepo.GetUserById(buyerId);
                
                // Load buyer info and user data
                await this.LoadBuyer(buyer, BuyerDataSegments.BasicInfo | BuyerDataSegments.User);
                
                return buyer;
            }
            catch (Exception)
            {
                // If any error occurs, return null
                return null;
            }
        }

        /// <inheritdoc/>
        public int GetBadgeProgress(Buyer buyer)
        {
            if (buyer == null)
            {
                return (int)MinimumBadgeProgress;
            }

            // Calculate spending progress (weighted)
            decimal spendingProgress = Math.Min(buyer.TotalSpending / SpendingBase, 1.0m) * SpendingWeight;

            // Calculate purchase count progress (weighted)
            decimal purchaseProgress = Math.Min(buyer.NumberOfPurchases / PurchasesBase, 1.0m) * PurchasesWeight;
            if (buyer == null)
            {
                return (int)MinimumBadgeProgress;
            }

            // Calculate spending progress (weighted)
            decimal spendingProgress = Math.Min(buyer.TotalSpending / SpendingBase, 1.0m) * SpendingWeight;

            // Calculate purchase count progress (weighted)
            decimal purchaseProgress = Math.Min(buyer.NumberOfPurchases / PurchasesBase, 1.0m) * PurchasesWeight;

            // Combine both factors
            decimal totalProgress = (spendingProgress + purchaseProgress) * 100;

            // Clamp between minimum and maximum
            return (int)Math.Max(MinimumBadgeProgress, Math.Min(MaxBadgeProgress, totalProgress));
        }

        /// <inheritdoc/>
        public async Task<List<BuyerWishlistItem>> GetWishlistItems(int buyerId)
        {
            try
            {
                var wishlist = await this.buyerRepo.GetWishlist(buyerId);
                return wishlist?.Items ?? new List<BuyerWishlistItem>();
            }
            catch (Exception)
            {
                // Return empty list if any error occurs
                return new List<BuyerWishlistItem>();
            }
            // Combine both factors
            decimal totalProgress = (spendingProgress + purchaseProgress) * 100;

            // Clamp between minimum and maximum
            return (int)Math.Max(MinimumBadgeProgress, Math.Min(MaxBadgeProgress, totalProgress));
        }

        /// <inheritdoc/>
        public async Task<List<BuyerWishlistItem>> GetWishlistItems(int buyerId)
        {
            try
            {
                var wishlist = await this.buyerRepo.GetWishlist(buyerId);
                return wishlist?.Items ?? new List<BuyerWishlistItem>();
            }
            catch (Exception)
            {
                // Return empty list if any error occurs
                return new List<BuyerWishlistItem>();
            }
        }

        /// <summary>
        /// Validates the buyer's information.
        /// </summary>
        /// <param name="buyer">The buyer to validate.</param>
        /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
        private void ValidateBuyerInfo(Buyer buyer)
        {
            // Basic validations
            if (string.IsNullOrWhiteSpace(buyer.FirstName))
            {
                throw new ArgumentException("First name cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(buyer.LastName))
            {
                throw new ArgumentException("Last name cannot be empty.");
            }

            if (!this.userValidator.IsValidPhoneNumber(buyer.PhoneNumber))
            {
                throw new ArgumentException("Invalid Phone Number");
            }

            this.ValidateAddress(buyer.BillingAddress);
            if (!buyer.UseSameAddress)
            {
                this.ValidateAddress(buyer.ShippingAddress);
            }
        }

        /// <summary>
        /// Validates an address.
        /// </summary>
        /// <param name="address">The address to validate.</param>
        /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
        private void ValidateAddress(Address address)
        {
            if (address == null)
            {
                throw new ArgumentException("Address cannot be null");
            }

            this.ValidateMandatoryField("Street Name", address.StreetLine);
            this.ValidateMandatoryField("Postal Code", address.PostalCode);
            this.ValidateMandatoryField("City", address.City);
            this.ValidateMandatoryField("Country", address.Country);
        }

        /// <summary>
        /// Validates a mandatory field.
        /// </summary>
        /// <param name="fieldName">The name of the field being validated.</param>
        /// <param name="fieldValue">The value to validate.</param>
        /// <exception cref="ArgumentException">Thrown when the field is empty or whitespace.</exception>
        private void ValidateMandatoryField(string fieldName, string? fieldValue)
        {
            if (string.IsNullOrWhiteSpace(fieldValue))
            {
                throw new ArgumentException(fieldName + " is required");
            }
        }
    }
}