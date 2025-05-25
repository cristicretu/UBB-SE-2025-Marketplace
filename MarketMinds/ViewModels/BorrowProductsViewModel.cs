using System.Collections.Generic;
using System.Threading.Tasks;
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

    public List<BorrowProduct> GetAllProducts()
    {
        var borrowProducts = new List<BorrowProduct>();
        foreach (var product in borrowProductsService.GetProducts())
        {
            borrowProducts.Add((BorrowProduct)product);
        }
        return borrowProducts;
    }

    /// <summary>
    /// Fetch a single BorrowProduct by its ID.
    /// </summary>
    public async Task<BorrowProduct> GetBorrowProductByIdAsync(int id)
    {
        // The service already exposes an async method for this
        return await borrowProductsService.GetBorrowProductByIdAsync(id);
    }
}