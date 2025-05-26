using MarketMinds.Shared.Models;

namespace MarketMinds.ViewModels
{
    public interface ICartItemViewModel
    {
        Product Product { get; set; }
        int Quantity { get; set; }
        double TotalPrice { get; }
    }
}