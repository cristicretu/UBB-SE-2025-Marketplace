// <copyright file="BuyerSellerFollow.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketMinds.Shared.Models
{
    /// <summary>
    /// Represents a follow relationship between a buyer and seller
    /// </summary>
    public class BuyerSellerFollow
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BuyerId { get; set; }

        [Required]
        public int SellerId { get; set; }

        [Required]
        public DateTime FollowedDate { get; set; }

        // Navigation properties
        [ForeignKey("BuyerId")]
        public virtual Buyer? Buyer { get; set; }

        [ForeignKey("SellerId")]
        public virtual Seller? Seller { get; set; }

        /// <summary>
        /// Constructor for BuyerSellerFollow
        /// </summary>
        public BuyerSellerFollow()
        {
            FollowedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Constructor for BuyerSellerFollow with buyer and seller IDs
        /// </summary>
        /// <param name="buyerId">Buyer ID</param>
        /// <param name="sellerId">Seller ID</param>
        public BuyerSellerFollow(int buyerId, int sellerId)
        {
            BuyerId = buyerId;
            SellerId = sellerId;
            FollowedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Checks if this follow relationship involves the specified buyer
        /// </summary>
        /// <param name="buyerId">Buyer ID to check</param>
        /// <returns>True if the buyer is part of this follow relationship</returns>
        public bool InvolvesBuyer(int buyerId)
        {
            return BuyerId == buyerId;
        }

        /// <summary>
        /// Checks if this follow relationship involves the specified seller
        /// </summary>
        /// <param name="sellerId">Seller ID to check</param>
        /// <returns>True if the seller is part of this follow relationship</returns>
        public bool InvolvesSeller(int sellerId)
        {
            return SellerId == sellerId;
        }
    }
}