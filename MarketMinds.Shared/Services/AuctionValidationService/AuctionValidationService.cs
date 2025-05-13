using System;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.AuctionProductsService;

namespace MarketMinds.Shared.Services.AuctionValidationService
{
    public class AuctionValidationService : IAuctionValidationService
    {
        private readonly IAuctionProductsService auctionProductsService;

        public AuctionValidationService(IAuctionProductsService auctionProductsService)
        {
            this.auctionProductsService = auctionProductsService;
        }

        public void ValidateAndPlaceBid(AuctionProduct product, User bidder, string enteredBidText)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product), "Auction product cannot be null");
            }
            
            if (bidder == null)
            {
                throw new ArgumentNullException(nameof(bidder), "Bidder cannot be null");
            }
            
            if (string.IsNullOrWhiteSpace(enteredBidText))
            {
                throw new ArgumentException("Bid amount is required");
            }
            
            if (!double.TryParse(enteredBidText, out double bidAmount))
            {
                throw new ArgumentException($"Invalid bid format: '{enteredBidText}' is not a valid number");
            }
            
            if (bidAmount <= 0)
            {
                throw new ArgumentException("Bid amount must be positive");
            }

            try
            {
                auctionProductsService.PlaceBid(product, bidder, bidAmount);
            }
            catch (Exception exception)
            {
                throw new Exception($"Bid failed: {exception.Message}", exception);
            }
        }

        public void ValidateAndConcludeAuction(AuctionProduct product)
        {
            if (product == null)
            {
                throw new ArgumentException("Product cannot be null");
            }

            auctionProductsService.ConcludeAuction(product);
        }
    }
}