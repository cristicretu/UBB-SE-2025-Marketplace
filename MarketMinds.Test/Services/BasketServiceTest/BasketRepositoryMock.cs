using System.Collections.Generic;
using System.Linq;
using MarketMinds.Shared.IRepository;
using MarketMinds.Shared.Models;

public class BasketRepositoryMock : IBasketRepository
{
    private readonly Dictionary<int, Basket> _baskets = new(); // Key: Basket.Id
    private int _basketItemIdCounter = 1;

    public Basket GetBasketByUserId(int userId)
    {
        var basket = _baskets.Values.FirstOrDefault(b => b.BuyerId == userId);
        if (basket == null)
        {
            basket = new Basket
            {
                Id = userId, // assuming ID == BuyerId for mock
                BuyerId = userId,
                Items = new List<BasketItem>()
            };
            _baskets[basket.Id] = basket;
        }

        return basket;
    }

    public void RemoveItemByProductId(int basketId, int productId)
    {
        if (_baskets.TryGetValue(basketId, out var basket))
        {
            basket.Items.RemoveAll(i => i.ProductId == productId);
        }
    }

    public List<BasketItem> GetBasketItems(int basketId)
    {
        return _baskets.TryGetValue(basketId, out var basket)
            ? new List<BasketItem>(basket.Items)
            : new List<BasketItem>();
    }

    public void AddItemToBasket(int basketId, int productId, int quantity)
    {
        if (!_baskets.TryGetValue(basketId, out var basket)) return;

        var existingItem = basket.Items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            var mockProduct = new BuyProduct
            {
                Id = productId,
                Price = 10.0 // default for testing
            };

            var newItem = new BasketItem(_basketItemIdCounter++, mockProduct, quantity)
            {
                BasketId = basketId
            };

            basket.Items.Add(newItem);
        }
    }

    public void UpdateItemQuantityByProductId(int basketId, int productId, int quantity)
    {
        if (_baskets.TryGetValue(basketId, out var basket))
        {
            var item = basket.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                item.Quantity = quantity;
            }
        }
    }

    public void ClearBasket(int basketId)
    {
        if (_baskets.TryGetValue(basketId, out var basket))
        {
            basket.Items.Clear();
        }
    }
}
