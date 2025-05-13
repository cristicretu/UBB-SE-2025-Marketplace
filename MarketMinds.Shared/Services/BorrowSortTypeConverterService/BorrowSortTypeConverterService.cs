using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Services.BorrowSortTypeConverterService
{
    public class BorrowSortTypeConverterService : IBorrowSortTypeConverterService
    {
        public ProductSortType Convert(string sortTag)
        {
            switch (sortTag)
            {
                case "SellerRatingAsc":
                    return new ProductSortType("Seller Rating", "SellerRating", true);
                case "SellerRatingDesc":
                    return new ProductSortType("Seller Rating", "SellerRating", false);
                case "DailyRateAsc":
                    return new ProductSortType("Daily Rate", "DailyRate", true);
                case "DailyRateDesc":
                    return new ProductSortType("Daily Rate", "DailyRate", false);
                case "StartDateAsc":
                    return new ProductSortType("Start Date", "StartDate", true);
                case "StartDateDesc":
                    return new ProductSortType("Start Date", "StartDate", false);
                default:
                    return null;
            }
        }
    }
}