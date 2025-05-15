// <copyright file="IBuyerRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SharedClassLibrary.IRepository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using SharedClassLibrary.Domain;

    /// <summary>
    /// Interface for managing buyer-related database operations.
    /// </summary>
    public interface IBuyerRepository
    {
        /// <summary>
        /// Loads buyer information from the database.
        /// </summary>
        /// <param name="buyerEntity">The buyer object to load information into.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when the buyer or user or shipping/billingaddress is not found.</exception>
        Task LoadBuyerInfo(Buyer buyerEntity);

        /// <summary>
        /// Saves buyer information to the database.
        /// </summary>
        /// <param name="buyerEntity">The buyer object containing information to save.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when the buyer is not found.</exception>
        Task SaveInfo(Buyer buyerEntity);

        /// <summary>
        /// Retrieves the wishlist for a specific buyer.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>A task containing the buyer's wishlist.</returns>
        Task<BuyerWishlist> GetWishlist(int buyerId);

        /// <summary>
        /// Gets all linkages for a specific buyer.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>A task containing a list of buyer linkages.</returns>
        Task<List<BuyerLinkage>> GetBuyerLinkages(int buyerId);

        /// <summary>
        /// Creates a new linkage request between two buyers.
        /// </summary>
        /// <param name="requestingBuyerId">The ID of the buyer making the request.</param>
        /// <param name="receivingBuyerId">The ID of the buyer receiving the request.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task CreateLinkageRequest(int requestingBuyerId, int receivingBuyerId);

        /// <summary>
        /// Updates an existing linkage request between two buyers.
        /// </summary>
        /// <param name="requestingBuyerId">The ID of the buyer who made the request.</param>
        /// <param name="receivingBuyerId">The ID of the buyer who received the request.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateLinkageRequest(int requestingBuyerId, int receivingBuyerId);

        /// <summary>
        /// Deletes a linkage request between two buyers.
        /// </summary>
        /// <param name="requestingBuyerId">The ID of the buyer who made the request.</param>
        /// <param name="receivingBuyerId">The ID of the buyer who received the request.</param>
        /// <returns>A task containing a boolean indicating whether the deletion was successful.</returns>
        Task<bool> DeleteLinkageRequest(int requestingBuyerId, int receivingBuyerId);

        /// <summary>
        /// Finds buyers with a specific shipping address.
        /// </summary>
        /// <param name="shippingAddress">The shipping address to search for.</param>
        /// <returns>A task containing a list of buyers matching the shipping address.</returns>
        Task<List<Buyer>> FindBuyersWithShippingAddress(Address shippingAddress);

        /// <summary>
        /// Creates a new buyer in the database.
        /// </summary>
        /// <param name="buyerEntity">The buyer object to create.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task CreateBuyer(Buyer buyerEntity);

        /// <summary>
        /// Gets the IDs of users that a buyer is following.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <returns>A task containing a list of followed user IDs.</returns>
        Task<List<int>> GetFollowingUsersIds(int buyerId);

        /// <summary>
        /// Gets the list of sellers followed by a buyer.
        /// </summary>
        /// <param name="followingUsersIds">The list of user IDs being followed.</param>
        /// <returns>A task containing a list of followed sellers.</returns>
        Task<List<Seller>> GetFollowedSellers(List<int>? followingUsersIds);

        /// <summary>
        /// Gets all sellers in the system.
        /// </summary>
        /// <returns>A task containing a list of all sellers.</returns>
        Task<List<Seller>> GetAllSellers();

        /// <summary>
        /// Gets all products from a specific seller.
        /// </summary>
        /// <param name="sellerId">The ID of the seller.</param>
        /// <returns>A task containing a list of products.</returns>
        Task<List<Product>> GetProductsFromSeller(int sellerId);

        /// <summary>
        /// Checks if a buyer exists in the database.
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
        /// Gets the total count of buyers in the system.
        /// </summary>
        /// <returns>A task containing the total number of buyers.</returns>
        Task<int> GetTotalCount();

        /// <summary>
        /// Updates buyer information after a purchase.
        /// </summary>
        /// <param name="buyerEntity">The buyer object to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateAfterPurchase(Buyer buyerEntity);

        /// <summary>
        /// Removes an item from a buyer's wishlist.
        /// </summary>
        /// <param name="buyerId">The ID of the buyer.</param>
        /// <param name="productId">The ID of the product to remove.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RemoveWishilistItem(int buyerId, int productId);


        /// <summary>
        /// Retrieves all addresses from the database.
        /// </summary>
        /// <returns>A task containing a list of all addresses.</returns>
        Task<List<Address>> GetAllAddressesAsync();

        /// <summary>
        /// Retrieves an address by its ID.
        /// </summary>
        /// <param name="id">The ID of the address.</param>
        /// <returns>A task containing the address.</returns>
        Task<Address> GetAddressByIdAsync(int id);

        /// <summary>
        /// Adds a new address to the database.
        /// </summary>
        /// <param name="address">The address to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddAddressAsync(Address address);

        /// <summary>
        /// Updates an existing address in the database.
        /// </summary>
        /// <param name="address">The address to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateAddressAsync(Address address);

        /// <summary>
        /// Deletes an address from the database by its ID.
        /// </summary>
        /// <param name="address">The address to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteAddressAsync(Address address);

        /// <summary>
        /// Checks if an address exists in the database by its ID.
        /// </summary>
        /// <param name="id">The ID of the address.</param>
        /// <returns>A task containing a boolean indicating whether the address exists.</returns>
        Task<bool> AddressExistsAsync(int id);

    }
}
