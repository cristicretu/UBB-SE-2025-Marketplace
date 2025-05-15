// <copyright file="IBuyerService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SharedClassLibrary.Service
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using SharedClassLibrary.Domain;

    /// <summary>
    /// Interface for managing buyer-related business operations.
    /// </summary>
    public interface IBuyerService
    {
        /// <summary>
        /// Gets a buyer associated with a specific user.
        /// </summary>
        /// <param name="user">The user to get the buyer for.</param>
        /// <returns>A task containing the buyer associated with the user.</returns>
        Task<Buyer> GetBuyerByUser(User user);

        /// <summary>
        /// Creates a new buyer in the system.
        /// </summary>
        /// <param name="buyer">The buyer to create.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task CreateBuyer(Buyer buyer);

        /// <summary>
        /// Saves buyer information to the system.
        /// </summary>
        /// <param name="buyer">The buyer information to save.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SaveInfo(Buyer buyer);

        /// <summary>
        /// Finds buyers with a matching shipping address.
        /// </summary>
        /// <param name="currentBuyerShippingAddress">The shipping address to search for.</param>
        /// <returns>A task containing a list of buyers with matching shipping address.</returns>
        Task<List<Buyer>> FindBuyersWithShippingAddress(Address currentBuyerShippingAddress);

        /// <summary>
        /// Loads specific segments of buyer data.
        /// </summary>
        /// <param name="buyer">The buyer to load data for.</param>
        /// <param name="buyerDataSegments">The segments of data to load.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task LoadBuyer(Buyer buyer, BuyerDataSegments buyerDataSegments);

        /// <summary>
        /// Creates a linkage request between two buyers.
        /// </summary>
        /// <param name="userBuyer">The buyer initiating the request.</param>
        /// <param name="linkedBuyer">The buyer to link with.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task CreateLinkageRequest(Buyer userBuyer, Buyer linkedBuyer);

        /// <summary>
        /// Breaks an existing linkage between two buyers.
        /// </summary>
        /// <param name="userBuyer">The first buyer in the linkage.</param>
        /// <param name="linkedBuyer">The second buyer in the linkage.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task BreakLinkage(Buyer userBuyer, Buyer linkedBuyer);

        /// <summary>
        /// Cancels a pending linkage request.
        /// </summary>
        /// <param name="userBuyer">The buyer who initiated the request.</param>
        /// <param name="linkedBuyer">The buyer who received the request.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task CancelLinkageRequest(Buyer userBuyer, Buyer linkedBuyer);

        /// <summary>
        /// Accepts a pending linkage request.
        /// </summary>
        /// <param name="userBuyer">The buyer accepting the request.</param>
        /// <param name="linkedBuyer">The buyer who sent the request.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AcceptLinkageRequest(Buyer userBuyer, Buyer linkedBuyer);

        /// <summary>
        /// Refuses a pending linkage request.
        /// </summary>
        /// <param name="userBuyer">The buyer refusing the request.</param>
        /// <param name="linkedBuyer">The buyer who sent the request.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RefuseLinkageRequest(Buyer userBuyer, Buyer linkedBuyer);

        /// <summary>
        /// Gets the IDs of users that a buyer is following.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>A task containing a list of followed user IDs.</returns>
        Task<List<int>> GetFollowingUsersIDs(int buyerId);

        /// <summary>
        /// Gets products from sellers that a buyer follows.
        /// </summary>
        /// <param name="followedSellersIDs">The list of followed seller IDs.</param>
        /// <returns>A task containing a list of products from followed sellers.</returns>
        Task<List<Product>> GetProductsFromFollowedSellers(List<int> followedSellersIDs);

        /// <summary>
        /// Gets the list of sellers followed by a buyer.
        /// </summary>
        /// <param name="followingUserIds">The list of user IDs being followed.</param>
        /// <returns>A task containing a list of followed sellers.</returns>
        Task<List<Seller>> GetFollowedSellers(List<int> followingUserIds);

        /// <summary>
        /// Gets all sellers in the system.
        /// </summary>
        /// <returns>A task containing a list of all sellers.</returns>
        Task<List<Seller>> GetAllSellers();

        /// <summary>
        /// Updates buyer information after a purchase.
        /// </summary>
        /// <param name="buyer">The buyer to update.</param>
        /// <param name="purchaseAmount">The amount of the purchase.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateAfterPurchase(Buyer buyer, decimal purchaseAmount);

        /// <summary>
        /// Removes an item from a buyer's wishlist.
        /// </summary>
        /// <param name="buyer">The buyer whose wishlist to modify.</param>
        /// <param name="productId">The ID of the product to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RemoveWishilistItem(Buyer buyer, int productId);

        /// <summary>
        /// Gets products for a seller's profile view.
        /// </summary>
        /// <param name="sellerId">The ID of the seller.</param>
        /// <returns>A task containing a list of the seller's products.</returns>
        Task<List<Product>> GetProductsForViewProfile(int sellerId);

        /// <summary>
        /// Checks if a buyer exists in the system.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer to check.</param>
        /// <returns>A task containing a boolean indicating whether the buyer exists.</returns>
        Task<bool> CheckIfBuyerExists(int buyerId);

        /// <summary>
        /// Checks if a buyer is following a seller.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <param name="sellerId">The ID of the seller.</param>
        /// <returns>A task containing a boolean indicating whether the buyer is following the seller.</returns>
        Task<bool> IsFollowing(int buyerId, int sellerId);

        /// <summary>
        /// Makes a buyer follow a seller.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <param name="sellerId">The ID of the seller to follow.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task FollowSeller(int buyerId, int sellerId);

        /// <summary>
        /// Makes a buyer unfollow a seller.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <param name="sellerId">The ID of the seller to unfollow.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UnfollowSeller(int buyerId, int sellerId);

        /// <summary>
        /// Gets the calculated progress towards the next badge level for a buyer.
        /// </summary>
        /// <param name="buyer">The buyer for whom to calculate the progress.</param>
        /// <returns>An integer representing the badge progress percentage (1-100).</returns>
        int GetBadgeProgress(Buyer buyer);

        /// <summary>
        /// Gets the wishlist items with product details for a specific buyer.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>A task containing a list of wishlist items with product details.</returns>
        

    }
}
