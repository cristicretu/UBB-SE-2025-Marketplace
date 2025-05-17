// <copyright file="BuyerCartItemEntity.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Server.DataModels
{
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents the BuyerCartItem table structure for EF Core mapping.
    /// </summary>
    [Table("BuyerCartItems")]
    public class BuyerCartItemEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuyerCartItemEntity"/> class.
        /// </summary>
        /// <param name="buyerId">The buyer ID.</param>
        /// <param name="productId">The product ID.</param>
        /// <param name="quantity">The quantity.</param>
        public BuyerCartItemEntity(int buyerId, int productId, int quantity)
        {
            this.BuyerId = buyerId;
            this.ProductId = productId;
            this.Quantity = quantity;
        }

        /// <summary>
        /// Gets or sets the buyer ID.
        /// </summary>
        public int BuyerId { get; set; }

        /// <summary>
        /// Gets or sets the product ID.
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the quantity.
        /// </summary>
        public int Quantity { get; set; }
    }
}