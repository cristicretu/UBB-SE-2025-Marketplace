// <copyright file="BuyerWishlist.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SharedClassLibrary.Domain
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a buyer's wishlist containing products they are interested in.
    /// This class manages a collection of products that the buyer wants to track or purchase later.
    /// </summary>
    /// <seealso cref="Buyer"/>
    /// <seealso cref="BuyerWishlistItem"/>
    public class BuyerWishlist
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuyerWishlist"/> class.
        /// Creates an empty collection of items and initializes the code as empty.
        /// </summary>
        public BuyerWishlist()
        {
            this.Items = new List<BuyerWishlistItem>();
            this.Code = string.Empty;
        }

        /// <summary>
        /// Gets the collection of items in the wishlist.
        /// </summary>
        public List<BuyerWishlistItem> Items { get; set; }

        /// <summary>
        /// Gets the unique code associated with the wishlist.
        /// </summary>
        public string Code { get; }
    }
}