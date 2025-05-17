using System.Collections.Generic;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.AuctionProductsService;
using MarketMinds.Shared.Services.AuctionValidationService;

namespace ViewModelLayer.ViewModel;

public class AuctionProductsViewModel
{
    private readonly IAuctionProductsService auctionProductsService;
    private readonly AuctionValidationService auctionValidationService;

    public AuctionProductsViewModel(IAuctionProductsService auctionProductsService)
    {
        this.auctionProductsService = auctionProductsService;
        this.auctionValidationService = new AuctionValidationService((IAuctionProductsService)auctionProductsService);
    }

    public List<AuctionProduct> GetAllProducts()
    {
        var auctionProducts = new List<AuctionProduct>();
        foreach (var product in auctionProductsService.GetProducts())
        {
            auctionProducts.Add((AuctionProduct)product);
        }
        return auctionProducts;
    }

    public void PlaceBid(AuctionProduct product, User bidder, string enteredBidText)
    {
        auctionValidationService.ValidateAndPlaceBid(product, bidder, enteredBidText);
    }

    public void ConcludeAuction(AuctionProduct product)
    {
        auctionValidationService.ValidateAndConcludeAuction(product);
    }

    public string GetTimeLeft(AuctionProduct product)
    {
        return ((IAuctionProductsService)auctionProductsService).GetTimeLeft(product);
    }

    public bool IsAuctionEnded(AuctionProduct product)
    {
        return ((IAuctionProductsService)auctionProductsService).IsAuctionEnded(product);
    }
}