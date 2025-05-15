// -----------------------------------------------------------------------
// <copyright file="ShoppingCartRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using SharedClassLibrary.Domain;

namespace SharedClassLibrary.IRepository
{
    public interface IShoppingCartRepository
    {
        Task AddProductToCartAsync(int buyerId, int productId, int quantity);
        Task ClearCartAsync(int buyerId);
        Task<int> GetCartItemCountAsync(int buyerId);
        Task<List<Product>> GetCartItemsAsync(int buyerId);
        Task<int> GetProductQuantityAsync(int buyerId, int productId);
        Task<bool> IsProductInCartAsync(int buyerId, int productId);
        Task RemoveProductFromCartAsync(int buyerId, int productId);
        Task UpdateProductQuantityAsync(int buyerId, int productId, int quantity);
    }
}