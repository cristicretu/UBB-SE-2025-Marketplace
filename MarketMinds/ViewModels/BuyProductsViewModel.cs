using System.Collections.Generic;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.BuyProductsService;

namespace ViewModelLayer.ViewModel;

public class BuyProductsViewModel
{
    private readonly BuyProductsService buyProductsService;

    public BuyProductsViewModel(BuyProductsService buyProductsService)
    {
        this.buyProductsService = buyProductsService;
    }

    public List<BuyProduct> GetAllProducts()
    {
        var buyProducts = new List<BuyProduct>();
        foreach (var product in buyProductsService.GetProducts())
        {
            buyProducts.Add((BuyProduct)product);
        }
        return buyProducts;
    }
}