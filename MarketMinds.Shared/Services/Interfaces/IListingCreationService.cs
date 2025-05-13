using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Services.ListingCreationService
{
    /// <summary>
    /// Interface for ListingCreationService to manage market listing creation.
    /// </summary>
    public interface IListingCreationService
    {
        /// <summary>
        /// Creates a market listing based on the specified listing type.
        /// </summary>
        /// <param name="product">The product to be listed.</param>
        /// <param name="listingType">The type of listing (e.g., "buy", "borrow", "auction").</param>
        void CreateMarketListing(Product product, string listingType);
    }
}
