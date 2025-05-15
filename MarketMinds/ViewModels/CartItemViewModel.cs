using System.Windows.Input;
using MarketMinds.Shared.Models;

namespace MarketPlace924.ViewModel
{
    public class CartItemViewModel
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public double TotalPrice => Product.Price * Quantity;

        public ICommand RemoveFromCartCommand { get; }

        public CartItemViewModel(Product product, int quantity, ICommand removeFromCartCommand)
        {
            Product = product;
            Quantity = quantity;
            RemoveFromCartCommand = removeFromCartCommand;
        }
    }
}