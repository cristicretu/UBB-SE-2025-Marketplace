using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Windows.Input;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;
namespace MarketMinds.ViewModels
{
    public class ShoppingCartViewModel : IShoppingCartViewModel
    {
        private readonly IShoppingCartService shoppingCartService;
        private readonly int buyerId;

        public ObservableCollection<CartItemViewModel> CartItems { get; private set; } = new ObservableCollection<CartItemViewModel>();

        public ShoppingCartViewModel(IShoppingCartService shoppingCartService, int buyerId)
        {
            this.shoppingCartService = shoppingCartService;
            this.buyerId = buyerId;
        }

        /// <summary>
        /// Gets the buyer ID associated with this shopping cart.
        /// </summary>
        public int BuyerId => this.buyerId;

        /// <summary>
        /// Gets the total price of all items in the cart.
        /// </summary>
        /// <returns>The total price of all items.</returns>
        public double GetCartTotal()
        {
            double total = 0;
            foreach (var item in this.CartItems)
            {
                total += item.TotalPrice;
            }

            return total;
        }

        /// <summary>
        /// Gets all the products in the cart with their quantities for checkout.
        /// </summary>
        /// <returns>A list containing products and their quantities.</returns>
        public List<Product> GetProductsForCheckout()
        {
            var products = new List<Product>();
            foreach (var item in this.CartItems)
            {
                products.Add(item.Product);
            }

            return products;
        }

        public async Task LoadCartItemsAsync()
        {
            var cartItemsFromDb = await this.shoppingCartService.GetCartItemsAsync(this.buyerId);

            this.CartItems.Clear();
            foreach (var item in cartItemsFromDb)
            {
                this.CartItems.Add(new CartItemViewModel(
                    item,
                    item.Stock));
            }
        }

        public async Task AddToCartAsync(Product product, int quantity)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product), "Product cannot be null.");
            }

            await this.shoppingCartService.AddProductToCartAsync(this.buyerId, product.Id, quantity);
            await this.LoadCartItemsAsync();
        }

        public async Task RemoveFromCartAsync(Product product)
        {
            await this.shoppingCartService.RemoveProductFromCartAsync(this.buyerId, product.Id);
            await this.LoadCartItemsAsync();
        }

        public async Task UpdateQuantityAsync(Product product, int quantity)
        {
            await this.shoppingCartService.UpdateProductQuantityAsync(this.buyerId, product.Id, quantity);
            await this.LoadCartItemsAsync();
        }

        private async Task DecreaseQuantityAsync(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product), "Product cannot be null.");
            }

            // Find the cart item in the collection
            var cartItem = this.CartItems.FirstOrDefault(item => item.Product.Id == product.Id);
            if (cartItem != null)
            {
                if (cartItem.Quantity > 1)
                {
                    // decreasing the quantity
                    cartItem.Quantity--;

                    // updating the quantity in the database
                    await this.shoppingCartService.UpdateProductQuantityAsync(this.buyerId, product.Id, cartItem.Quantity);
                }
                else
                {
                    // if quantity is 1, remove the item from the cart
                    await this.shoppingCartService.RemoveProductFromCartAsync(this.buyerId, product.Id);
                }

                // Reload the cart items to reflect changes
                await this.LoadCartItemsAsync();
            }
        }
    }
}
