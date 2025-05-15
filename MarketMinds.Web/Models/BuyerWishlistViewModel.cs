// BuyerWishlistViewModel.cs
using SharedClassLibrary.Domain;
using System.Collections.Generic;

namespace WebMarketplace.Models
{
    /// <summary>
    /// View model for the buyer wishlist page
    /// </summary>
    public class BuyerWishlistViewModel
    {
        /// <summary>
        /// Gets or sets the buyer ID
        /// </summary>
        public int BuyerId { get; set; }

        /// <summary>
        /// Gets or sets the list of wishlist items
        /// </summary>
        public List<Product> WishlistItems { get; set; }

        /// <summary>
        /// Gets or sets the count of items in the wishlist
        /// </summary>
        public int ItemCount => WishlistItems?.Count ?? 0;

        /// <summary>
        /// Gets a value indicating whether the wishlist is empty
        /// </summary>
        public bool IsEmpty => ItemCount == 0;
    }
}
