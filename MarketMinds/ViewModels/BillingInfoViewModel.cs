using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;
using MarketMinds.Shared.Services.BorrowProductsService;
using MarketMinds.Shared.Services.Interfaces;
using MarketMinds.Shared.Services.BuyProductsService;
using MarketMinds.Views;
using Microsoft.UI.Xaml;
using MarketMinds.Shared.Helper;
using Microsoft.UI.Xaml.Controls;
using MarketMinds.Shared.Services.UserService;

namespace MarketMinds.ViewModels
{
    /// <summary>
    /// Represents the view model for billing information and order processing.
    /// </summary>
    public class BillingInfoViewModel : INotifyPropertyChanged, IBillingInfoViewModel
    {
        // Services
        private readonly IOrderHistoryService orderHistoryService;
        private readonly IOrderSummaryService orderSummaryService;
        private readonly IOrderService orderService;
        private readonly IProductService productService;
        private readonly IBorrowProductsService borrowProductService;
        private readonly IDummyWalletService dummyWalletService;
        private readonly IBuyProductsService buyProductsService;
        private readonly IShoppingCartService shoppingCartService;
        private readonly IUserService userService;
        private readonly IBuyerService buyerService;

        // Fields
        private int orderHistoryID;
        private bool isWalletEnabled;
        private bool isCashEnabled;
        private bool isCardEnabled;
        private string selectedPaymentMethod;
        private string fullName;
        private string email;
        private string phoneNumber;
        private string address;
        private string zipCode;
        private string additionalInfo;
        private DateTimeOffset startDate;
        private DateTimeOffset endDate;
        private double subtotal;
        private double deliveryFee;
        private double total;
        private double warrantyTax;
        private bool isProcessing;
        private string errorMessage;

        // Properties for cart-related data
        private List<Product> cartItems;
        public ObservableCollection<Product> ProductList { get; set; } = new ObservableCollection<Product>();
        private Dictionary<int, int> cartQuantities = new Dictionary<int, int>();
        private double cartTotal;
        private int buyerId;
        private Dictionary<int, int> productQuantities = new Dictionary<int, int>();

        public ObservableCollection<ProductViewModel> CartItems { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BillingInfoViewModel"/> class.
        /// </summary>
        public BillingInfoViewModel()
        {
            this.orderHistoryService = App.OrderHistoryService;
            this.orderService = App.OrderService;
            this.orderSummaryService = App.OrderSummaryService;
            this.dummyWalletService = App.DummyWalletService;
            this.productService = App.ProductService;
            this.buyProductsService = App.BuyProductsService;
            this.shoppingCartService = App.ShoppingCartService;
            this.userService = App.UserService;
            this.buyerService = App.BuyerService;

            this.CartItems = new ObservableCollection<ProductViewModel>();
            this.ProductList = new ObservableCollection<Product>();
            this.cartItems = new List<Product>();
            this.warrantyTax = 0;

            // Enable payment methods by default
            this.IsWalletEnabled = true;
            this.IsCashEnabled = true;
            this.IsCardEnabled = true;

            // Default values
            this.deliveryFee = 0;
            this.total = 0;
            this.subtotal = 0;
            this.isProcessing = false;
            this.errorMessage = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BillingInfoViewModel"/> class with a specific order history ID.
        /// </summary>
        /// <param name="orderHistoryID">The order history ID to load.</param>
        // public BillingInfoViewModel(int orderHistoryID) : this()
        // {
        //     this.orderHistoryID = orderHistoryID;
        //     _ = this.InitializeViewModelAsync();
        // }

        public int GetQuantityForProduct(int productId)
        {
            if (productQuantities.TryGetValue(productId, out int quantity))
            {
                return quantity;
            }
            return 1; // Default to 1 if not found
        }

        /// <summary>
        /// Sets the cart items for checkout and properly handles quantities.
        /// </summary>
        public async void SetCartItems(List<Product> cartItems)
        {
            Debug.WriteLine($"Setting cart items: {cartItems?.Count ?? 0} products");

            this.cartItems = cartItems ?? new List<Product>();

            // Clear the dictionaries and product list collections
            this.productQuantities.Clear();
            this.cartQuantities.Clear();
            this.ProductList.Clear();
            this.CartItems.Clear();

            // Process each cart item
            foreach (var item in this.cartItems)
            {
                try
                {
                    // Get the quantity from the shopping cart service
                    int currentUser = UserSession.CurrentUserId ?? 1;
                    int quantity;

                    try
                    {
                        // Await the task to get the actual quantity value
                        quantity = await this.shoppingCartService.GetProductQuantityAsync(currentUser, item.Id);
                        Debug.WriteLine($"Retrieved quantity for product {item.Id}: {quantity}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error getting quantity from service: {ex.Message}");
                        quantity = 1;
                    }

                    // Store quantities in dictionaries for reference
                    this.productQuantities[item.Id] = quantity;
                    this.cartQuantities[item.Id] = quantity;

                    // Create a view model wrapper with the product and quantity
                    var productVM = new ProductViewModel(item, quantity);

                    // Add the product to both collections
                    this.CartItems.Add(productVM);
                    this.ProductList.Add(item); // Also add to ProductList for compatibility

                    Debug.WriteLine($"Added product to display: {item.Id} - {item.Title} - ${item.Price} × {quantity}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error getting quantity for product {item.Id}: {ex.Message}");
                    // Use default quantity of 1 if there was an error
                    this.productQuantities[item.Id] = 1;
                    this.cartQuantities[item.Id] = 1;
                    this.CartItems.Add(new ProductViewModel(item, 1));
                    this.ProductList.Add(item);
                }
            }

            // Update UI
            this.OnPropertyChanged(nameof(this.CartItems));
            this.OnPropertyChanged(nameof(this.ProductList));
            this.OnPropertyChanged(nameof(this.CartQuantities));

            // Calculate totals based on the new cart items - but don't add existing cartTotal
            this.cartTotal = 0; // Reset to prevent doubling
            this.CalculateOrderTotal();
        }

        /// <summary>
        /// Calculates the order total based on cart items and their quantities.
        /// </summary>
        public void CalculateOrderTotal()
        {
            Debug.WriteLine("Calculating order total");

            if (this.cartItems == null || this.cartItems.Count == 0)
            {
                Debug.WriteLine("No items in cart, setting totals to 0");
                this.Total = 0;
                this.Subtotal = 0;
                this.DeliveryFee = 0;
                return;
            }

            // Calculate product subtotal taking quantities into account
            double subtotalProducts = 0;
            foreach (var product in this.cartItems)
            {
                // Get quantity from our dictionary
                int quantity = this.GetQuantityForProduct(product.Id);
                subtotalProducts += product.Price * quantity;
                Debug.WriteLine($"Product {product.Id}: {product.Title} - ${product.Price} × {quantity} = ${product.Price * quantity}");
            }

            // Calculate delivery fee based on subtotal
            this.Subtotal = subtotalProducts;

            if (subtotalProducts >= 200)
            {
                Debug.WriteLine("Order total >= $200, no delivery fee");
                this.DeliveryFee = 0;
                this.Total = subtotalProducts;
            }
            else
            {
                Debug.WriteLine("Order total < $200, applying delivery fee");
                this.DeliveryFee = 13.99;
                this.Total = subtotalProducts + this.DeliveryFee;
            }

            // If cart total was explicitly set, use that instead
            // But ONLY if it was explicitly set and is not zero
            if (this.cartTotal > 0)
            {
                Debug.WriteLine($"Using explicitly set cart total: ${this.cartTotal}");
                this.Total = this.cartTotal;
                this.Subtotal = this.cartTotal - this.DeliveryFee - this.WarrantyTax;
            }

            Debug.WriteLine($"Final calculation - Subtotal: ${this.Subtotal}, Delivery: ${this.DeliveryFee}, Total: ${this.Total}");

            // Update UI
            this.OnPropertyChanged(nameof(this.Subtotal));
            this.OnPropertyChanged(nameof(this.DeliveryFee));
            this.OnPropertyChanged(nameof(this.Total));
        }

        /// <summary>
        /// Sets the cart total value.
        /// </summary>
        /// <param name="total">The total price of the cart.</param>
        public void SetCartTotal(double total)
        {
            Debug.WriteLine($"Setting cart total: ${total}");

            this.cartTotal = total;
            this.Total = total; // Direct assignment
            this.Subtotal = total - this.DeliveryFee - this.WarrantyTax;

            Debug.WriteLine($"Updated values - Total: ${this.Total}, Subtotal: ${this.Subtotal}");

            // Update UI
            this.OnPropertyChanged(nameof(this.Total));
            this.OnPropertyChanged(nameof(this.Subtotal));

            // Don't call CalculateOrderTotal() here as it would recalculate and potentially double-count
        }

        /// <summary>
        /// Sets the buyer ID for the order.
        /// </summary>
        /// <param name="buyerId">The buyer ID.</param>
        public void SetBuyerId(int buyerId)
        {
            Debug.WriteLine($"Setting buyer ID: {buyerId}");
            this.buyerId = buyerId;
        }

        /// <summary>
        /// Initializes the view model by loading data from various sources.
        /// </summary>
        public async Task InitializeViewModelAsync()
        {
            Debug.WriteLine("Initializing BillingInfoViewModel");

            try
            {
                // If we already have cart items, don't override with order history
                if (this.ProductList != null && this.ProductList.Count > 0)
                {
                    Debug.WriteLine("Products already loaded - skipping order history load");
                    return;
                }

                // Only try to get from order history if no cart items were passed and the ID is valid
                if (this.orderHistoryID > 0)
                {
                    Debug.WriteLine($"Loading products from order history ID: {this.orderHistoryID}");

                    try
                    {
                        var products = await this.orderHistoryService.GetProductsFromOrderHistoryAsync(this.orderHistoryID);
                        if (products != null && products.Count > 0)
                        {
                            this.cartItems = products;
                            this.ProductList = new ObservableCollection<Product>(products);
                            Debug.WriteLine($"Loaded {products.Count} products from order history");
                            this.OnPropertyChanged(nameof(this.ProductList));
                        }
                        else
                        {
                            Debug.WriteLine("No products found in order history");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error loading from order history: {ex.Message}");
                        this.ErrorMessage = $"Error loading order details: {ex.Message}";
                    }
                }

                this.CalculateOrderTotal();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in InitializeViewModelAsync: {ex.Message}");
                this.ErrorMessage = $"Initialization error: {ex.Message}";
            }
        }

        /// <summary>
        /// Legacy method for backwards compatibility.
        /// </summary>
        public void CalculateOrderTotal(int orderHistoryID)
        {
            this.CalculateOrderTotal();
        }

        /// <summary>
        /// Autofills billing information with the current user's data.
        /// </summary>
        public async Task AutofillUserInformationAsync()
        {
            try
            {
                // Get the current user ID
                int userId = UserSession.CurrentUserId ?? 1;

                // Check if we have a valid user ID
                if (userId <= 0)
                {
                    Debug.WriteLine("Invalid user ID for autofill");
                    return;
                }

                if (this.userService != null && this.buyerService != null)
                {
                    User? user = await this.userService.GetUserByIdAsync(userId);
                    Buyer? buyer = await this.buyerService.GetBuyerByUser(App.CurrentUser);

                    if (user != null && buyer != null)
                    {
                        // Fill in the user information fields
                        this.FullName = buyer.FirstName + " " + buyer.LastName;
                        this.Email = user.Email ?? this.Email;
                        this.PhoneNumber = buyer.PhoneNumber;
                        this.Address = buyer.BillingAddress.StreetLine;
                        this.ZipCode = buyer.BillingAddress.PostalCode;

                        Debug.WriteLine($"Autofilled user information for user ID: {userId}");
                    }
                    else
                    {
                        Debug.WriteLine($"Could not find user with ID: {userId}");
                    }
                }
                else
                {
                    Debug.WriteLine("User service not available for autofill");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error autofilling user information: {ex.Message}");
            }
        }

        public async Task ClearUserInformationExceptEmailAsync()
        {
            try
            {
                // Clear all fields except email
                this.FullName = string.Empty;
                this.PhoneNumber = string.Empty;
                this.Address = string.Empty;
                this.ZipCode = string.Empty;
                this.AdditionalInfo = string.Empty;
                this.SelectedPaymentMethod = string.Empty;
                Debug.WriteLine("Cleared user information except email");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error clearing user information: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates all input fields in the billing information.
        /// </summary>
        /// <returns>A list of validation error messages. Empty if all fields are valid.</returns>
        public List<string> ValidateFields()
        {
            var errors = new List<string>();

            // Full Name validation
            if (string.IsNullOrWhiteSpace(this.FullName))
            {
                errors.Add("Full name is required.");
            }

            // Email validation
            if (string.IsNullOrWhiteSpace(this.Email))
            {
                errors.Add("Email address is required.");
            }
            else if (!this.Email.Contains("@") || !this.Email.Contains("."))
            {
                errors.Add("Please enter a valid email address.");
            }

            // Phone Number validation - should be in format +40XXXXXXXXX
            if (string.IsNullOrWhiteSpace(this.PhoneNumber))
            {
                errors.Add("Phone number is required.");
            }
            else if (!this.PhoneNumber.StartsWith("+40") || this.PhoneNumber.Length != 12)
            {
                errors.Add("Phone number must be in format +40XXXXXXXXX.");
            }

            // Address validation
            if (string.IsNullOrWhiteSpace(this.Address))
            {
                errors.Add("Address is required.");
            }

            // Zip Code validation - should be 6 digits
            if (string.IsNullOrWhiteSpace(this.ZipCode))
            {
                errors.Add("Zip code is required.");
            }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(this.ZipCode, @"^\d{6}$"))
            {
                errors.Add("Zip code must be a 6-digit number.");
            }

            // Payment Method validation
            if (string.IsNullOrWhiteSpace(this.SelectedPaymentMethod))
            {
                errors.Add("Please select a payment method.");
            }

            return errors;
        }

        /// <summary>
        /// Handles the finalization of an order.
        /// </summary>
        public async Task OnFinalizeButtonClickedAsync()
        {
            if (this.IsProcessing)
            {
                Debug.WriteLine("Already processing order, ignoring request");
                throw new InvalidOperationException("Already processing order, ignoring request");
            }

            try
            {
                this.IsProcessing = true;
                this.ErrorMessage = string.Empty;
                Debug.WriteLine("Processing order finalization");

                // Ensure we have a payment method selected
                if (string.IsNullOrEmpty(this.SelectedPaymentMethod))
                {
                    this.SelectedPaymentMethod = this.IsCardEnabled ? "card" :
                                              (this.IsCashEnabled ? "cash" : "wallet");
                    Debug.WriteLine($"No payment method selected, defaulting to: {this.SelectedPaymentMethod}");
                }

                // Create order summary
                await this.CreateOrderSummaryAsync();

                // Process payment
                if (this.SelectedPaymentMethod == "wallet")
                {
                    await this.ProcessWalletRefillAsync();
                }

                // Create order entries
                await this.CreateOrderEntriesAsync();

                // Open next screen based on payment method
                await this.OpenNextWindowAsync(this.SelectedPaymentMethod);

                Debug.WriteLine("Order finalization complete");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnFinalizeButtonClickedAsync: {ex.Message}");
                this.ErrorMessage = $"Error finalizing order: {ex.Message}";
                throw; // Rethrow so the UI can show an error dialog
            }
            finally
            {
                this.IsProcessing = false;
            }
        }

        /// <summary>
        /// Creates a new order history record.
        /// </summary>
        private async Task<int> CreateOrderHistoryAsync()
        {
            try
            {
                int userId = this.buyerId > 0 ? this.buyerId : (UserSession.CurrentUserId ?? 1);
                Debug.WriteLine($"Creating order history for user {userId}");

                int newOrderHistoryId = await this.orderHistoryService.CreateOrderHistoryAsync(userId);
                Debug.WriteLine($"Created order history with ID: {newOrderHistoryId}");

                return newOrderHistoryId;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating order history: {ex.Message}");
                throw new InvalidOperationException($"Failed to create order history: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Creates or updates the order summary.
        /// </summary>
        private async Task CreateOrderSummaryAsync()
        {
            try
            {
                Debug.WriteLine($"Creating or updating order summary for order history ID: {this.orderHistoryID}");

                // Use null-safe default values for required fields
                string safeName = this.FullName ?? "Guest User";
                string safeEmail = this.Email ?? "guest@example.com";
                string safePhone = this.PhoneNumber ?? "000-000-0000";
                string safeAddress = this.Address ?? "No Address Provided";
                string safeZipCode = this.ZipCode ?? "00000";
                string additionalInfo = this.AdditionalInfo ?? string.Empty;

                // Fix for ContractDetails error - provide an empty string instead of null
                string contractDetails = string.Empty; // TODO: Add contract details

                // Create a new OrderSummary object
                var orderSummary = new OrderSummary
                {
                    // ID = this.orderHistoryID,
                    Subtotal = this.Subtotal,
                    WarrantyTax = this.WarrantyTax,
                    DeliveryFee = this.DeliveryFee,
                    FinalTotal = this.Total,
                    FullName = safeName,
                    Email = safeEmail,
                    PhoneNumber = safePhone,
                    Address = safeAddress,
                    PostalCode = safeZipCode,
                    AdditionalInfo = additionalInfo,
                    ContractDetails = contractDetails
                };

                try
                {
                    Debug.WriteLine("Creating new order summary");
                    int newOrderSummaryId = await this.orderSummaryService.CreateOrderSummaryAsync(orderSummary);

                    // After successful creation, assign the generated ID
                    this.orderHistoryID = newOrderSummaryId;
                    this.OrderHistoryId = newOrderSummaryId; // update the property for the rest of the flow

                    Debug.WriteLine($"Successfully created order summary with ID: {newOrderSummaryId}");
                }
                catch (Exception createEx)
                {
                    Debug.WriteLine($"Error creating order summary: {createEx.Message}");
                    throw new Exception($"Failed to create order summary: {createEx.Message}", createEx);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in CreateOrderSummaryAsync: {ex.Message}");
                throw; // Rethrow to handle in calling method
            }
        }

        /// <summary>
        /// Creates order entries for each product in the cart.
        /// </summary>
        private async Task CreateOrderEntriesAsync()
        {
            if (this.cartItems == null || this.cartItems.Count == 0)
            {
                Debug.WriteLine("No cart items to process");
                return;
            }

            Debug.WriteLine($"Creating order entries for {this.cartItems.Count} products");

            foreach (var item in this.cartItems)
            {
                try
                {
                    // Get quantity from our dictionaries instead of blindly using Stock
                    int quantity = this.GetQuantityForProduct(item.Id);
                    Debug.WriteLine($"Processing product {item.Id} ({item.Title}), quantity: {quantity}");

                    // Create one order entry per item ordered
                    for (int i = 0; i < quantity; i++)
                    {
                        try
                        {
                            // Based on the web app code, the server expects "new", "used", or "borrowed"
                            // NOT "buy", "borrow", etc.
                            string productType;

                            if (item is BorrowProduct)
                            {
                                productType = "borrowed";
                            }
                            else if (item is AuctionProduct)
                            {
                                productType = "auction"; // This might need to change depending on your backend
                            }
                            else // Default to "new" for BuyProducts
                            {
                                productType = "new";
                            }

                            Debug.WriteLine($"Using product type: {productType} for product {item.Id}");

                            await this.orderService.AddOrderAsync(
                                item.Id,
                                this.buyerId,
                                productType,
                                this.SelectedPaymentMethod,
                                this.orderHistoryID,
                                DateTime.Now);

                            Debug.WriteLine($"Created order entry for product {item.Id}, iteration {i + 1}/{quantity}");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error adding order entry: {ex.Message}");
                        }
                    }

                    // Update stock for BuyProducts correctly - DECREASE BY quantity
                    if (item is BuyProduct buyProd)
                    {
                        try
                        {
                            // Get the current stock first
                            var product = await this.buyProductsService.GetProductByIdAsync(item.Id);
                            if (product != null && product is BuyProduct buyProduct)
                            {
                                int currentStock = buyProduct.Stock;
                                // Calculate new stock as current - quantity instead of setting to quantity
                                int newStock = Math.Max(0, currentStock - quantity);

                                // Use DecreaseProductStockAsync if available since that's what the web app uses
                                if (this.buyProductsService is IBuyProductsService service &&
                                    typeof(IBuyProductsService).GetMethod("DecreaseProductStockAsync") != null)
                                {
                                    await service.DecreaseProductStockAsync(item.Id, quantity);
                                    Debug.WriteLine($"Decreased stock for product {item.Id} by {quantity} (from {currentStock})");
                                }
                                else
                                {
                                    // Fallback to UpdateProductStockAsync
                                    await this.buyProductsService.UpdateProductStockAsync(item.Id, newStock);
                                    Debug.WriteLine($"Updated stock for product {item.Id} from {currentStock} to {newStock}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error updating stock: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error processing product {item.Id}: {ex.Message}");
                }
            }

            // After successful order creation, clear the shopping cart
            await ClearShoppingCartAsync();
        }

        /// <summary>
        /// Clears the shopping cart after successful order creation
        /// </summary>
        private async Task ClearShoppingCartAsync()
        {
            try
            {
                int userId = this.buyerId > 0 ? this.buyerId : (UserSession.CurrentUserId ?? 1);
                Debug.WriteLine($"Clearing shopping cart for user {userId}");
                await this.shoppingCartService.ClearCartAsync(userId);
                Debug.WriteLine("Shopping cart cleared successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error clearing shopping cart: {ex.Message}");
            }
        }

        /// <summary>
        /// Processes a wallet payment.
        /// </summary>
        public async Task ProcessWalletRefillAsync()
        {
            try
            {
                int userId = App.CurrentUser.Id;
                Debug.WriteLine($"Processing wallet payment for user {userId}, amount: ${this.Total}");

                double walletBalance = await this.dummyWalletService.GetWalletBalanceAsync(userId);
                Debug.WriteLine($"Current wallet balance: ${walletBalance}");

                if (walletBalance < this.Total)
                {
                    Debug.WriteLine("Insufficient funds in wallet");
                    throw new InvalidOperationException($"Insufficient funds in wallet. Available: ${walletBalance}, Required: ${this.Total}");
                }

                double newBalance = walletBalance - this.Total;
                await this.dummyWalletService.UpdateWalletBalance(userId, newBalance);
                Debug.WriteLine($"Wallet payment successful. New balance: ${newBalance}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing wallet payment: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Opens the next screen based on payment method.
        /// </summary>
        public async Task OpenNextWindowAsync(string selectedPaymentMethod)
        {
            // OBS, does not actually open the window, just sets the data for the next page
            try
            {
                // Save the order history ID for access by the finalize purchase page
                App.LastProcessedOrderId = this.orderHistoryID; // this is actually the order summary ID
                Debug.WriteLine($"Setting App.LastProcessedOrderId = {this.orderHistoryID}");

                // Create a new FinalizePurchaseViewModel if it doesn't exist
                if (App.FinalizePurchaseViewModel == null)
                {
                    Debug.WriteLine("Creating new FinalizePurchaseViewModel instance");
                }
                else
                {
                    Debug.WriteLine("Using existing FinalizePurchaseViewModel instance");
                }

                // Initialize it with our data for faster loading
                var viewModel = App.FinalizePurchaseViewModel;
                viewModel.FullName = this.FullName;
                viewModel.Email = this.Email;
                viewModel.PhoneNumber = this.PhoneNumber;
                viewModel.PaymentMethod = this.SelectedPaymentMethod;
                viewModel.OrderStatus = "Completed";
                viewModel.Subtotal = this.Subtotal;
                viewModel.DeliveryFee = this.DeliveryFee;
                viewModel.WarrantyTax = this.WarrantyTax;
                viewModel.Total = this.Total;
                viewModel.OrderHistoryID = this.orderHistoryID;

                // Clear previous products
                viewModel.ProductList.Clear();

                // Set products if we have them
                if (this.cartItems?.Count > 0)
                {
                    foreach (var product in this.cartItems)
                    {
                        int qty = this.GetQuantityForProduct(product.Id);
                        viewModel.ProductList.Add(new ProductViewModel(product, qty));
                    }
                    Debug.WriteLine($"Added {viewModel.ProductList.Count} products to FinalizePurchaseViewModel");
                }

                Debug.WriteLine($"Opening finalize purchase window for {selectedPaymentMethod} payment");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening next window: {ex.Message}");
                this.ShowBasicSuccessMessage("Thank you for your order!");
            }
        }

        /// <summary>
        /// Shows a basic success message.
        /// </summary>
        private void ShowBasicSuccessMessage(string message)
        {
            Debug.WriteLine($"SUCCESS: {message}");
        }

        /// <summary>
        /// Retrieves products from an order history.
        /// </summary>
        public async Task<List<Product>> GetProductsFromOrderHistoryAsync(int orderHistoryID)
        {
            return await this.orderHistoryService.GetProductsFromOrderHistoryAsync(orderHistoryID);
        }

        /// <summary>
        /// Applies borrowed tax to a product.
        /// </summary>
        public async Task ApplyBorrowedTax(BorrowProduct borrowProduct)
        {
            if (borrowProduct == null)
            {
                return;
            }

            if (this.StartDate > this.EndDate)
            {
                Debug.WriteLine("Start date is after end date, cannot apply borrowed tax");
                return;
            }

            int monthsBorrowed = ((this.EndDate.Year - this.StartDate.Year) * 12) +
                               this.EndDate.Month - this.StartDate.Month;
            if (monthsBorrowed <= 0)
            {
                monthsBorrowed = 1;
            }

            Debug.WriteLine($"Calculating borrowed tax for {monthsBorrowed} months");

            double warrantyTaxRate = 0.2; // 20% warranty tax
            double finalPrice = borrowProduct.Price * monthsBorrowed;
            double calculatedWarrantyTax = finalPrice * warrantyTaxRate;

            this.warrantyTax += calculatedWarrantyTax;
            this.WarrantyTax = this.warrantyTax;

            borrowProduct.Price = finalPrice + calculatedWarrantyTax;

            Debug.WriteLine($"Applied tax - Price: ${borrowProduct.Price}, Warranty Tax: ${this.WarrantyTax}");

            this.CalculateOrderTotal();

            // Update borrow product dates
            DateTime newStartDate = this.startDate.Date;
            DateTime newEndDate = this.endDate.Date;
            borrowProduct.StartDate = newStartDate;
            borrowProduct.EndDate = newEndDate;

            await this.borrowProductService.UpdateBorrowProductAsync(borrowProduct);
            Debug.WriteLine("Borrow product updated with new dates and price");
        }

        public async Task AddPurchase(double purchaseAmount)
        {
            var currentUser = App.CurrentUser;
            if (currentUser?.Id > 0)
            {
                await this.buyerService.UpdateAfterPurchaseById(currentUser.Id, purchaseAmount);
            }
        }

        /// <summary>
        /// Updates the start date for a borrowed product.
        /// </summary>
        public void UpdateStartDate(DateTimeOffset date)
        {
            this.startDate = date.DateTime;
            this.StartDate = date;
            Debug.WriteLine($"Updated start date: {date.Date}");
        }

        /// <summary>
        /// Updates the end date for a borrowed product.
        /// </summary>
        public void UpdateEndDate(DateTimeOffset date)
        {
            this.endDate = date.DateTime;
            this.EndDate = date;
            Debug.WriteLine($"Updated end date: {date.Date}");
        }

        public string SelectedPaymentMethod
        {
            get => this.selectedPaymentMethod;
            set
            {
                if (this.selectedPaymentMethod != value)
                {
                    this.selectedPaymentMethod = value;
                    this.OnPropertyChanged(nameof(this.SelectedPaymentMethod));
                    Debug.WriteLine($"Selected payment method: {value}");
                }
            }
        }

        public string FullName
        {
            get => this.fullName;
            set
            {
                if (this.fullName != value)
                {
                    this.fullName = value;
                    this.OnPropertyChanged(nameof(this.FullName));
                }
            }
        }

        public string Email
        {
            get => this.email;
            set
            {
                if (this.email != value)
                {
                    this.email = value;
                    this.OnPropertyChanged(nameof(this.Email));
                }
            }
        }

        public string PhoneNumber
        {
            get => this.phoneNumber;
            set
            {
                if (this.phoneNumber != value)
                {
                    this.phoneNumber = value;
                    this.OnPropertyChanged(nameof(this.PhoneNumber));
                }
            }
        }

        public string Address
        {
            get => this.address;
            set
            {
                if (this.address != value)
                {
                    this.address = value;
                    this.OnPropertyChanged(nameof(this.Address));
                }
            }
        }

        public string ZipCode
        {
            get => this.zipCode;
            set
            {
                if (this.zipCode != value)
                {
                    this.zipCode = value;
                    this.OnPropertyChanged(nameof(this.ZipCode));
                }
            }
        }

        public string AdditionalInfo
        {
            get => this.additionalInfo;
            set
            {
                if (this.additionalInfo != value)
                {
                    this.additionalInfo = value;
                    this.OnPropertyChanged(nameof(this.AdditionalInfo));
                }
            }
        }

        /// <summary>
        /// Gets the dictionary of product quantities in the cart.
        /// </summary>
        public Dictionary<int, int> CartQuantities
        {
            get => this.cartQuantities;
        }

        public bool IsWalletEnabled
        {
            get => this.isWalletEnabled;
            set
            {
                if (this.isWalletEnabled != value)
                {
                    this.isWalletEnabled = value;
                    this.OnPropertyChanged(nameof(this.IsWalletEnabled));
                }
            }
        }

        public bool IsCashEnabled
        {
            get => this.isCashEnabled;
            set
            {
                if (this.isCashEnabled != value)
                {
                    this.isCashEnabled = value;
                    this.OnPropertyChanged(nameof(this.IsCashEnabled));
                }
            }
        }

        public bool IsCardEnabled
        {
            get => this.isCardEnabled;
            set
            {
                if (this.isCardEnabled != value)
                {
                    this.isCardEnabled = value;
                    this.OnPropertyChanged(nameof(this.IsCardEnabled));
                }
            }
        }

        public double Subtotal
        {
            get => this.subtotal;
            set
            {
                if (this.subtotal != value)
                {
                    this.subtotal = value;
                    this.OnPropertyChanged(nameof(this.Subtotal));
                }
            }
        }

        public double DeliveryFee
        {
            get => this.deliveryFee;
            set
            {
                if (this.deliveryFee != value)
                {
                    this.deliveryFee = value;
                    this.OnPropertyChanged(nameof(this.DeliveryFee));
                }
            }
        }

        public double Total
        {
            get => this.total;
            set
            {
                if (this.total != value)
                {
                    this.total = value;
                    this.OnPropertyChanged(nameof(this.Total));
                }
            }
        }

        public double WarrantyTax
        {
            get => this.warrantyTax;
            set
            {
                if (this.warrantyTax != value)
                {
                    this.warrantyTax = value;
                    this.OnPropertyChanged(nameof(this.WarrantyTax));
                }
            }
        }

        public DateTimeOffset StartDate
        {
            get => this.startDate;
            set
            {
                if (this.startDate != value)
                {
                    this.startDate = value;
                    this.OnPropertyChanged(nameof(this.StartDate));
                }
            }
        }

        public DateTimeOffset EndDate
        {
            get => this.endDate;
            set
            {
                if (this.endDate != value)
                {
                    this.endDate = value;
                    this.OnPropertyChanged(nameof(this.EndDate));
                }
            }
        }

        public bool IsProcessing
        {
            get => this.isProcessing;
            set
            {
                if (this.isProcessing != value)
                {
                    this.isProcessing = value;
                    this.OnPropertyChanged(nameof(this.IsProcessing));
                }
            }
        }

        public string ErrorMessage
        {
            get => this.errorMessage;
            set
            {
                if (this.errorMessage != value)
                {
                    this.errorMessage = value;
                    this.OnPropertyChanged(nameof(this.ErrorMessage));
                }
            }
        }

        public int OrderHistoryId
        {
            get => this.orderHistoryID;
            set
            {
                if (this.orderHistoryID != value)
                {
                    this.orderHistoryID = value;
                    this.OnPropertyChanged(nameof(this.OrderHistoryId));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}