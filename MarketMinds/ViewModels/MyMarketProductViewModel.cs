using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketPlace924.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.Service;
    using Microsoft.UI.Xaml;
    using CommunityToolkit.Mvvm.Input;

    public class MyMarketProductViewModel : IMyMarketProductViewModel
    {
        public Product Product { get; set; }
        public ICommand AddToCartCommand { get; }

        public MyMarketProductViewModel(Product product)
        {
            this.Product = product;
            this.AddToCartCommand = new RelayCommand<Product>(async (product) =>
            {
                var shoppingCartViewModel = new ShoppingCartViewModel(new ShoppingCartService(), buyerId: UserSession.CurrentUserId ?? 1);
                await shoppingCartViewModel.AddToCartAsync(product, 1);
            });
        }
    }
}
