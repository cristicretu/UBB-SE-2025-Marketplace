using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Input;
using System.Diagnostics;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;
using MarketMinds.Shared.Helper;
using MarketMinds.Shared.Services.BuyProductsService;
using MarketMinds.Views;

namespace MarketMinds.ViewModels
{
    public class ShoppingCartViewModel : IShoppingCartViewModel, INotifyPropertyChanged
    {
        private readonly IShoppingCartService shoppingCartService;
        private readonly IBuyProductsService buyProductsService;
        private int buyerId;
        private bool isLoading;
        private bool isCartEmpty = true;
        private double cartTotal;

        public ObservableCollection<CartItemViewModel> CartItems { get; private set; } = new ObservableCollection<CartItemViewModel>();

        public ICommand CheckoutCommand { get; }

        public ShoppingCartViewModel()
        {
            this.shoppingCartService = App.ShoppingCartService ?? throw new InvalidOperationException("ShoppingCartService is not initialized in App");
            this.buyProductsService = App.BuyProductsService ?? throw new InvalidOperationException("BuyProductsService is not initialized in App");
            // buyer id will be set when the view(s) using this viewmodel is/are being loaded
            Debug.WriteLine($"Initialized ShoppingCartViewModel with buyer ID: {buyerId}");

            CheckoutCommand = new RelayCommand(async _ => await CheckoutAsync());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int BuyerId
        {
            get => buyerId;
            set
            {
                if (buyerId != value)
                {
                    buyerId = value;
                }
            }
        }

        public bool IsLoading
        {
            get => isLoading;
            set
            {
                if (isLoading != value)
                {
                    isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsCartEmpty
        {
            get => isCartEmpty;
            set
            {
                if (isCartEmpty != value)
                {
                    isCartEmpty = value;
                    OnPropertyChanged();
                }
            }
        }

        public double CartTotal
        {
            get => cartTotal;
            set
            {
                if (cartTotal != value)
                {
                    cartTotal = value;
                    OnPropertyChanged();
                }
            }
        }

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

        /// <summary>
        /// Get a product with up-to-date stock information
        /// </summary>
        /// <param name="productId">The ID of the product to retrieve</param>
        /// <returns>A BuyProduct with current stock information or null</returns>
        private async Task<BuyProduct> GetProductWithStockAsync(int productId)
        {
            try
            {
                // Try to get the actual BuyProduct with stock information
                var product = await buyProductsService.GetProductByIdAsync(productId);
                if (product is BuyProduct buyProduct)
                {
                    Debug.WriteLine($"Retrieved product {productId} with stock: {buyProduct.Stock}");
                    return buyProduct;
                }

                Debug.WriteLine($"Product {productId} is not a BuyProduct");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting buy product: {ex.Message}");
                return null;
            }
        }

        public async Task LoadCartItemsAsync()
        {
            try
            {
                IsLoading = true;
                Debug.WriteLine($"Loading cart items for buyer ID: {buyerId}");

                try
                {
                    var cartItems = await this.shoppingCartService.GetCartItemsAsync(this.buyerId);
                    Debug.WriteLine($"Retrieved {cartItems?.Count ?? 0} items from database");

                    this.CartItems.Clear();
                    double calculatedTotal = 0;

                    if (cartItems != null && cartItems.Any())
                    {
                        foreach (var item in cartItems)
                        {
                            // Get the actual quantity in cart for this product
                            int quantityInCart = await this.shoppingCartService.GetProductQuantityAsync(this.buyerId, item.Id);

                            // Try to get up-to-date product with stock information
                            BuyProduct buyProduct = await GetProductWithStockAsync(item.Id);

                            // Use the enhanced product if available, otherwise use the original
                            Product productToUse = buyProduct ?? item;

                            // If it's a buy product with no stock, skip it
                            if (buyProduct != null && buyProduct.Stock <= 0)
                            {
                                Debug.WriteLine($"Skipping product {item.Id} ({item.Title}) because it's out of stock");
                                // Optionally remove it from the cart
                                await shoppingCartService.RemoveProductFromCartAsync(buyerId, item.Id);
                                continue;
                            }

                            // If quantity exceeds stock, adjust it
                            if (buyProduct != null && quantityInCart > buyProduct.Stock)
                            {
                                Debug.WriteLine($"Adjusting quantity for {item.Title} from {quantityInCart} to {buyProduct.Stock} due to stock limitations");
                                quantityInCart = buyProduct.Stock;
                                await shoppingCartService.UpdateProductQuantityAsync(buyerId, item.Id, quantityInCart);
                            }

                            Debug.WriteLine($"Adding item to cart: {productToUse.Title} (ID: {productToUse.Id}), " +
                                $"Stock: {(buyProduct != null ? buyProduct.Stock : "unknown")}, Quantity: {quantityInCart}");

                            this.CartItems.Add(new CartItemViewModel(
                                productToUse,
                                quantityInCart,
                                this));

                            calculatedTotal += productToUse.Price * quantityInCart;
                        }

                        IsCartEmpty = this.CartItems.Count == 0;
                        CartTotal = calculatedTotal;
                    }
                    else
                    {
                        Debug.WriteLine("No items found in cart");
                        IsCartEmpty = true;
                        CartTotal = 0;
                    }
                }
                catch (Exception ex) when (ex.Message.Contains("Not Found"))
                {
                    // If "Not Found" error occurs, treat it as an empty cart
                    Debug.WriteLine("Cart not found for this user - treating as empty cart");
                    this.CartItems.Clear();
                    IsCartEmpty = true;
                    CartTotal = 0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading cart items: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                // Set empty state to provide a better user experience
                this.CartItems.Clear();
                IsCartEmpty = true;
                CartTotal = 0;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task AddToCartAsync(Product product, int quantity)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product), "Product cannot be null.");
            }

            try
            {
                IsLoading = true;
                Debug.WriteLine($"Adding product {product.Id} to cart for buyer {buyerId}, quantity: {quantity}");

                // Check if product is BuyProduct and has enough stock
                BuyProduct buyProduct = await GetProductWithStockAsync(product.Id);
                if (buyProduct != null)
                {
                    if (buyProduct.Stock < quantity)
                    {
                        Debug.WriteLine($"Insufficient stock for product {product.Id}. Available: {buyProduct.Stock}, Requested: {quantity}");
                        if (buyProduct.Stock <= 0)
                        {
                            throw new InvalidOperationException($"The product '{buyProduct.Title}' is out of stock.");
                        }
                        else
                        {
                            // Adjust quantity to available stock
                            Debug.WriteLine($"Adjusting quantity to {buyProduct.Stock} based on available stock");
                            quantity = buyProduct.Stock;
                        }
                    }
                }

                await this.shoppingCartService.AddProductToCartAsync(this.buyerId, product.Id, quantity);
                await this.LoadCartItemsAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task RemoveFromCartAsync(Product product)
        {
            try
            {
                IsLoading = true;
                Debug.WriteLine($"Removing product {product.Id} from cart for buyer {buyerId}");

                await this.shoppingCartService.RemoveProductFromCartAsync(this.buyerId, product.Id);

                // Remove item from collection
                var itemToRemove = CartItems.FirstOrDefault(i => i.Product.Id == product.Id);
                if (itemToRemove != null)
                {
                    CartItems.Remove(itemToRemove);
                }

                // Update empty state
                IsCartEmpty = CartItems.Count == 0;

                // Update the total
                await RecalculateCartTotalAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task UpdateQuantityAsync(Product product, int quantity)
        {
            try
            {
                IsLoading = true;
                Debug.WriteLine($"Updating quantity for product {product.Id} to {quantity} for buyer {buyerId}");

                // Check if product is BuyProduct and has enough stock
                BuyProduct buyProduct = await GetProductWithStockAsync(product.Id);
                if (buyProduct != null)
                {
                    if (buyProduct.Stock < quantity)
                    {
                        Debug.WriteLine($"Cannot update quantity: requested {quantity} but only {buyProduct.Stock} available");
                        if (buyProduct.Stock <= 0)
                        {
                            // If no stock left, remove from cart
                            await RemoveFromCartAsync(product);
                            return;
                        }
                        else
                        {
                            // Adjust quantity to available stock
                            Debug.WriteLine($"Adjusting quantity to {buyProduct.Stock} based on available stock");
                            quantity = buyProduct.Stock;
                        }
                    }
                }

                await this.shoppingCartService.UpdateProductQuantityAsync(this.buyerId, product.Id, quantity);

                // Update the quantity in the matching CartItemViewModel
                var cartItem = CartItems.FirstOrDefault(ci => ci.Product.Id == product.Id);
                if (cartItem != null)
                {
                    // Update the quantity in the UI model
                    cartItem.Quantity = quantity;
                }

                // Update cart total immediately
                await RecalculateCartTotalAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating quantity: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Recalculates the cart total based on current cart items
        /// </summary>
        private async Task RecalculateCartTotalAsync()
        {
            double total = 0;

            foreach (var item in CartItems)
            {
                // Get current price from the service to ensure accuracy
                double currentPrice;
                try
                {
                    currentPrice = await shoppingCartService.GetProductPriceAsync(buyerId, item.Product.Id);
                }
                catch (Exception)
                {
                    // If we can't get the price from service, use the local product price
                    currentPrice = item.Product.Price;
                }

                total += currentPrice * item.Quantity;
            }

            CartTotal = total;
            Debug.WriteLine($"Cart total recalculated: {CartTotal}");
        }

        private async Task CheckoutAsync()
        {
            if (CartItems.Count == 0)
            {
                Debug.WriteLine("Cannot checkout with empty cart");
                return;
            }

            Debug.WriteLine("Proceeding to checkout");

            try
            {
                // Ensure we have the most up-to-date cart total
                await RecalculateCartTotalAsync();

                // Get products for checkout with their current quantities
                var productsForCheckout = GetProductsForCheckout();
                double finalCartTotal = CartTotal;

                Debug.WriteLine($"Starting checkout with {productsForCheckout.Count} products, total: {finalCartTotal}");

                // Create a BillingInfo window
                var billingInfoWindow = new BillingInfoWindow();
                var billingInfoPage = new BillingInfo();

                // Set cart items and total BEFORE setting it as content to prevent data loss
                billingInfoPage.SetCartItems(productsForCheckout);
                billingInfoPage.SetCartTotal(finalCartTotal);
                billingInfoPage.SetBuyerId(buyerId);

                // Now set the content and activate the window
                billingInfoWindow.Content = billingInfoPage;
                billingInfoWindow.Activate();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during checkout: {ex.Message}");
            }
        }
    }
}
