using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.ListingCreationService;

namespace MarketMinds.ViewModels
{
    public class CreateListingViewModel
    {
        private readonly ListingCreationService listingCreationService;
        public Product Product { get; set; }
        public string ListingType { get; set; }

        public CreateListingViewModel(ListingCreationService listingCreationService)
        {
            this.listingCreationService = listingCreationService;
        }

        public void CreateMarketPlaceListing()
        {
            listingCreationService.CreateMarketListing(Product, ListingType);
        }
    }
}
