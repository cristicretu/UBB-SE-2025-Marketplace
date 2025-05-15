// <copyright file="IOnBuyerWishlistItemRemoveCallback.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.ViewModel
{
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for handling wishlist item removal callbacks.
    /// </summary>
    public interface IOnBuyerWishlistItemRemoveCallback
    {
        /// <summary>
        /// Called when a wishlist item is removed.
        /// </summary>
        /// <param name="productId">The ID of the product being removed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task OnBuyerWishlistItemRemove(int productId);
    }
}