﻿// <copyright file="IBuyerWishlistItemDetailsProvider.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketMinds.ViewModels
{
    /// <summary>
    /// Interface for providing details for wishlist items.
    /// </summary>
    public interface IBuyerWishlistItemDetailsProvider
    {
        /// <summary>
        /// Loads additional details for a wishlist item.
        /// </summary>
        /// <param name="item">The item to load details for.</param>
        /// <returns>The wishlist item with loaded details.</returns>
        IBuyerWishlistItemViewModel LoadWishlistItemDetails(IBuyerWishlistItemViewModel item);
    }
}
