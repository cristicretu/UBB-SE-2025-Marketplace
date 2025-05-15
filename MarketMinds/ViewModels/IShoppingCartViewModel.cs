using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;

namespace MarketPlace924.ViewModel
{
    public interface IShoppingCartViewModel

    {
        ObservableCollection<CartItemViewModel> CartItems { get; }
        Task LoadCartItemsAsync();
        Task AddToCartAsync(Product product, int quantity);
        Task RemoveFromCartAsync(Product product);
        Task UpdateQuantityAsync(Product product, int quantity);
    }
}
