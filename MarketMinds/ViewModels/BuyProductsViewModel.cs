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
}