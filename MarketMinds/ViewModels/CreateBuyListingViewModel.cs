using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.BuyProductsService;

namespace MarketMinds.ViewModels
{
    public class CreateBuyListingViewModel : CreateListingViewModelBase
    {
        public BuyProductsService BuyProductsService { get; set; }
        public override void CreateListing(Product product)
        {
            if (product is BuyProduct buyProduct)
            {
                BuyProductsService.CreateListing(buyProduct);
            }
        }
    }
}
