using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using MarketMinds.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace MarketMinds.Views
{
    [ExcludeFromCodeCoverage]
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TrackedOrderControlPage : Page
    {
        public int TrackedOrderID { get; set; }
        public TrackedOrderViewModel ViewModel { get; }

        public TrackedOrderControlPage()
        {
            InitializeComponent();
            ViewModel = App.TrackedOrderViewModel;
            DataContext = ViewModel;
        }

        /// <summary>
        /// Called when the page is navigated to.
        /// </summary>
        /// <param name="e">Navigation event arguments</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            // Handle navigation parameter
            if (e.Parameter is int trackedOrderID)
            {
                await SetTrackedOrderIDAsync(trackedOrderID);
            }
        }

        public async void SetTrackedOrderID(int trackedOrderID)
        {
            TrackedOrderID = trackedOrderID;
            await LoadOrderData();
        }

        public async Task SetTrackedOrderIDAsync(int trackedOrderID)
        {
            TrackedOrderID = trackedOrderID;
            await LoadOrderData();
        }

        private async Task LoadOrderData()
        {
            try
            {
                await ViewModel.LoadOrderDataAsync(TrackedOrderID);
            }
            catch (Exception ex)
            {
                await ShowErrorDialog($"Error loading order data: {ex.Message}");
            }
        }

        /// <summary>
        /// Shows an error dialog with the specified error message.
        /// </summary>
        /// <param name="message">The message to be displayed in the error dialog.</param>
        /// <returns></returns>
        private async Task ShowErrorDialog(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }

        /// <summary>
        /// Shows a success dialog with the specified message.
        /// </summary>
        /// <param name="message">The message to be shown in the success dialog.</param>
        /// <returns></returns>
        private async Task ShowSuccessDialog(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Success",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private async void RevertLastCheckpointButton_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                await ViewModel.RevertLastCheckpointAsync(TrackedOrderID);
                await ShowSuccessDialog("Successfully reverted to previous checkpoint");
            }
            catch (Exception ex)
            {
                await ShowErrorDialog($"Error reverting checkpoint: {ex.Message}");
            }
        }

        private void ShowDeliveryDateUpdateButton_Clicked(object sender, RoutedEventArgs e)
        {
            deliveryDateUpdatePanel.Visibility = Visibility.Visible;
            showDeliveryDateUpdateButton.Visibility = Visibility.Collapsed;
        }

        private async void ConfirmChangeEstimatedDeliveryDateButton_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (deliveryCalendarDatePicker?.Date.HasValue == true)
                {
                    var selectedDate = deliveryCalendarDatePicker.Date.Value.DateTime;

                    // Get the tracked order to access the order ID
                    var trackedOrder = ViewModel.CurrentOrder;
                    if (trackedOrder == null)
                    {
                        await ShowErrorDialog("Order not found");
                        return;
                    }

                    // Get the order date using the public method
                    var orderDate = await ViewModel.GetOrderDateAsync(trackedOrder.OrderID);

                    // Validate that the selected date is not before the order placement date
                    if (selectedDate.Date < orderDate.Date)
                    {
                        await ShowErrorDialog("Delivery date cannot be before the order placement date");
                        return;
                    }

                    await ViewModel.UpdateEstimatedDeliveryDateAsync(TrackedOrderID, selectedDate);
                    await ShowSuccessDialog("Successfully updated estimated delivery date");

                    // Reset the controls
                    deliveryCalendarDatePicker.Date = null;
                    deliveryDateUpdatePanel.Visibility = Visibility.Collapsed;
                    showDeliveryDateUpdateButton.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                await ShowErrorDialog($"Error updating delivery date: {ex.Message}");
            }
        }

        private async void ConfirmAddNewCheckpointButton_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(DescriptionTextBoxAdd?.Text))
                {
                    DateTime timestamp;
                    if (ManualTimestampRadio.IsChecked == true)
                    {
                        if (!TimestampDatePicker.Date.HasValue)
                        {
                            await ShowErrorDialog("Please select a date for the checkpoint");
                            return;
                        }

                        var selectedDate = TimestampDatePicker.Date.Value.DateTime;
                        var selectedTime = TimestampTimePicker.Time;
                        timestamp = selectedDate.Date + selectedTime;

                        // Get the newest checkpoint timestamp
                        var newestCheckpoint = ViewModel.Checkpoints.FirstOrDefault();
                        if (newestCheckpoint != null && timestamp < newestCheckpoint.Timestamp)
                        {
                            await ShowErrorDialog($"New checkpoint timestamp must be newer than the newest checkpoint ({newestCheckpoint.Timestamp:dd/MM/yyyy HH:mm})");
                            return;
                        }
                    }
                    else
                    {
                        timestamp = DateTime.UtcNow;
                    }

                    // Get the selected status from the ComboBox
                    if (StatusComboBoxAdd.SelectedItem is ComboBoxItem selectedStatusItem && selectedStatusItem.Content is string statusString)
                    {
                        if (Enum.TryParse(statusString, out MarketMinds.Shared.Models.OrderStatus selectedStatus))
                        {
                            await ViewModel.AddNewCheckpointAsync(TrackedOrderID, DescriptionTextBoxAdd.Text, timestamp, selectedStatus);
                            await ShowSuccessDialog("Successfully added new checkpoint");
                            DescriptionTextBoxAdd.Text = string.Empty;
                            LocationTextBoxAdd.Text = string.Empty;
                            StatusComboBoxAdd.SelectedIndex = -1;
                        }
                        else
                        {
                            await ShowErrorDialog("Invalid status selected.");
                        }
                    }
                    else
                    {
                        await ShowErrorDialog("Please select a valid status for the new checkpoint");
                    }
                }
                else
                {
                    await ShowErrorDialog("Please enter a description for the new checkpoint");
                }
            }
            catch (Exception ex)
            {
                await ShowErrorDialog($"Error adding checkpoint: {ex.Message}");
            }
        }

        private async void ConfirmUpdateCurrentCheckpointButton_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(DescriptionTextBoxUpdate?.Text))
                {
                    DateTime timestamp;
                    if (ManualTimestampRadio.IsChecked == true)
                    {
                        if (!TimestampDatePicker.Date.HasValue)
                        {
                            await ShowErrorDialog("Please select a date for the checkpoint");
                            return;
                        }

                        var selectedDate = TimestampDatePicker.Date.Value.DateTime;
                        var selectedTime = TimestampTimePicker.Time;
                        timestamp = selectedDate.Date + selectedTime;

                        // Get the second newest checkpoint timestamp (since we're updating the newest one)
                        var secondNewestCheckpoint = ViewModel.Checkpoints.Skip(1).FirstOrDefault();
                        if (secondNewestCheckpoint != null && timestamp <= secondNewestCheckpoint.Timestamp)
                        {
                            await ShowErrorDialog($"Updated checkpoint timestamp must be after the previous checkpoint ({secondNewestCheckpoint.Timestamp:dd/MM/yyyy HH:mm})");
                            return;
                        }
                    }
                    else
                    {
                        timestamp = DateTime.UtcNow;
                    }

                    await ViewModel.UpdateLastCheckpointAsync(TrackedOrderID, DescriptionTextBoxUpdate.Text, timestamp);
                    await ShowSuccessDialog("Successfully updated checkpoint");
                    DescriptionTextBoxUpdate.Text = string.Empty;
                    LocationTextBoxUpdate.Text = string.Empty;
                    StatusComboBoxUpdate.SelectedIndex = -1;
                    LocationTextBoxUpdate.Visibility = Visibility.Collapsed;
                    DescriptionTextBoxUpdate.Visibility = Visibility.Collapsed;
                    StatusComboBoxUpdate.Visibility = Visibility.Collapsed;
                    TimestampRadioButtons.Visibility = Visibility.Collapsed;
                    DateTimePickers.Visibility = Visibility.Collapsed;
                    confirmUpdateCurrentCheckpointButton.Visibility = Visibility.Collapsed;
                }
                else
                {
                    await ShowErrorDialog("Please enter a description for the checkpoint update");
                }
            }
            catch (Exception ex)
            {
                await ShowErrorDialog($"Error updating checkpoint: {ex.Message}");
            }
        }

        private void ManualTimestampRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (TimestampRadioButtons != null)
            {
                TimestampRadioButtons.Visibility = Visibility.Visible;
                if (DateTimePickers != null)
                {
                    DateTimePickers.Visibility = Visibility.Visible;
                }
            }
        }

        private void AutoTimestampRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (TimestampRadioButtons != null)
            {
                TimestampRadioButtons.Visibility = Visibility.Visible;
                if (DateTimePickers != null)
                {
                    DateTimePickers.Visibility = Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// Handles the back button click to navigate back to the order history.
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">Event arguments</param>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the current frame and navigate back
                Frame currentFrame = null;
                
                // Try to find the frame by walking up the visual tree
                var parent = this.Parent;
                while (parent != null && currentFrame == null)
                {
                    if (parent is Frame frame)
                    {
                        currentFrame = frame;
                        break;
                    }
                    parent = (parent as FrameworkElement)?.Parent;
                }
                
                // If we found a frame, navigate back
                if (currentFrame != null)
                {
                    if (currentFrame.CanGoBack)
                    {
                        currentFrame.GoBack();
                    }
                    else
                    {
                        // If can't go back, navigate to OrderHistoryView directly
                        currentFrame.Navigate(typeof(OrderHistoryView));
                    }
                }
                else
                {
                    // Fallback: try to use the app's main frame if available
                    if (App.MainWindow?.Content is Frame mainFrame)
                    {
                        if (mainFrame.CanGoBack)
                        {
                            mainFrame.GoBack();
                        }
                        else
                        {
                            mainFrame.Navigate(typeof(OrderHistoryView));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating back: {ex.Message}");
                // If navigation fails, try to show an error dialog
                _ = ShowErrorDialog($"Failed to navigate back: {ex.Message}");
            }
        }
    }
}