using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;

namespace MarketMinds.ViewModels
{
    /// <summary>
    /// Represents the view model for finalizing a purchase and displaying order confirmation.
    /// </summary>
    public class FinalizePurchaseViewModel : IFinalizePurchaseViewModel, INotifyPropertyChanged
    {
        private readonly IOrderHistoryService orderHistoryService;
        private readonly IOrderSummaryService orderSummaryService;
        private readonly IOrderService orderService;
        private readonly INotificationViewModel notificationViewModel;

        // Fields
        private int orderHistoryID;
        private double subtotal;
        private double deliveryFee;
        private double warrantyTax;
        private double total;
        private string fullname;
        private string phone;
        private string email;
        private string paymentMethod;
        private string orderStatus;

        // Properties
        public ObservableCollection<ProductViewModel> ProductList { get; set; }
        public List<Order> Orders;

        /// <summary>
        /// Initializes a new instance of the <see cref="FinalizePurchaseViewModel"/> class.
        /// </summary>
        public FinalizePurchaseViewModel()
        {
            orderHistoryService = App.OrderHistoryService;
            orderService = App.OrderService;
            orderSummaryService = App.OrderSummaryService;
            notificationViewModel = App.NotificationViewModel ?? new NotificationViewModel(1);

            ProductList = new ObservableCollection<ProductViewModel>();
            Orders = new List<Order>();

            // Default values
            OrderStatus = "Completed";

            // Use the last processed order history ID from App state if available
            orderHistoryID = App.LastProcessedOrderId;

            if (orderHistoryID > 0)
            {
                // Use Task.Run to avoid blocking the UI thread during initialization
                Task.Run(async () => await InitializeViewModelAsync()).ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        Debug.WriteLine($"Failed to initialize ViewModel: {t.Exception}");
                    }
                });
            }
            else
            {
                Debug.WriteLine("No order history ID available");
            }
        }

        /// <summary>
        /// Asynchronously initializes the view model by loading products and order summary details.
        /// </summary>
        public async Task InitializeViewModelAsync()
        {
            Debug.WriteLine($"Loading data for order history ID: {orderHistoryID}");

            try
            {
                // Get products from order history
                var products = await orderHistoryService.GetProductsFromOrderHistoryAsync(orderHistoryID);

                // Clear and populate product list
                ProductList.Clear();
                if (products != null)
                {
                    foreach (var product in products)
                    {
                        // Create ProductViewModels with quantities
                        ProductList.Add(new ProductViewModel(product, 1));
                    }
                }

                Debug.WriteLine($"Retrieved {ProductList.Count} products");
                OnPropertyChanged(nameof(ProductList));

                // Get order summary info
                var orderSummary = await orderSummaryService.GetOrderSummaryByIdAsync(orderHistoryID);
                if (orderSummary != null)
                {
                    await SetOrderHistoryInfo(orderSummary);
                }
                else
                {
                    Debug.WriteLine("Order summary is null");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing view model: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets the order history information from the order summary.
        /// </summary>
        /// <param name="orderSummary">The order summary object.</param>
        public async Task SetOrderHistoryInfo(OrderSummary orderSummary)
        {
            try
            {
                Orders = await orderService.GetOrdersFromOrderHistoryAsync(orderHistoryID);

                // Financial details
                Subtotal = orderSummary.Subtotal;
                DeliveryFee = orderSummary.DeliveryFee;
                Total = orderSummary.FinalTotal;
                WarrantyTax = orderSummary.WarrantyTax;

                // Customer details
                FullName = orderSummary.FullName;
                Email = orderSummary.Email;
                PhoneNumber = orderSummary.PhoneNumber;

                // Get payment method from first order
                if (Orders != null && Orders.Count > 0)
                {
                    PaymentMethod = Orders[0].PaymentMethod;
                }

                // Set order number for display
                OrderNumber = orderHistoryID;

                Debug.WriteLine("Order history info set successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error setting order history info: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves products from an order history.
        /// </summary>
        /// <param name="orderHistoryID">The order history ID.</param>
        public async Task<List<Product>> GetProductsFromOrderHistoryAsync(int orderHistoryID)
        {
            return await orderHistoryService.GetProductsFromOrderHistoryAsync(orderHistoryID);
        }

        /// <summary>
        /// Handles the finish process by sending notifications and navigating to the home page.
        /// </summary>
        public async void HandleFinish()
        {
            try
            {
                Debug.WriteLine("Handling finish action");

                // Create payment confirmation notifications
                if (Orders != null)
                {
                    foreach (var order in Orders)
                    {
                        await notificationViewModel.AddNotificationAsync(
                            new PaymentConfirmationNotification(
                                UserSession.CurrentUserId ?? 1,
                                DateTime.Now,
                                order.ProductID,
                                order.Id,
                                false));

                        Debug.WriteLine($"Added notification for order {order.Id}, product {order.ProductID}");
                    }
                }

                // Navigate to home page or marketplace (handled in the view)
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error handling finish: {ex.Message}");
            }
        }

        public double Subtotal
        {
            get => subtotal;
            set
            {
                subtotal = value;
                OnPropertyChanged(nameof(Subtotal));
                // Format for display
                SubtotalFormatted = $"{subtotal:N2} €";
                OnPropertyChanged(nameof(SubtotalFormatted));
            }
        }

        public string SubtotalFormatted { get; private set; }

        public double DeliveryFee
        {
            get => deliveryFee;
            set
            {
                deliveryFee = value;
                OnPropertyChanged(nameof(DeliveryFee));
                // Format for display
                DeliveryFeeFormatted = $"{deliveryFee:N2} €";
                OnPropertyChanged(nameof(DeliveryFeeFormatted));
            }
        }

        public double WarrantyTax
        {
            get => warrantyTax;
            set
            {
                warrantyTax = value;
                OnPropertyChanged(nameof(WarrantyTax));
                // Format for display
                WarrantytaxFormatted = $"{warrantyTax:N2} €";
                OnPropertyChanged(nameof(WarrantytaxFormatted));
            }
        }

        public string DeliveryFeeFormatted { get; private set; }

        public string WarrantytaxFormatted { get; private set; }

        public double Total
        {
            get => total;
            set
            {
                total = value;
                OnPropertyChanged(nameof(Total));
                // Format for display
                TotalFormatted = $"{total:N2} €";
                OnPropertyChanged(nameof(TotalFormatted));
            }
        }

        public string TotalFormatted { get; private set; }

        public string FullName
        {
            get => fullname;
            set
            {
                fullname = value;
                OnPropertyChanged(nameof(FullName));
            }
        }

        public string PhoneNumber
        {
            get => phone;
            set
            {
                phone = value;
                OnPropertyChanged(nameof(PhoneNumber));
            }
        }

        public string Email
        {
            get => email;
            set
            {
                email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        public string PaymentMethod
        {
            get => paymentMethod;
            set
            {
                paymentMethod = value;
                OnPropertyChanged(nameof(PaymentMethod));
            }
        }

        public string OrderStatus
        {
            get => orderStatus;
            set
            {
                orderStatus = value;
                OnPropertyChanged(nameof(OrderStatus));
            }
        }

        public int OrderNumber { get; private set; }
        public int OrderHistoryID
        {
            get => orderHistoryID;
            set
            {
                if (orderHistoryID != value)
                {
                    orderHistoryID = value;
                    OnPropertyChanged(nameof(OrderHistoryID));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}