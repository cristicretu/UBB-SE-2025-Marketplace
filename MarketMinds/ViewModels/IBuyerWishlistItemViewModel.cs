// <copyright file="IBuyerWishlistItemViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.ViewModel
{
    using SharedClassLibrary.Domain;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Input;

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
        Product Product { get; set; }
        ICommand AddToCartCommand { get; }
    }
}
