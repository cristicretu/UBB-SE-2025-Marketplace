using System.Collections.Generic;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.BorrowProductsService;

namespace ViewModelLayer.ViewModel;

public class BorrowProductsViewModel
{
    private readonly IBorrowProductsService borrowProductsService;

    public BorrowProductsViewModel(BorrowProductsService borrowProductsService)
    {
        this.borrowProductsService = borrowProductsService;
    }

    public List<BorrowProduct> GetAllProducts()
    {
        var borrowProducts = new List<BorrowProduct>();
        foreach (var product in borrowProductsService.GetProducts())
        {
            borrowProducts.Add((BorrowProduct)product);
        }
        return borrowProducts;
    }
}