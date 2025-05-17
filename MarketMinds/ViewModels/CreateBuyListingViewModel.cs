using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.BuyProductsService;

namespace ViewModelLayer.ViewModel
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
