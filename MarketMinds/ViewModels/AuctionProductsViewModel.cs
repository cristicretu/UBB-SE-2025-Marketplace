using System.Collections.Generic;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.AuctionProductsService;
using MarketMinds.Shared.Services.AuctionValidationService;

namespace MarketMinds.ViewModels;

public class AuctionProductsViewModel
{
    private readonly IAuctionProductsService auctionProductsService;
    private readonly AuctionValidationService auctionValidationService;

    public AuctionProductsViewModel(IAuctionProductsService auctionProductsService)
    {
        this.auctionProductsService = auctionProductsService;
        this.auctionValidationService = new AuctionValidationService((IAuctionProductsService)auctionProductsService);
    }

    public async Task<List<AuctionProduct>> GetAllProductsAsync()
    {
        return await ((AuctionProductsService)auctionProductsService).GetAllAuctionProductsAsync();
    }

    public async Task<List<AuctionProduct>> GetProductsAsync(int offset, int count)
    {
        return await ((AuctionProductsService)auctionProductsService).GetAllAuctionProductsAsync(offset, count);
    }

    public async Task<int> GetProductCountAsync()
    {
        return await ((AuctionProductsService)auctionProductsService).GetAuctionProductCountAsync();
    }

    // Keep the synchronous methods for backward compatibility, but they now use GetAwaiter().GetResult()
    public List<AuctionProduct> GetAllProducts()
    {
        return GetAllProductsAsync().GetAwaiter().GetResult();
    }

    public List<AuctionProduct> GetProducts(int offset, int count)
    {
        return GetProductsAsync(offset, count).GetAwaiter().GetResult();
    }

    public int GetProductCount()
    {
        return GetProductCountAsync().GetAwaiter().GetResult();
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