// <copyright file="BuyerWishlistItemViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketMinds.ViewModels
{
    using System.Windows.Input;
    using MarketMinds.Shared.Services;
    using MarketMinds.Shared.Models;
    using MarketMinds.Shared.Helper;

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

        public BuyerWishlistItemViewModel()
        {
            this.AddToCartCommand = new RelayCommand(async (product) =>
            {
                if (product is Product typedProduct)
                {
                    System.Diagnostics.Debug.WriteLine($"[AddToCart] Attempting to add product ID: {typedProduct.Id}, Title: {typedProduct.Title}");
                    var shoppingCartViewModel = new ShoppingCartViewModel();
                    await shoppingCartViewModel.AddToCartAsync(typedProduct, 1);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[AddToCart] Product is null or not of type Product");
                }
            });
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