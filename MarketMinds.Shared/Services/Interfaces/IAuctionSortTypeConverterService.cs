using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Services.AuctionSortTypeConverterService
{
    /// <summary>
    /// Interface for AuctionSortTypeConverterService to manage sort type conversions.
    /// </summary>
    public interface IAuctionSortTypeConverterService
    {
        /// <summary>
        /// Converts a sort tag into a ProductSortType.
        /// </summary>
        /// <param name="sortTag">The sort tag to be converted.</param>
        /// <returns>A ProductSortType object corresponding to the sort tag.</returns>
        ProductSortType Convert(string sortTag);
    }
}
