using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.ViewModels;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;

// Add this using directive for Configuration
namespace MarketMinds.Views
{
    [ExcludeFromCodeCoverage]
    public sealed partial class OrderHistoryView : Page
    {        

        /// <summary>
        /// The order history view page;
        /// </summary>
        private readonly int userId;
        private IOrderViewModel orderViewModel;
        private IContractViewModel contractViewModel;
        private OrderHistoryViewModel orderHistoryViewModel;
        private Dictionary<int, string> orderProductCategoryTypes = new Dictionary<int, string>();        
        
        /// <summary>
        /// Initializes a new instance of the OrderHistoryUI page.
        /// </summary>
        /// <param name="userId">The ID of the user whose order history to display. Must be a positive integer.</param>
        /// <exception cref="ArgumentException">Thrown when userId is less than or equal to zero.</exception>
        public OrderHistoryView()
        {
            InitializeComponent();
            this.userId = App.CurrentUser.Id;
            orderViewModel = new OrderViewModel();
            contractViewModel = App.ContractViewModel;
            orderHistoryViewModel = new OrderHistoryViewModel();

            this.Loaded += Page_Loaded;
        }

        /// <summary>
        /// Event handler triggered when the page is loaded. Loads initial order data.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">Event data.</param>
        private async void Page_Loaded(object sender, RoutedEventArgs args)
        {
            this.Loaded -= Page_Loaded;
            await LoadOrders(SearchTextBox.Text);
        }

        /// <summary>
        /// Loads orders for the current user based on search text and selected time period filter.
        /// </summary>
        /// <param name="searchText">Optional search text to filter orders by product name. Can be null.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when there is an error retrieving or displaying order data.</exception>
        private async Task LoadOrders(string searchText = null)
        {
            try
            {
                var selectedPeriod = (TimePeriodComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

                if (selectedPeriod == null)
                {
                    return;
                }

                // Use the view model to get orders with product info
                var orderDisplayInfos = await orderViewModel.GetOrdersWithProductInfoAsync(userId, searchText, selectedPeriod);

                // Extract the product category types for use in showing contract details
                foreach (var orderInfo in orderDisplayInfos)
                {
                    orderProductCategoryTypes[orderInfo.OrderSummaryID] = orderInfo.ProductCategory;
                }

                DispatcherQueue.TryEnqueue(() =>
                {
                    if (orderDisplayInfos == null)
                    {
                        throw new NullReferenceException("orderDisplayInfos is null");
                    }

                    if (OrdersListView == null)
                    {
                        throw new NullReferenceException("OrdersListView is null");
                    }

                    if (NoResultsText == null)
                    {
                        throw new NullReferenceException("NoResultsText is null");
                    }
                    if (orderDisplayInfos.Count > 0)
                    {
                        // Group the orders by OrderSummaryID and create OrderGroup objects
                        var groupedOrders = orderDisplayInfos
                            .GroupBy(order => order.OrderSummaryID)
                            .Select(group => new OrderGroup
                            {
                                Name = $"Order #{group.Key}",
                                Items = group.Cast<dynamic>().ToList()
                            })
                            .ToList();

                        // Bind the grouped data to the ListView
                        OrdersListView.ItemsSource = groupedOrders;
                        OrdersListView.Visibility = Visibility.Visible;
                        NoResultsText.Visibility = Visibility.Collapsed;
                    }                    
                    else
                    {
                        // Clear the ListView when no results
                        OrdersListView.ItemsSource = null;
                        OrdersListView.Visibility = Visibility.Collapsed;
                        NoResultsText.Visibility = Visibility.Visible;

                        NoResultsText.Text = string.IsNullOrEmpty(searchText) ?
                            "No orders found" :
                            $"No orders found containing '{searchText}'";

                        if (selectedPeriod != "All Orders")
                        {
                            NoResultsText.Text += $" in {selectedPeriod}";
                        }
                    }
                });
            }            
            catch (Exception exception)
            {
                DispatcherQueue.TryEnqueue(async () =>
                {
                    // Clear the ListView on error
                    OrdersListView.ItemsSource = null;
                    OrdersListView.Visibility = Visibility.Collapsed;
                    NoResultsText.Visibility = Visibility.Visible;
                    NoResultsText.Text = "Error loading orders";

                    var errorContentDialog = new ContentDialog
                    {
                        Title = "Error",
                        Content = $"Failed to load orders: {exception.Message}",
                        CloseButtonText = "OK",
                        XamlRoot = this.Content.XamlRoot
                    };
                    await errorContentDialog.ShowAsync();
                });
            }
        }

        /// <summary>
        /// Event handler for the refresh button click. Reloads the order list.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadOrders(SearchTextBox.Text);
        }

        /// <summary>
        /// Event handler for toggling the expand/collapse state of an order group.
        /// </summary>
        /// <param name="sender">The source of the event (Border with DataContext=OrderGroup).</param>
        /// <param name="e">Event data.</param>
        private void ToggleOrderGroup_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is OrderGroup orderGroup)
            {
                orderGroup.IsExpanded = !orderGroup.IsExpanded;
            }
        }

        /// <summary>
        /// Event handler for the order details button click. Shows detailed information for a selected order.
        /// </summary>
        /// <param name="sender">The source of the event (Button with Tag=orderSummaryId).</param>
        /// <param name="e">Event data.</param>
        /// <exception cref="Exception">Thrown when there is an error retrieving or displaying order details.</exception>
        private async void OrderDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int orderSummaryId)
            {
                Button clickedButton = button;

                clickedButton.Content = "Loading...";
                clickedButton.IsEnabled = false;

                try
                {
                    var orderSummary = await orderViewModel.GetOrderSummaryAsync(orderSummaryId);
                    if (orderSummary == null)
                    {
                        await ShowCustomMessageAsync("Error", "Order summary not found.");
                        return;
                    }

                    bool isTaskEnqueued = DispatcherQueue.TryEnqueue(() => ShowOrderDetailsDialog(orderSummary, orderSummaryId));

                    if (!isTaskEnqueued)
                    {
                        await ShowCustomMessageAsync("Error", "Failed to display order details.");
                    }
                }
                catch (Exception exception)
                {
                    await ShowCustomMessageAsync("Error Details",
                        $"Type: {exception.GetType().Name}\nMessage: {exception.Message}");
                }
                finally
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        if (clickedButton != null)
                        {
                            clickedButton.Content = "View Details";
                            clickedButton.IsEnabled = true;
                        }
                    });
                }
            }
        }

        private OrderSummary currentOrderSummary;

        /// <summary>
        /// Displays a detailed dialog for a specific order summary using the XAML-defined dialog.
        /// </summary>
        /// <param name="orderSummary">The order summary object containing details to display. Must not be null.</param>
        /// <param name="orderSummaryId">The ID of the order summary. Must be a positive integer.</param>
        /// <exception cref="ArgumentNullException">Thrown when orderSummary is null.</exception>
        /// <exception cref="Exception">Thrown when there is an error setting up the dialog.</exception>
        private void ShowOrderDetailsDialog(OrderSummary orderSummary, int orderSummaryId)
        {
            try
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    try
                    {
                        if (Content?.XamlRoot == null)
                        {
                            System.Diagnostics.Debug.WriteLine("Error: XamlRoot is null - cannot show dialog");
                            return;
                        }

                        // Store the current order summary for the contract generation
                        currentOrderSummary = orderSummary;

                        // Clear previous content
                        OrderDetailsContent.Children.Clear();

                        // Populate order details
                        AddDetailRowToPanel(OrderDetailsContent, "Order Summary ID:", orderSummary.ID.ToString());
                        AddDetailRowToPanel(OrderDetailsContent, "Subtotal:", orderSummary.Subtotal.ToString("C"));
                        AddDetailRowToPanel(OrderDetailsContent, "Delivery Fee:", orderSummary.DeliveryFee.ToString("C"));
                        AddDetailRowToPanel(OrderDetailsContent, "Final Total:", orderSummary.FinalTotal.ToString("C"));
                        AddDetailRowToPanel(OrderDetailsContent, "Customer Name:", orderSummary.FullName);
                        AddDetailRowToPanel(OrderDetailsContent, "Email:", orderSummary.Email);
                        AddDetailRowToPanel(OrderDetailsContent, "Phone:", orderSummary.PhoneNumber);
                        AddDetailRowToPanel(OrderDetailsContent, "Address:", orderSummary.Address);
                        AddDetailRowToPanel(OrderDetailsContent, "Postal Code:", orderSummary.PostalCode);

                        if (!string.IsNullOrEmpty(orderSummary.AdditionalInfo))
                        {
                            AddDetailRowToPanel(OrderDetailsContent, "Additional Info:", orderSummary.AdditionalInfo);
                        }

                        AddDetailRowToPanel(OrderDetailsContent, "Warranty Tax:", orderSummary.WarrantyTax.ToString("C"));

                        if (!string.IsNullOrEmpty(orderSummary.ContractDetails))
                        {
                            AddDetailRowToPanel(OrderDetailsContent, "Contract Details:", orderSummary.ContractDetails);
                        }

                        // Reset contract UI elements
                        GenerateContractButton.Content = "Generate Contract";
                        GenerateContractButton.IsEnabled = true;
                        ContractSuccessMessage.Visibility = Visibility.Collapsed;
                        ContractErrorMessage.Visibility = Visibility.Collapsed;

                        // Set XamlRoot and show dialog
                        OrderDetailsDialog.XamlRoot = Content.XamlRoot;
                        _ = OrderDetailsDialog.ShowAsync();
                    }
                    catch (Exception exception)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error setting up dialog: {exception.Message}");
                    }
                });
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing order details: {exception.Message}");
            }
        }

        /// <summary>
        /// Event handler for the Generate Contract button click in the XAML dialog.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private async void GenerateContractButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentOrderSummary == null) return;

            try
            {
                // Update UI to show loading state
                GenerateContractButton.IsEnabled = false;
                GenerateContractButton.Content = "Generating...";
                ContractSuccessMessage.Visibility = Visibility.Collapsed;
                ContractErrorMessage.Visibility = Visibility.Collapsed;

                // Generate the contract
                await HandleGenerateAndDisplayContractClick(currentOrderSummary);

                // Show success state
                GenerateContractButton.Content = "✓ Contract Generated";
                ContractSuccessMessage.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Contract generation error: {ex.Message}");

                // Show error state
                GenerateContractButton.IsEnabled = true;
                GenerateContractButton.Content = "Generate Contract";
                ContractErrorMessage.Text = $"✗ Failed to generate contract: {ex.Message}";
                ContractErrorMessage.Visibility = Visibility.Visible;
            }
        }        /// <summary>
        /// Handles the click event for generating and displaying a contract.
        /// Similar to BuyerProfile implementation - generates PDF and opens it directly.
        /// </summary>
        /// <param name="orderSummary">The order summary object containing contract details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task HandleGenerateAndDisplayContractClick(OrderSummary orderSummary)
        {
            // Create a new contract
            var contract = new Contract
            {
                OrderID = orderSummary.ID,
                ContractStatus = "ACTIVE",
                ContractContent = orderSummary.ContractDetails ?? "Standard contract terms",
                RenewalCount = 0,
                AdditionalTerms = string.Empty
            };

            // Get the predefined contract type (assuming BorrowingContract for now)
            var predefinedContract = await contractViewModel.GetPredefinedContractByPredefineContractTypeAsync(PredefinedContractType.BorrowingContract);

            // Add the contract to the database first
            byte[] pdfContent = new byte[0];
            var newContract = await contractViewModel.AddContractAsync(contract, pdfContent);

            // Generate and display the contract using the same approach as BuyerProfile
            await contractViewModel.GenerateAndSaveContractAsync(newContract.ContractID);
            
            // Success feedback is now handled in the UI directly, no separate dialog needed
        }

        /// <summary>
        /// Displays a simple message dialog with the specified title and message.
        /// </summary>
        /// <param name="title">The title of the dialog. Must not be null.</param>
        /// <param name="message">The message to display. Must not be null.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when there is an error showing the message dialog.</exception>
        private Task ShowCustomMessageAsync(string title, string message)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            DispatcherQueue.TryEnqueue(async () =>
            {
                try
                {
                    if (Content?.XamlRoot == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error: XamlRoot is null. Cannot display message: {title} - {message}");
                        taskCompletionSource.SetResult(false);
                        return;
                    }

                    var messageDialog = new ContentDialog
                    {
                        Title = title,
                        Content = message,
                        CloseButtonText = "OK",
                        XamlRoot = Content.XamlRoot
                    };

                    await messageDialog.ShowAsync();
                    taskCompletionSource.SetResult(true);
                }
                catch (Exception exception)
                {
                    System.Diagnostics.Debug.WriteLine($"Error showing message: {exception}");
                    taskCompletionSource.SetException(exception);
                }
            });

            return taskCompletionSource.Task;
        }

        /// <summary>
        /// Adds a row with a label and value to the specified panel.
        /// </summary>
        /// <param name="panel">The panel to which the row will be added. Must not be null.</param>
        /// <param name="label">The label text to display. Must not be null.</param>
        /// <param name="value">The value text to display. Must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when panel, label, or value is null.</exception>
        private void AddDetailRowToPanel(Panel panel, string label, string value)
        {
            var stackPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 5) };
            stackPanel.Children.Add(new TextBlock { Text = label, FontWeight = FontWeights.SemiBold, Width = 150 });
            stackPanel.Children.Add(new TextBlock { Text = value, TextWrapping = TextWrapping.Wrap });
            panel.Children.Add(stackPanel);
        }

        /// <summary>
        /// Event handler for the search text box text changed event. Filters orders based on search text.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            await Task.Delay(300); // 300ms delay
            await LoadOrders(SearchTextBox.Text);
        }

        /// <summary>
        /// Displays a message dialog with the specified title and message.
        /// </summary>
        /// <param name="title">The title of the dialog. Must not be null.</param>
        /// <param name="message">The message to display. Must not be null.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the XamlRoot is not available.</exception>
        private Task ShowMessageAsync(string title, string message)
        {
            var tcs = new TaskCompletionSource<bool>();

            try
            {
                bool enqueued = DispatcherQueue.TryEnqueue(async () =>
                {
                    try
                    {
                        if (this.Content?.XamlRoot == null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error: XamlRoot is null. Cannot display message: {title} - {message}");
                            tcs.SetResult(false);
                            return;
                        }

                        var messageDialog = new ContentDialog
                        {
                            Title = title,
                            Content = message,
                            CloseButtonText = "OK",
                            XamlRoot = this.Content.XamlRoot,
                        };

                        await messageDialog.ShowAsync();
                        tcs.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error showing message dialog: {ex}");
                        tcs.SetException(ex);
                    }
                });

                if (!enqueued)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to enqueue message dialog operation");
                    tcs.SetResult(false);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ShowMessageAsync: {ex}");
                tcs.SetException(ex);
            }

            return tcs.Task;
        }

        /// <summary>
        /// Adds a row with label and value to the order details dialog.
        /// </summary>
        /// <param name="label">The label to display. Must not be null.</param>
        /// <param name="value">The value to display. Must not be null.</param>
        private void AddDetailRow(string label, string value)
        {
            var stackPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 5) };
            stackPanel.Children.Add(new TextBlock { Text = label, FontWeight = FontWeights.SemiBold, Width = 150 });
            stackPanel.Children.Add(new TextBlock { Text = value });
            OrderDetailsContent.Children.Add(stackPanel);
        }        
        /// <summary>
        /// Event handler for the Track Order button click.
        /// Opens the TrackedOrderWindow with the specific OrderID.
        /// </summary>
        /// <param name="sender">The button that triggered the event</param>
        /// <param name="e">Event arguments</param>
        private async void TrackOrder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int orderID)
            {
                try
                {
                    // Use the ViewModel to handle the complete track order workflow
                    bool success = await orderHistoryViewModel.TrackOrderAsync(orderID);
                    
                    if (!success)
                    {
                        await ShowCustomMessageAsync("Error", "Failed to process order tracking. Please try again later.");
                    }
                }
                catch (Exception exception)
                {
                    await ShowCustomMessageAsync("Error", $"Failed to open order tracking: {exception.Message}");
                }
            }
        }
    }
    
    /// <summary>
    /// Converter to convert boolean IsExpanded property to expand/collapse icon
    /// </summary>
    public class BoolToExpandIconConverter : Microsoft.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isExpanded)
            {
                return isExpanded ? "▼" : "▶";
            }
            return "▶";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class OrderGroup : INotifyPropertyChanged
    {
        private bool _isExpanded = true; // Start expanded by default

        public string Name { get; set; }
        public List<dynamic> Items { get; set; }
        
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ItemsVisibility));
                }
            }
        }

        public Visibility ItemsVisibility => IsExpanded ? Visibility.Visible : Visibility.Collapsed;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
