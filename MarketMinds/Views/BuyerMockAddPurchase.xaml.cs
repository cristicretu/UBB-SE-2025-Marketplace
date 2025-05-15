// <copyright file="BuyerMockAddPurchase.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.View
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using MarketPlace924.ViewModel;
    using Microsoft.UI.Xaml.Controls;

    /// <summary>
    /// A user control that provides functionality for adding mock purchases in the marketplace.
    /// This control is used for testing and demonstration purposes.
    /// </summary>
    public sealed partial class BuyerMockAddPurchase : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuyerMockAddPurchase"/> class.
        /// </summary>
        public BuyerMockAddPurchase()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the view model associated with this control.
        /// </summary>
        public IBuyerProfileViewModel? ViewModel { get; set; }

        /// <summary>
        /// Gets or sets the amount of the mock purchase.
        /// </summary>
        private string? PurchaseAmount { get; set; }

        /// <summary>
        /// Processes the mock purchase with the specified amount.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task AddPurchase()
        {
            if (this.ViewModel == null)
            {
                return;
            }

            await this.ViewModel.AddPurchase(this.PurchaseAmount ?? string.Empty);
        }
    }
}