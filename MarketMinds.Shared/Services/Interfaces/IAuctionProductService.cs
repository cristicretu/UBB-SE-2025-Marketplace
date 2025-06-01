using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Services.Interfaces
{
    public interface IAuctionProductService
    {
        Task<List<AuctionProduct>> GetAllAuctionProductsAsync();
        Task<List<AuctionProduct>> GetAllAuctionProductsAsync(int offset, int count);
        Task<int> GetAuctionProductCountAsync();
        Task<AuctionProduct> GetAuctionProductByIdAsync(int id);
        Task<bool> CreateAuctionProductAsync(AuctionProduct auctionProduct);
        Task<bool> PlaceBidAsync(int auctionId, int bidderId, double bidAmount);
        Task<bool> UpdateAuctionProductAsync(AuctionProduct auctionProduct);
        Task<bool> DeleteAuctionProductAsync(int id);

        /// <summary>
        /// Gets filtered auction products with pagination support.
        /// </summary>
        /// <param name="offset">The number of products to skip (0 for first page).</param>
        /// <param name="count">The number of products to return (0 for all products).</param>
        /// <param name="conditionIds">List of condition IDs to filter by (null or empty for no filter).</param>
        /// <param name="categoryIds">List of category IDs to filter by (null or empty for no filter).</param>
        /// <param name="maxPrice">Maximum price filter (null for no maximum).</param>
        /// <param name="searchTerm">Search term to filter by title, description, or seller (null or empty for no search).</param>
        /// <returns>A list of filtered auction products for the specified page.</returns>
        Task<List<AuctionProduct>> GetFilteredAuctionProductsAsync(int offset, int count, List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null);

        /// <summary>
        /// Gets the total count of auction products matching the specified filters.
        /// </summary>
        /// <param name="conditionIds">List of condition IDs to filter by (null or empty for no filter).</param>
        /// <param name="categoryIds">List of category IDs to filter by (null or empty for no filter).</param>
        /// <param name="maxPrice">Maximum price filter (null for no maximum).</param>
        /// <param name="searchTerm">Search term to filter by title, description, or seller (null or empty for no search).</param>
        /// <returns>The total number of auction products matching the filters.</returns>
        Task<int> GetFilteredAuctionProductCountAsync(List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null);

        /// <summary>
        /// Gets filtered auction products with pagination support including seller filter.
        /// </summary>
        /// <param name="offset">The number of products to skip (0 for first page).</param>
        /// <param name="count">The number of products to return (0 for all products).</param>
        /// <param name="conditionIds">List of condition IDs to filter by (null or empty for no filter).</param>
        /// <param name="categoryIds">List of category IDs to filter by (null or empty for no filter).</param>
        /// <param name="maxPrice">Maximum price filter (null for no maximum).</param>
        /// <param name="searchTerm">Search term to filter by title, description, or seller (null or empty for no search).</param>
        /// <param name="sellerId">Seller ID to filter by (null for no seller filter).</param>
        /// <returns>A list of filtered auction products for the specified page.</returns>
        Task<List<AuctionProduct>> GetFilteredAuctionProductsAsync(int offset, int count, List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null, int? sellerId = null);

        /// <summary>
        /// Gets the total count of auction products matching the specified filters including seller filter.
        /// </summary>
        /// <param name="conditionIds">List of condition IDs to filter by (null or empty for no filter).</param>
        /// <param name="categoryIds">List of category IDs to filter by (null or empty for no filter).</param>
        /// <param name="maxPrice">Maximum price filter (null for no maximum).</param>
        /// <param name="searchTerm">Search term to filter by title, description, or seller (null or empty for no search).</param>
        /// <param name="sellerId">Seller ID to filter by (null for no seller filter).</param>
        /// <returns>The total number of auction products matching the filters.</returns>
        Task<int> GetFilteredAuctionProductCountAsync(List<int>? conditionIds = null, List<int>? categoryIds = null, double? maxPrice = null, string? searchTerm = null, int? sellerId = null);

        // Business logic methods
        void ValidateBid(AuctionProduct auction, int bidderId, double bidAmount);
        void ExtendAuctionTimeIfNeeded(AuctionProduct auction);
        void SetDefaultAuctionTimes(AuctionProduct product);
        void SetDefaultPricing(AuctionProduct product);
        void ProcessRefundForPreviousBidder(AuctionProduct product, double newBidAmount);
        bool IsAuctionEnded(AuctionProduct auction);
    }
}