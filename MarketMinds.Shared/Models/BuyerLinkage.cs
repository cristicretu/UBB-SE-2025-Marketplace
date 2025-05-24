// <copyright file="BuyerLinkage.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketMinds.Shared.Models
{
    /// <summary>
    /// Represents a linkage between two buyers
    /// </summary>
    public class BuyerLinkage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BuyerId1 { get; set; }

        [Required]
        public int BuyerId2 { get; set; }

        [Required]
        public DateTime LinkedDate { get; set; }

        // Navigation properties
        [ForeignKey("BuyerId1")]
        public virtual Buyer? Buyer1 { get; set; }

        [ForeignKey("BuyerId2")]
        public virtual Buyer? Buyer2 { get; set; }

        /// <summary>
        /// Constructor for BuyerLinkage
        /// </summary>
        public BuyerLinkage()
        {
            LinkedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Constructor for BuyerLinkage with buyer IDs
        /// </summary>
        /// <param name="buyerId1">First buyer ID</param>
        /// <param name="buyerId2">Second buyer ID</param>
        public BuyerLinkage(int buyerId1, int buyerId2)
        {
            // Ensure consistent ordering: smaller ID first
            if (buyerId1 < buyerId2)
            {
                BuyerId1 = buyerId1;
                BuyerId2 = buyerId2;
            }
            else
            {
                BuyerId1 = buyerId2;
                BuyerId2 = buyerId1;
            }
            LinkedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Checks if this linkage involves the specified buyer
        /// </summary>
        /// <param name="buyerId">Buyer ID to check</param>
        /// <returns>True if the buyer is part of this linkage</returns>
        public bool Involvesbuyer(int buyerId)
        {
            return BuyerId1 == buyerId || BuyerId2 == buyerId;
        }

        /// <summary>
        /// Gets the other buyer ID in this linkage
        /// </summary>
        /// <param name="buyerId">Known buyer ID</param>
        /// <returns>The other buyer ID, or null if the provided ID is not in this linkage</returns>
        public int? GetOtherBuyerId(int buyerId)
        {
            if (BuyerId1 == buyerId) return BuyerId2;
            if (BuyerId2 == buyerId) return BuyerId1;
            return null;
        }
    }
}