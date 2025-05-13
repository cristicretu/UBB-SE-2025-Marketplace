using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Services.AuctionSortTypeConverterService
{
    public class AuctionSortTypeConverterService : IAuctionSortTypeConverterService
    {
        public ProductSortType Convert(string sortTag)
        {
            switch (sortTag)
            {
                case "SellerRatingAsc":
                    return new ProductSortType("Seller Rating", "SellerRating", true);
                case "SellerRatingDesc":
                    return new ProductSortType("Seller Rating", "SellerRating", false);
                case "StartingPriceAsc":
                    return new ProductSortType("Starting Price", "StartingPrice", true);
                case "StartingPriceDesc":
                    return new ProductSortType("Starting Price", "StartingPrice", false);
                case "CurrentPriceAsc":
                    return new ProductSortType("Current Price", "CurrentPrice", true);
                case "CurrentPriceDesc":
                    return new ProductSortType("Current Price", "CurrentPrice", false);
                default:
                    return null;
            }
        }
    }
}