using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Server.DataAccessLayer;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.IRepository;
using System.Net.Http;
using System.Threading.Tasks;

namespace MarketMinds.Repositories.BasketRepository
{
    public class BasketRepository : IBasketRepository
    {
        private readonly ApplicationDbContext context;

        public BasketRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        public Basket GetBasketByUserId(int userId)
        {
            var basketEntity = context.Baskets
                .FirstOrDefault(b => b.BuyerId == userId);

            if (basketEntity == null)
            {
                basketEntity = new Basket
                {
                    BuyerId = userId
                };
                context.Baskets.Add(basketEntity);
                context.SaveChanges();
            }

            basketEntity.Items = GetBasketItems(basketEntity.Id);

            return basketEntity;
        }

        public void RemoveItemByProductId(int basketId, int productId)
        {
            var basketItem = context.BasketItems
                .FirstOrDefault(bi => bi.BasketId == basketId && bi.ProductId == productId);

            if (basketItem != null)
            {
                context.BasketItems.Remove(basketItem);
                context.SaveChanges();
            }
        }

        public List<BasketItem> GetBasketItems(int basketId)
        {
            var basketItems = context.BasketItems
                .Where(bi => bi.BasketId == basketId)
                .ToList();

            foreach (var item in basketItems)
            {
                var product = context.BuyProducts
                    .Include(p => p.Seller)
                    .Include(p => p.Condition)
                    .Include(p => p.Category)
                    .FirstOrDefault(p => p.Id == item.ProductId);

                if (product != null)
                {
                    item.Product = product;

                    var productTagIds = context.Set<BuyProductProductTag>()
                        .Where(pt => pt.ProductId == product.Id)
                        .Select(pt => pt.TagId)
                        .ToList();

                    var tags = context.Set<ProductTag>()
                        .Where(t => productTagIds.Contains(t.Id))
                        .ToList();

                    product.Tags = tags;

                    var productImages = context.Set<BuyProductImage>()
                        .Where(pi => pi.ProductId == product.Id)
                        .ToList();

                    product.NonMappedImages = productImages.Select(pi => new Image(pi.Url)).ToList();
                }
            }

            return basketItems;
        }

        public void AddItemToBasket(int basketId, int productId, int quantity)
        {
            var product = context.BuyProducts.Find(productId);
            if (product == null)
            {
                throw new Exception("Product not found");
            }

            var existingItem = context.BasketItems
                .FirstOrDefault(bi => bi.BasketId == basketId && bi.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                context.BasketItems.Update(existingItem);
            }
            else
            {
                var basketItem = new BasketItem
                {
                    BasketId = basketId,
                    ProductId = productId,
                    Quantity = quantity,
                    Price = product.Price
                };
                context.BasketItems.Add(basketItem);
            }

            context.SaveChanges();
        }

        public void UpdateProductQuantity(int basketId, int productId, int quantity)
        {
            UpdateItemQuantityByProductId(basketId, productId, quantity);
        }

        public void ClearBasket(int basketId)
        {
            var basketItems = context.BasketItems
                .Where(bi => bi.BasketId == basketId);

            context.BasketItems.RemoveRange(basketItems);
            context.SaveChanges();
        }

        public bool RemoveProductFromBasket(int basketId, int productId)
        {
            var basketItem = context.BasketItems
                .FirstOrDefault(bi => bi.BasketId == basketId && bi.ProductId == productId);

            if (basketItem == null)
            {
                return false;
            }

            context.BasketItems.Remove(basketItem);
            context.SaveChanges();

            return true;
        }

        public void UpdateItemQuantityByProductId(int basketId, int productId, int quantity)
        {
            var basketItem = context.BasketItems
                .FirstOrDefault(bi => bi.BasketId == basketId && bi.ProductId == productId);

            if (basketItem == null)
            {
                throw new Exception("Basket item not found");
            }

            if (quantity == 0)
            {
                context.BasketItems.Remove(basketItem);
            }
            else
            {
                basketItem.Quantity = quantity;
                context.BasketItems.Update(basketItem);
            }

            context.SaveChanges();
        }

        // Stub implementations for Raw methods (these won't be called server-side)
        public HttpResponseMessage AddProductToBasketRaw(int userId, int productId, int quantity)
        {
            throw new NotImplementedException("This method is only for client-side use");
        }

        public string GetBasketByUserRaw(int userId)
        {
            throw new NotImplementedException("This method is only for client-side use");
        }

        public HttpResponseMessage RemoveProductFromBasketRaw(int userId, int productId)
        {
            throw new NotImplementedException("This method is only for client-side use");
        }

        public HttpResponseMessage UpdateProductQuantityRaw(int userId, int productId, int quantity)
        {
            throw new NotImplementedException("This method is only for client-side use");
        }

        public HttpResponseMessage ClearBasketRaw(int userId)
        {
            throw new NotImplementedException("This method is only for client-side use");
        }

        public string ValidateBasketBeforeCheckOutRaw(int basketId)
        {
            throw new NotImplementedException("This method is only for client-side use");
        }

        public HttpResponseMessage ApplyPromoCodeRaw(int basketId, string code)
        {
            throw new NotImplementedException("This method is only for client-side use");
        }

        public string GetPromoCodeDiscountRaw(string code)
        {
            throw new NotImplementedException("This method is only for client-side use");
        }

        public string CalculateBasketTotalsRaw(int basketId, string promoCode)
        {
            throw new NotImplementedException("This method is only for client-side use");
        }

        public HttpResponseMessage DecreaseProductQuantityRaw(int userId, int productId)
        {
            throw new NotImplementedException("This method is only for client-side use");
        }

        public HttpResponseMessage IncreaseProductQuantityRaw(int userId, int productId)
        {
            throw new NotImplementedException("This method is only for client-side use");
        }

        public Task<HttpResponseMessage> CheckoutBasketRaw(int userId, int basketId, object requestData)
        {
            throw new NotImplementedException("This method is only for client-side use");
        }
    }
}
