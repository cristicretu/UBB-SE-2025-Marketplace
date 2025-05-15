namespace MarketPlace924.ViewModel
{
    using System.Windows.Input;
    using MarketMinds.Shared.Models;
    using SharedClassLibrary.Service;

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
