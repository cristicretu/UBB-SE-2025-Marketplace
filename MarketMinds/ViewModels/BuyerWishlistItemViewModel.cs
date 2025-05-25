// <copyright file="BuyerWishlistItemViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketMinds.ViewModels
{
    using System.Windows;
    using System.Windows.Input;
    using System.ComponentModel;
    using System.Collections.Generic;
    using MarketMinds.Shared.Services;
    using MarketMinds.Shared.Models;
    using MarketMinds.Shared.Helper;
    using System.Diagnostics;

    /// <summary>
    /// View model class for managing buyer wishlist item data and operations.
    /// </summary>
    public class BuyerWishlistItemViewModel : IBuyerWishlistItemViewModel
    {
        public int ProductId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
        private string imageSource = "ms-appx:///Assets/Products/default-product.png";
        public string ImageSource
        {
            get => imageSource;
            set => imageSource = string.IsNullOrEmpty(value) ? "ms-appx:///Assets/Products/default-product.png" : value;
        }
        public bool OwnItem { get; set; }
        public IOnBuyerWishlistItemRemoveCallback RemoveCallback { get; set; } = null!;
        public ICommand AddToCartCommand { get; }

        public BuyProduct Product { get; set; }
        Product IBuyerWishlistItemViewModel.Product { get => Product; set => throw new System.NotImplementedException(); }

        private IBuyerService buyerService;

        private List<BuyerWishlistItem> wishlistProductIds;

        public bool IsInWishlist(int productId)
        {
            foreach (var prod in wishlistProductIds)
            {
                Debug.WriteLine("In wishlist: " + prod.ProductId);
            }
            return wishlistProductIds.Exists(item => item.ProductId == productId);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public BuyerWishlistItemViewModel()
        {
            buyerService = App.BuyerService;

            // wishlistProductIds = buyerService.GetWishlistItems(UserSession.CurrentUserId ?? 1).Result;
            wishlistProductIds = new List<BuyerWishlistItem>();

            this.AddToCartCommand = new RelayCommand(async (product) =>
            {
                if (product is Product typedProduct)
                {
                    System.Diagnostics.Debug.WriteLine($"[AddToCart] Attempting to add product ID: {typedProduct.Id}, Title: {typedProduct.Title}");
                    var shoppingCartViewModel = new ShoppingCartViewModel(new ShoppingCartService(), buyerId: UserSession.CurrentUserId ?? 1);
                    await shoppingCartViewModel.AddToCartAsync(typedProduct, 1);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[AddToCart] Product is null or not of type Product");
                }
            });
        }

        // Dummy property to force UI update
        public bool WishlistChanged => true;

        private void NotifyWishlistChanged() => OnPropertyChanged(nameof(WishlistChanged));

        public async void AddToWishlist(int productId)
        {
            int userId = UserSession.CurrentUserId ?? 1;
            // Create a basic User object instead of fetching all users
            var user = new MarketMinds.Shared.Models.User(userId);
            user.Id = userId; // Ensure the Id is set correctly

            var buyer = await buyerService.GetBuyerByUser(user);

            System.Diagnostics.Debug.WriteLine($"[AddToWishlist] Attempting to add product ID: {productId}");
            await buyerService.AddWishlistItem(buyer, productId);
            wishlistProductIds.Add(new BuyerWishlistItem(productId));
            NotifyWishlistChanged();
        }

        public async void RemoveFromWishlist(int productId)
        {
            int userId = UserSession.CurrentUserId ?? 1;
            // Create a basic User object instead of fetching all users
            var user = new MarketMinds.Shared.Models.User(userId);
            user.Id = userId; // Ensure the Id is set correctly

            var buyer = await buyerService.GetBuyerByUser(user);

            System.Diagnostics.Debug.WriteLine($"[RemoveFromWishlist] Attempting to add product ID: {productId}");
            await buyerService.RemoveWishilistItem(buyer, productId);
            wishlistProductIds.RemoveAll(item => item.ProductId == productId);
            NotifyWishlistChanged();
        }

        public async void Remove()
        {
            if (OwnItem)
            {
                await this.RemoveCallback.OnBuyerWishlistItemRemove(this.ProductId);
            }
        }
    }
}