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
}