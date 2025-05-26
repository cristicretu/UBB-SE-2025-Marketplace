using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using MarketMinds.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MarketMinds.Views
{
    public sealed partial class FinalisePurchase : Page
    {
        /// <summary>
        /// The view model for the FinalisePurchase page
        /// </summary>
        private FinalizePurchaseViewModel viewModel;

        public FinalisePurchase()
        {
            try
            {
                this.InitializeComponent();

                // Get the order history ID from App
                int orderHistoryId = App.LastProcessedOrderId;
                Debug.WriteLine($"FinalisePurchase constructor with order ID: {orderHistoryId}");

                Debug.WriteLine("Using existing FinalizePurchaseViewModel instance");
                // Use existing viewmodel from App
                viewModel = App.FinalizePurchaseViewModel;

                // Force refresh
                Task.Run(async () => await viewModel.InitializeViewModelAsync());

                DataContext = viewModel;

                if (orderHistoryId <= 0)
                {
                    Debug.WriteLine("Warning: No valid order history ID provided");

                    // Show a message dialog async
                    DispatcherQueue.TryEnqueue(async () =>
                    {
                        try
                        {
                            var dialog = new ContentDialog
                            {
                                XamlRoot = this.XamlRoot,
                                Title = "Order Processing",
                                Content = "Thank you for your order! Your confirmation details will be available shortly.",
                                CloseButtonText = "OK"
                            };

                            await dialog.ShowAsync();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error showing dialog: {ex.Message}");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in FinalisePurchase constructor: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the click event for the continue shopping button
        /// </summary>
        private void OnContinueShopping_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                // Process notifications
                viewModel.HandleFinish();

                // Navigate back to marketplace or main window
                // This could open a new window or navigate in the current window, depending on your app's architecture
                // var mainWindow = new Window(); // Create your main window here
                // mainWindow.Activate();

                //// Close current window if needed
                // var currentWindow = WindowHelper.GetWindowForElement(this);
                // currentWindow?.Close();
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"Error handling continue shopping: {ex.Message}");
            }
        }
    }
}
