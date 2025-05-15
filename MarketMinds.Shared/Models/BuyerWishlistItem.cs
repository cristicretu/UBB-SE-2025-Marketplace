// <copyright file="BuyerWishlistItem.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SharedClassLibrary.Domain
{
    /// <summary>
    /// Represents an item in a buyer's wishlist.
    /// This class maintains a reference to a product that the buyer is interested in.
    /// </summary>
    /// <seealso cref="Buyer"/>
    /// <seealso cref="BuyerWishlist"/>
    /// <remarks>
    /// Initializes a new instance of the <see cref="BuyerWishlistItem"/> class.
    /// </remarks>
    /// <param name="productId">The unique identifier of the product to add to the wishlist.</param>
    public class BuyerWishlistItem(int productId)
    {
        /// <summary>
        /// The unique identifier of the product in the wishlist.
        /// </summary>
        private int productId = productId;

        /// <summary>
        /// Gets the unique identifier of the product in the wishlist.
        /// </summary>
        public int ProductId { get => this.productId; set => this.productId = value; }
    }
}