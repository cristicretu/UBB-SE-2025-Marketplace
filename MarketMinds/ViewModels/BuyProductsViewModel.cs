using System.Diagnostics;
using System;
using System.Collections.Generic;
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
    /// Gets the count of filtered products
    /// </summary>
    public int GetFilteredProductCount(List<int> conditionIds = null, List<int> categoryIds = null, double? maxPrice = null, string searchTerm = null)
    {
        try
        {
            // For efficiency, get with zero count to just get the total matching count
            var products = buyProductsService.GetFilteredProducts(0, 0, conditionIds, categoryIds, maxPrice, searchTerm);
            return products.Count;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting filtered product count: {ex.Message}");
            return 0;
        }
    }
}