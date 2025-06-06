﻿namespace MarketMinds.ViewModels
{
    using System.Windows.Input;
    using MarketMinds.Shared.Models;
    using MarketMinds.Shared.Services;
    using MarketMinds.Shared.Helper;

    public class MyMarketProductViewModel : IMyMarketProductViewModel
    {
        public Product Product { get; set; }
        public ICommand AddToCartCommand { get; }

        public MyMarketProductViewModel(Product product)
        {
            this.Product = product;
            this.AddToCartCommand = new RelayCommand<Product>(async (product) =>
            {
                var shoppingCartViewModel = new ShoppingCartViewModel();
                await shoppingCartViewModel.AddToCartAsync(product, 1);
            });
        }
    }
}
