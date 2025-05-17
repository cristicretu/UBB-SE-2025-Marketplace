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
        public string ImageSource { get; set; } = string.Empty;
        public bool OwnItem { get; set; } = true;
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
                    var shoppingCartViewModel = new ShoppingCartViewModel(new ShoppingCartService(), buyerId: UserSession.CurrentUserId ?? 1);
                    await shoppingCartViewModel.AddToCartAsync(typedProduct, 1);
                }
            });
        }

        public async void Remove()
        {
            await this.RemoveCallback.OnBuyerWishlistItemRemove(this.ProductId);
        }
    }
}