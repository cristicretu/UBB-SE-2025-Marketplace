using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Services.AuctionValidationService
{
    /// <summary>
    /// Interface for AuctionValidationService to manage auction validation operations.
    /// </summary>
    public interface IAuctionValidationService
    {
        /// <summary>
        /// Validates and places a bid on an auction product.
        /// </summary>
        /// <param name="product">The auction product.</param>
        /// <param name="bidder">The user placing the bid.</param>
        /// <param name="enteredBidText">The bid amount as a string.</param>
        void ValidateAndPlaceBid(AuctionProduct product, User bidder, string enteredBidText);

        /// <summary>
        /// Validates and concludes an auction.
        /// </summary>
        /// <param name="product">The auction product to conclude.</param>
        void ValidateAndConcludeAuction(AuctionProduct product);
    }
}