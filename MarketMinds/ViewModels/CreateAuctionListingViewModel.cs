using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.AuctionProductsService;
using ViewModelLayer.ViewModel;

namespace MarketMinds.ViewModels
{
    public class CreateAuctionListingViewModel : CreateListingViewModelBase
    {
        private readonly AuctionProductsService auctionProductsService;

        public CreateAuctionListingViewModel(AuctionProductsService auctionProductsService)
        {
            this.auctionProductsService = auctionProductsService;
        }

        public override void CreateListing(Product product)
        {
            auctionProductsService.CreateListing(product);
        }
    }
}
