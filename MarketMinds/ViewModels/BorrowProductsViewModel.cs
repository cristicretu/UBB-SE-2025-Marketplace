using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.BorrowProductsService;

namespace MarketMinds.ViewModels;

public class BorrowProductsViewModel
{
    private readonly IBorrowProductsService borrowProductsService;

    public BorrowProductsViewModel(BorrowProductsService borrowProductsService)
    {
        this.borrowProductsService = borrowProductsService;
    }

    public async Task<List<BorrowProduct>> GetAllProductsAsync()
    {
        return await borrowProductsService.GetAllBorrowProductsAsync();
    }

    public async Task<List<BorrowProduct>> GetProductsAsync(int offset, int count)
    {
        return await borrowProductsService.GetAllBorrowProductsAsync(offset, count);
    }

    public async Task<int> GetProductCountAsync()
    {
        return await borrowProductsService.GetBorrowProductCountAsync();
    }

    // Keep the synchronous methods for backward compatibility, but they now use GetAwaiter().GetResult()
    public List<BorrowProduct> GetAllProducts()
    {
        return GetAllProductsAsync().GetAwaiter().GetResult();
    }

    public List<BorrowProduct> GetProducts(int offset, int count)
    {
        return GetProductsAsync(offset, count).GetAwaiter().GetResult();
    }

    public int GetProductCount()
    {
        return GetProductCountAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Fetch a single BorrowProduct by its ID.
    /// </summary>
    public async Task<BorrowProduct> GetBorrowProductByIdAsync(int id)
    {
        // The service already exposes an async method for this
        return await borrowProductsService.GetBorrowProductByIdAsync(id);
    }

    /// <summary>
    /// Gets filtered borrow products asynchronously based on provided filters
    /// </summary>
    public async Task<List<BorrowProduct>> GetFilteredProductsAsync(int offset, int count, List<int> conditionIds = null, List<int> categoryIds = null, double? maxPrice = null, string searchTerm = null)
    {
        try
        {
            var products = await Task.Run(() => borrowProductsService.GetFilteredProducts(offset, count, conditionIds, categoryIds, maxPrice, searchTerm, null));
            return products;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting filtered borrow products: {ex.Message}");
            return new List<BorrowProduct>();
        }
    }

    /// <summary>
    /// Gets the count of filtered borrow products asynchronously using dedicated count API
    /// </summary>
    public async Task<int> GetFilteredProductCountAsync(List<int> conditionIds = null, List<int> categoryIds = null, double? maxPrice = null, string searchTerm = null)
    {
        try
        {
            return await Task.Run(() => borrowProductsService.GetFilteredProductCount(conditionIds, categoryIds, maxPrice, searchTerm, null));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting filtered borrow product count: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Gets filtered borrow products asynchronously based on provided filters including seller filter
    /// </summary>
    public async Task<List<BorrowProduct>> GetFilteredProductsWithSellerAsync(int offset, int count, List<int> conditionIds = null, List<int> categoryIds = null, double? maxPrice = null, string searchTerm = null, int? sellerId = null)
    {
        try
        {
            return await Task.Run(() => borrowProductsService.GetFilteredProducts(offset, count, conditionIds, categoryIds, maxPrice, searchTerm, sellerId));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting filtered borrow products with seller filter: {ex.Message}");
            return new List<BorrowProduct>();
        }
    }

    /// <summary>
    /// Gets the count of filtered borrow products asynchronously using dedicated count API with seller filter
    /// </summary>
    public async Task<int> GetFilteredProductCountWithSellerAsync(List<int> conditionIds = null, List<int> categoryIds = null, double? maxPrice = null, string searchTerm = null, int? sellerId = null)
    {
        try
        {
            return await Task.Run(() => borrowProductsService.GetFilteredProductCount(conditionIds, categoryIds, maxPrice, searchTerm, sellerId));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting filtered borrow product count with seller filter: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Gets the maximum daily rate of all borrow products asynchronously
    /// </summary>
    public async Task<double> GetMaxPriceAsync()
    {
        try
        {
            return await borrowProductsService.GetMaxPriceAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting max price for borrow products: {ex.Message}");
            return 0.0;
        }
    }
}