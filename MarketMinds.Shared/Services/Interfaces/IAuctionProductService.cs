using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Services.Interfaces
{
    public interface IAuctionProductService
    {
        Task<List<AuctionProduct>> GetAllAuctionProductsAsync();
        Task<AuctionProduct> GetAuctionProductByIdAsync(int id);
        Task<bool> CreateAuctionProductAsync(AuctionProduct auctionProduct);
        Task<bool> PlaceBidAsync(int auctionId, int bidderId, double bidAmount);
        Task<bool> UpdateAuctionProductAsync(AuctionProduct auctionProduct);
        Task<bool> DeleteAuctionProductAsync(int id);
        
        // Business logic methods
        void ValidateBid(AuctionProduct auction, int bidderId, double bidAmount);
        void ExtendAuctionTimeIfNeeded(AuctionProduct auction);
        void SetDefaultAuctionTimes(AuctionProduct product);
        void SetDefaultPricing(AuctionProduct product);
        void ProcessRefundForPreviousBidder(AuctionProduct product, double newBidAmount);
        bool IsAuctionEnded(AuctionProduct auction);
    }
} 