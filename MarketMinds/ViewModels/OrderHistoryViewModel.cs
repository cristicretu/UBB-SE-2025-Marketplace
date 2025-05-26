using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;
using MarketMinds.Views;

namespace MarketMinds.ViewModels
{
    /// <summary>
    /// ViewModel for managing order history operations including tracking orders
    /// </summary>
    public class OrderHistoryViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly ITrackedOrderService trackedOrderService;
        private readonly IOrderService orderService;
        private readonly IOrderSummaryService orderSummaryService;
        private readonly IOrderViewModel orderViewModel;        public OrderHistoryViewModel()
        {
            trackedOrderService = App.TrackedOrderService;
            orderService = App.OrderService;
            orderSummaryService = App.OrderSummaryService;
            orderViewModel = new OrderViewModel();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Handles the track order operation for a given order ID
        /// </summary>
        /// <param name="orderID">The ID of the order to track</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task<bool> TrackOrderAsync(int orderID)
        {
            try
            {
                // First, check if there's a tracked order for this OrderID
                var trackedOrder = await trackedOrderService.GetTrackedOrderByOrderIdAsync(orderID);
                
                if (trackedOrder == null)
                {
                    // Create a tracked order if it doesn't exist
                    var order = await orderService.GetOrderByIdAsync(orderID);
                    
                    if (order == null)
                    {
                        return false;
                    }

                    var orderSummary = await orderSummaryService.GetOrderSummaryByIdAsync(order.OrderSummaryID);
                    
                    string deliveryAddress = orderSummary?.Address ?? "No delivery address provided";
                    
                    await trackedOrderService.CreateTrackedOrderForOrderAsync(
                        orderID, 
                        DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
                        deliveryAddress,
                        MarketMinds.Shared.Models.OrderStatus.PROCESSING,
                        "Order received"
                    );
                    
                    trackedOrder = await trackedOrderService.GetTrackedOrderByOrderIdAsync(orderID);
                    if (trackedOrder == null)
                    {
                        return false;
                    }
                }

                // Open the TrackedOrderWindow
                var trackedOrderWindow = new TrackedOrderWindow();
                
                // Determine if the user has control access based on their role
                bool hasControlAccess = App.CurrentUser.UserType == (int)MarketMinds.Shared.Models.UserRole.Seller || 
                                      App.CurrentUser.UserType == (int)MarketMinds.Shared.Models.UserRole.Admin;

                if (hasControlAccess)
                {
                    var trackedOrderControlPage = new TrackedOrderControlPage();
                    trackedOrderControlPage.SetTrackedOrderID(trackedOrder.TrackedOrderID);
                    trackedOrderWindow.Content = trackedOrderControlPage;
                }
                else
                {
                    var trackedOrderBuyerPage = new TrackedOrderBuyerPage();
                    trackedOrderBuyerPage.SetTrackedOrderID(trackedOrder.TrackedOrderID);
                    trackedOrderWindow.Content = trackedOrderBuyerPage;
                }

                trackedOrderWindow.Activate();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
