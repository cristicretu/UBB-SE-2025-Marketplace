using System.Collections.Generic;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Models.DTOs;
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

    public BorrowProduct CreateBorrowProduct(CreateBorrowProductDTO createBorrowProductDTO)
    {
        return this.borrowProductsService.CreateProduct(createBorrowProductDTO);
    }

    public Dictionary<string, string[]> ValidateCreateProductDTO(CreateBorrowProductDTO createBorrowProductDTO)
    {
        return this.borrowProductsService.ValidateProductDTO(createBorrowProductDTO);
    }
}