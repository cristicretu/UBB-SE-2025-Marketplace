using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SharedClassLibrary.Domain;
using MarketPlace924.ViewModel;
using Microsoft.UI.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;
using SharedClassLibrary.Service; // Add this using directive if not present
using SharedClassLibrary.Shared; // Add this using directive for Configuration

namespace MarketPlace924
{
    [ExcludeFromCodeCoverage]
    public sealed partial class OrderHistoryView : Window
    {
        /// <summary>
        /// The oder history view window;
        /// </summary>
        private readonly int userId;
        private IOrderViewModel orderViewModel;
        private IContractViewModel contractViewModel;
        private Dictionary<int, string> orderProductCategoryTypes = new Dictionary<int, string>();

        /// <summary>
        /// Initializes a new instance of the OrderHistoryUI window.
        /// </summary>
        /// <param name="connectionString">The database connection string. Must not be null or empty.</param>
        /// <param name="userId">The ID of the user whose order history to display. Must be a positive integer.</param>
        /// <exception cref="ArgumentNullException">Thrown when connectionString is null.</exception>
        /// <exception cref="ArgumentException">Thrown when userId is less than or equal to zero.</exception>
        public OrderHistoryView(string connectionString, int userId)
        {
            InitializeComponent();
            this.userId = userId;
            orderViewModel = new OrderViewModel(connectionString);
            contractViewModel = new ContractViewModel(connectionString);

            this.Activated += Window_Activated;
        }

        /// <summary>
        /// Event handler triggered when the window is activated. Loads initial order data.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">Event data.</param>
        private async void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            this.Activated -= Window_Activated;
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
                        OrdersListView.ItemsSource = orderDisplayInfos;
                        OrdersListView.Visibility = Visibility.Visible;
                        NoResultsText.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
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
                            clickedButton.Content = "See Details";
                            clickedButton.IsEnabled = true;
                        }
                    });
                }
            }
        }

        /// <summary>
        /// Displays a detailed dialog for a specific order summary.
        /// </summary>
        /// <param name="orderSummary">The order summary object containing details to display. Must not be null.</param>
        /// <param name="orderSummaryId">The ID of the order summary. Must be a positive integer.</param>
        /// <exception cref="ArgumentNullException">Thrown when orderSummary is null.</exception>
        /// <exception cref="Exception">Thrown when there is an error setting up the dialog.</exception>
        private void ShowOrderDetailsDialog(OrderSummary orderSummary, int orderSummaryId)
        {
            try
            {
                bool isTaskEnqueued = DispatcherQueue.TryEnqueue(() =>
                {
                    try
                    {
                        if (Content?.XamlRoot == null)
                        {
                            System.Diagnostics.Debug.WriteLine("Error: XamlRoot is null - cannot show dialog");
                            return;
                        }

                        var orderDetailsPanel = new StackPanel { Spacing = 10, Padding = new Thickness(10) };

                        AddDetailRowToPanel(orderDetailsPanel, "Order Summary ID:", orderSummary.ID.ToString());
                        AddDetailRowToPanel(orderDetailsPanel, "Subtotal:", orderSummary.Subtotal.ToString("C"));
                        AddDetailRowToPanel(orderDetailsPanel, "Delivery Fee:", orderSummary.DeliveryFee.ToString("C"));
                        AddDetailRowToPanel(orderDetailsPanel, "Final Total:", orderSummary.FinalTotal.ToString("C"));
                        AddDetailRowToPanel(orderDetailsPanel, "Customer Name:", orderSummary.FullName);
                        AddDetailRowToPanel(orderDetailsPanel, "Email:", orderSummary.Email);
                        AddDetailRowToPanel(orderDetailsPanel, "Phone:", orderSummary.PhoneNumber);
                        AddDetailRowToPanel(orderDetailsPanel, "Address:", orderSummary.Address);
                        AddDetailRowToPanel(orderDetailsPanel, "Postal Code:", orderSummary.PostalCode);

                        if (!string.IsNullOrEmpty(orderSummary.AdditionalInfo))
                        {
                            AddDetailRowToPanel(orderDetailsPanel, "Additional Info:", orderSummary.AdditionalInfo);
                        }

                        if (orderProductCategoryTypes.TryGetValue(orderSummary.ID, out string productType) && productType == "borrowed")
                        {
                            AddDetailRowToPanel(orderDetailsPanel, "Warranty Tax:", orderSummary.WarrantyTax.ToString("C"));

                            if (!string.IsNullOrEmpty(orderSummary.ContractDetails))
                            {
                                AddDetailRowToPanel(orderDetailsPanel, "Contract Details:", orderSummary.ContractDetails);
                            }

                            var viewContractButton = new Button
                            {
                                Content = "View Contract PDF",
                                Margin = new Thickness(0, 10, 0, 0),
                                HorizontalAlignment = HorizontalAlignment.Left
                            };

                            viewContractButton.Click += (s, args) =>
                            {
                                // Never use Task.Run for UI, use DispatcherQueue instead
                                DispatcherQueue.TryEnqueue(async () =>
                                {
                                    try
                                    {
                                        await HandleContractViewClick(orderSummary);
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"Contract view error: {ex.Message}");
                                    }
                                });
                            };

                            orderDetailsPanel.Children.Add(viewContractButton);
                        }

                        var scrollViewer = new ScrollViewer
                        {
                            Content = orderDetailsPanel,
                            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                            MaxHeight = 500
                        };

                        ContentDialog errorContentDialog = new ContentDialog
                        {
                            Title = "Order Details",
                            Content = scrollViewer,
                            CloseButtonText = "Close",
                            DefaultButton = ContentDialogButton.Close,
                            XamlRoot = Content.XamlRoot
                        };

                        _ = errorContentDialog.ShowAsync();
                    }
                    catch (Exception exception)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error setting up dialog: {exception.Message}");
                    }
                });

                if (!isTaskEnqueued)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to enqueue dialog creation operation");
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing order details: {exception.Message}");
            }
        }

        /// <summary>
        /// Handles the click event for viewing the contract associated with an order summary.
        /// </summary>
        /// <param name="orderSummary">The order summary object containing contract details. Must not be null.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when there is an error retrieving or displaying the contract.</exception>
        private async Task HandleContractViewClick(OrderSummary orderSummary)
        {
            try
            {
                var contract = await contractViewModel.GetContractByIdAsync(orderSummary.ID);

                var contractTypeValues = Enum.GetValues(typeof(PredefinedContractType));
                PredefinedContractType firstContractType = default;
                if (contractTypeValues.Length > 0)
                {
                    firstContractType = (PredefinedContractType)contractTypeValues.GetValue(0);
                }

                var predefinedContract = await contractViewModel
                    .GetPredefinedContractByPredefineContractTypeAsync(firstContractType);

                var fieldReplacements = new Dictionary<string, string>
                {
                    { "CustomerName", orderSummary.FullName },
                    { "ProductName", "Borrowed Product" },
                    { "StartDate", DateTime.Now.ToString("yyyy-MM-dd") },
                    { "EndDate", DateTime.Now.AddMonths(3).ToString("yyyy-MM-dd") },
                    { "Price", orderSummary.FinalTotal.ToString("C") }
                };
            }
            catch (Exception exception)
            {
                await ShowCustomMessageAsync("Error", $"Failed to generate contract: {exception.Message}");
            }
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
        /// Shows a PDF document in a dialog.
        /// </summary>
        /// <param name="pdfBytes">The PDF document as a byte array. Must not be null or empty.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="IOException">Thrown when there is an error writing the PDF to a temporary file.</exception>
        /// <exception cref="ArgumentNullException">Thrown when pdfBytes is null.</exception>
        private async Task ShowPdfDialog(byte[] pdfBytes)
        {
            var contractFilePath = Path.Combine(Path.GetTempPath(), $"contract_{Guid.NewGuid()}.pdf");
            await File.WriteAllBytesAsync(contractFilePath, pdfBytes);

            var pdfDialog = new ContentDialog
            {
                Title = "Contract PDF",
                CloseButtonText = "Close",
                XamlRoot = this.Content.XamlRoot,
                Content = new WebView2
                {
                    Width = 800,
                    Height = 1000,
                    Source = new Uri(contractFilePath)
                }
            };

            await pdfDialog.ShowAsync();

            try
            {
                File.Delete(contractFilePath);
            }
            catch
            {
            }
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
    }
}
