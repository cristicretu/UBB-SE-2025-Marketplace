// <copyright file="BuyerWishlistItemViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.ViewModel
{
    using System;
    using System.ComponentModel;
    using System.Windows.Input;
    using SharedClassLibrary.Service;
    using CommunityToolkit.Mvvm.Input;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.ProxyRepository;
    /// <summary>
    /// View model class for managing buyer wishlist item data and operations.
    /// </summary>
    public class BuyerWishlistItemViewModel : IBuyerWishlistItemViewModel
    {
        public int ProductId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ImageSource { get; set; } = string.Empty;
        public bool OwnItem { get; set; } = true;
        public IOnBuyerWishlistItemRemoveCallback RemoveCallback { get; set; } = null!;
        public ICommand AddToCartCommand { get; }

        public Product Product { get; set; }

        public BuyerWishlistItemViewModel()
        {
            this.AddToCartCommand = new RelayCommand<Product>(async (product) =>
            {
                var shoppingCartViewModel = new ShoppingCartViewModel(new ShoppingCartService(), buyerId: UserSession.CurrentUserId ?? 1);
                await shoppingCartViewModel.AddToCartAsync(product, 1);
            });
        }

        public async void Remove()
        {
            await this.RemoveCallback.OnBuyerWishlistItemRemove(this.ProductId);
        }
    }
}