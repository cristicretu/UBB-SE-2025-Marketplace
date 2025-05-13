using System;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.BuyProductsService;
using MarketMinds.Shared.Services.BorrowProductsService;
using MarketMinds.Shared.Services.AuctionProductsService;

namespace MarketMinds.Shared.Services.ListingCreationService
{
    public class ListingCreationService : IListingCreationService
    {
        private readonly IBuyProductsService buyProductsService;
        private readonly IBorrowProductsService borrowProductsService;
        private readonly IAuctionProductsService auctionProductsService;

        public ListingCreationService(
            IBuyProductsService buyProductsService,
            IBorrowProductsService borrowProductsService,
            IAuctionProductsService auctionProductsService)
        {
            this.buyProductsService = buyProductsService;
            this.borrowProductsService = borrowProductsService;
            this.auctionProductsService = auctionProductsService;
        }

        public void CreateMarketListing(Product product, string listingType)
        {
            switch (listingType.ToLower())
            {
                case "buy":
                    if (product is BuyProduct buyProduct)
                    {
                        buyProductsService.CreateListing(buyProduct);
                    }
                    break;
                case "borrow":
                    borrowProductsService.CreateListing(product);
                    break;
                case "auction":
                    auctionProductsService.CreateListing(product);
                    break;
                default:
                    throw new ArgumentException($"Invalid listing type: {listingType}");
            }
        }
    }
}