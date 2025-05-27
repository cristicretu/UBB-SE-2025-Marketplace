// <copyright file="IBuyerWishlistItemViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketMinds.ViewModels
{
    using System.Windows.Input;
    using MarketMinds.Shared.Models;

    /// <summary>
    /// Interface for managing buyer wishlist item view model operations.
    /// </summary>
    public interface IBuyerWishlistItemViewModel
    {
        /// <summary>
        /// Gets or sets the product identifier.
        /// </summary>
        int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the product title.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Gets or sets the product price.
        /// </summary>
        decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the product description.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Gets or sets the product image source path.
        /// </summary>
        string ImageSource { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the item belongs to the current buyer.
        /// </summary>
        bool OwnItem { get; set; }

        /// <summary>
        /// Gets or sets the callback for item removal operations.
        /// </summary>
        IOnBuyerWishlistItemRemoveCallback RemoveCallback { get; set; }

        /// <summary>
        /// Removes the item from the wishlist.
        /// </summary>
        void Remove();
        
        /// <summary>
        /// Gets or sets the product associated with this wishlist item.
        /// </summary>
        Product Product { get; set; }
        
        /// <summary>
        /// Gets the command for adding the item to the cart.
        /// </summary>
        ICommand AddToCartCommand { get; }
        
        /// <summary>
        /// Gets the stock value from the Product, or 0 if Product is null.
        /// </summary>
        int Stock { get; }
    }
}
