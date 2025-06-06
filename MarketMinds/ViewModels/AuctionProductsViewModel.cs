﻿using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.AuctionValidationService;
using MarketMinds.Shared.Services.Interfaces;
using MarketMinds.Shared.Services.AuctionProductsService;

namespace MarketMinds.ViewModels;

public class AuctionProductsViewModel
{
    private readonly AuctionProductsService auctionProductsService;
    private readonly AuctionValidationService auctionValidationService;

    public AuctionProductsViewModel(AuctionProductsService auctionProductsService)
    {
        this.auctionProductsService = auctionProductsService;
        this.auctionValidationService = new AuctionValidationService((IAuctionProductsService)auctionProductsService);
    }

    public async Task<List<AuctionProduct>> GetAllProductsAsync()
    {
        return await auctionProductsService.GetAllAuctionProductsAsync();
    }

    public async Task<List<AuctionProduct>> GetProductsAsync(int offset, int count)
    {
        return await auctionProductsService.GetAllAuctionProductsAsync(offset, count);
    }

    public async Task<int> GetProductCountAsync()
    {
        return await auctionProductsService.GetAuctionProductCountAsync();
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
        return auctionProductsService.GetTimeLeft(product);
    }

    public bool IsAuctionEnded(AuctionProduct product)
    {
        return auctionProductsService.IsAuctionEnded(product);
    }

    /// <summary>
    /// Gets filtered auction products asynchronously based on provided filters
    /// </summary>
    public async Task<List<AuctionProduct>> GetFilteredProductsAsync(int offset, int count, List<int> conditionIds = null, List<int> categoryIds = null, double? maxPrice = null, string searchTerm = null)
    {
        try
        {
            var products = await Task.Run(() => auctionProductsService.GetFilteredAuctionProductsAsync(offset, count, conditionIds, categoryIds, maxPrice, searchTerm, null));
            return products;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting filtered auction products: {ex.Message}");
            return new List<AuctionProduct>();
        }
    }

    /// <summary>
    /// Gets the count of filtered auction products asynchronously using dedicated count API
    /// </summary>
    public async Task<int> GetFilteredProductCountAsync(List<int> conditionIds = null, List<int> categoryIds = null, double? maxPrice = null, string searchTerm = null)
    {
        try
        {
            return await auctionProductsService.GetFilteredAuctionProductCountAsync(conditionIds, categoryIds, maxPrice, searchTerm, null);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting filtered auction product count: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Gets filtered auction products asynchronously based on provided filters including seller filter
    /// </summary>
    public async Task<List<AuctionProduct>> GetFilteredProductsWithSellerAsync(int offset, int count, List<int> conditionIds = null, List<int> categoryIds = null, double? maxPrice = null, string searchTerm = null, int? sellerId = null)
    {
        try
        {
            var products = await Task.Run(() => auctionProductsService.GetFilteredAuctionProductsAsync(offset, count, conditionIds, categoryIds, maxPrice, searchTerm, sellerId));
            return products;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting filtered auction products with seller filter: {ex.Message}");
            return new List<AuctionProduct>();
        }
    }

    /// <summary>
    /// Gets the count of filtered auction products asynchronously using dedicated count API with seller filter
    /// </summary>
    public async Task<int> GetFilteredProductCountWithSellerAsync(List<int> conditionIds = null, List<int> categoryIds = null, double? maxPrice = null, string searchTerm = null, int? sellerId = null)
    {
        try
        {
            // Use dedicated count API instead of fetching all products
            return await auctionProductsService.GetFilteredAuctionProductCountAsync(conditionIds, categoryIds, maxPrice, searchTerm, sellerId);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting filtered auction product count with seller filter: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Gets the maximum current price of all auction products asynchronously
    /// </summary>
    public async Task<double> GetMaxPriceAsync()
    {
        try
        {
            return await auctionProductsService.GetMaxPriceAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting max price for auction products: {ex.Message}");
            return 0.0;
        }
    }
}