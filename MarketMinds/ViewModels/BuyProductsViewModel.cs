using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.BuyProductsService;

namespace MarketMinds.ViewModels;

public class BuyProductsViewModel
{
    private readonly BuyProductsService buyProductsService;

    public BuyProductsViewModel(BuyProductsService buyProductsService)
    {
        this.buyProductsService = buyProductsService;
    }

    public List<BuyProduct> GetAllProducts()
    {
        // var buyProducts = new List<BuyProduct>();
        // foreach (var product in buyProductsService.GetProducts())
        // {
        //     buyProducts.Add((BuyProduct)product);
        // }
        // return buyProducts;
        return buyProductsService.GetProducts(); // not to complicate stuff
    }

    public List<BuyProduct> GetProducts(int offset, int count)
    {
        return buyProductsService.GetProducts(offset, count);
    }

    public int GetProductCount()
    {
        return buyProductsService.GetProductCount();
    }

    /// <summary>
    /// Gets filtered products based on the provided filters
    /// </summary>
    public List<BuyProduct> GetFilteredProducts(int offset, int count, List<int> conditionIds = null, List<int> categoryIds = null, double? maxPrice = null, string searchTerm = null)
    {
        try
        {
            var products = buyProductsService.GetFilteredProducts(offset, count, conditionIds, categoryIds, maxPrice, searchTerm);
            return products;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting filtered products: {ex.Message}");
            return new List<BuyProduct>();
        }
    }

    /// <summary>
    /// Gets filtered products asynchronously based on provided filters
    /// </summary>
    public async Task<List<BuyProduct>> GetFilteredProductsAsync(int offset, int count, List<int> conditionIds = null, List<int> categoryIds = null, double? maxPrice = null, string searchTerm = null)
    {
        try
        {
            // Always call the sellerId version with null to avoid ambiguity
            return await Task.Run(() => buyProductsService.GetFilteredProducts(offset, count, conditionIds, categoryIds, maxPrice, searchTerm, null));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting filtered products asynchronously: {ex.Message}");
            return new List<BuyProduct>();
        }
    }

    /// <summary>
    /// Gets the count of filtered products using dedicated count API
    /// </summary>
    public int GetFilteredProductCount(List<int> conditionIds = null, List<int> categoryIds = null, double? maxPrice = null, string searchTerm = null)
    {
        try
        {
            // Use dedicated count API instead of fetching all products
            return buyProductsService.GetFilteredProductCount(conditionIds, categoryIds, maxPrice, searchTerm);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting filtered product count: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Gets the count of filtered products asynchronously using direct server count API
    /// </summary>
    public async Task<int> GetFilteredProductCountAsync(List<int> conditionIds = null, List<int> categoryIds = null, double? maxPrice = null, string searchTerm = null)
    {
        try
        {
            // Always call the sellerId version with null to avoid ambiguity
            return await Task.Run(() => buyProductsService.GetFilteredProductCount(conditionIds, categoryIds, maxPrice, searchTerm, null));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting filtered product count asynchronously: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Gets the maximum price of all buy products asynchronously
    /// </summary>
    public async Task<double> GetMaxPriceAsync()
    {
        try
        {
            return await buyProductsService.GetMaxPriceAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting max price: {ex.Message}");
            return 0.0; // Return 0 on error
        }
    }

    /// <summary>
    /// Gets filtered products asynchronously based on provided filters including seller filter
    /// </summary>
    public async Task<List<BuyProduct>> GetFilteredProductsWithSellerAsync(int offset, int count, List<int> conditionIds = null, List<int> categoryIds = null, double? maxPrice = null, string searchTerm = null, int? sellerId = null)
    {
        try
        {
            return await Task.Run(() => buyProductsService.GetFilteredProducts(offset, count, conditionIds, categoryIds, maxPrice, searchTerm, sellerId));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting filtered products with seller filter asynchronously: {ex.Message}");
            return new List<BuyProduct>();
        }
    }

    /// <summary>
    /// Gets the count of filtered products asynchronously using direct server count API with seller filter
    /// </summary>
    public async Task<int> GetFilteredProductCountWithSellerAsync(List<int> conditionIds = null, List<int> categoryIds = null, double? maxPrice = null, string searchTerm = null, int? sellerId = null)
    {
        try
        {
            // Use the repository's dedicated count method instead of fetching all products
            return await Task.Run(() => buyProductsService.GetFilteredProductCount(conditionIds, categoryIds, maxPrice, searchTerm, sellerId));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting filtered product count with seller filter asynchronously: {ex.Message}");
            return 0;
        }
    }
}