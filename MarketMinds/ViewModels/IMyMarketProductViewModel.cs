// <copyright file="IMyMarketProductViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketMinds.ViewModels
{
    using System.Windows.Input;
    using MarketMinds.Shared.Models;

    /// <summary>
    /// Interface representing a single product in the "My Market" view.
    /// </summary>
    public interface IMyMarketProductViewModel
    {
        /// <summary>
        /// Gets or sets the product associated with this ViewModel.
        /// </summary>
        Product Product { get; set; }

        /// <summary>
        /// Gets the command to add the product to the cart.
        /// </summary>
        ICommand AddToCartCommand { get; }
    }
}
