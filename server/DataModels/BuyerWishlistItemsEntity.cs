// <copyright file="BuyerWishlistItemsEntity.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.DataModels
{
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents the BuyerWishlistItems table structure for EF Core mapping.
    /// </summary>
    [Table("BuyerWishlistItems")]
    public class BuyerWishlistItemsEntity
    {
        /// <summary>
        /// Gets or sets the buyer ID.
        /// </summary>
        public int BuyerId { get; set; }

        /// <summary>
        /// Gets or sets the product ID.
        /// </summary>
        public int ProductId { get; set; }
    }
}