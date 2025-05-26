using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;

namespace MarketMinds.ViewModels
{
    public interface IShoppingCartViewModel
    {
        ObservableCollection<CartItemViewModel> CartItems { get; }
        bool IsLoading { get; }
        Task LoadCartItemsAsync();
        Task AddToCartAsync(Product product, int quantity);
        Task RemoveFromCartAsync(Product product);
        Task UpdateQuantityAsync(Product product, int quantity);
    }
}
