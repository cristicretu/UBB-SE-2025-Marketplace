using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows;
using MarketPlace924.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace MarketPlace924.View
{
    [ExcludeFromCodeCoverage]
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TrackedOrderControlPage : Page
    {
        private readonly ITrackedOrderViewModel viewModel;
        public int TrackedOrderID { get; set; }

        internal TrackedOrderControlPage(ITrackedOrderViewModel viewModel, int trackedOrderID)
        {
            InitializeComponent(); // Must be called first to initialize XAML controls
            this.viewModel = viewModel;
            TrackedOrderID = trackedOrderID;
            DataContext = this.viewModel;

            // Initialize UI state
            if (DateTimePickers != null)
            {
                DateTimePickers.Visibility = Visibility.Collapsed;
            }

            if (deliveryCalendarDatePicker != null)
            {
                deliveryCalendarDatePicker.Visibility = Visibility.Collapsed;
            }

            if (confirmChangeEstimatedDeliveryDateButton != null)
            {
                confirmChangeEstimatedDeliveryDateButton.Visibility = Visibility.Collapsed;
            }

            if (AddDetails != null)
            {
                AddDetails.Visibility = Visibility.Collapsed;
            }

            if (UpdateDetails != null)
            {
                UpdateDetails.Visibility = Visibility.Collapsed;
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

        private async void LoadOrderDataButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await viewModel.LoadOrderDataAsync(TrackedOrderID);
            }
            catch (Exception ex)
            {
                await ShowErrorDialog($"Error loading order data: {ex.Message}");
            }
        }

        private async void RevertLastCheckpointButton_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                await viewModel.RevertLastCheckpointAsync(TrackedOrderID);
                await ShowSuccessDialog("Successfully reverted to previous checkpoint");
            }
            catch (Exception ex)
            {
                await ShowErrorDialog($"Error reverting checkpoint: {ex.Message}");
            }
        }

        private async void ConfirmChangeEstimatedDeliveryDateButton_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (deliveryCalendarDatePicker?.Date.HasValue == true)

                {
                    await viewModel.UpdateEstimatedDeliveryDateAsync(TrackedOrderID, deliveryCalendarDatePicker.Date.Value.DateTime);
                    await ShowSuccessDialog("Successfully updated estimated delivery date");
                    deliveryCalendarDatePicker.Visibility = Visibility.Collapsed;
                    confirmChangeEstimatedDeliveryDateButton.Visibility = Visibility.Collapsed;
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
                    await viewModel.AddNewCheckpointAsync(TrackedOrderID, DescriptionTextBoxAdd.Text);
                    await ShowSuccessDialog("Successfully added new checkpoint");
                    DescriptionTextBoxAdd.Text = string.Empty;
                    AddDetails.Visibility = Visibility.Collapsed;
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
                    await viewModel.UpdateLastCheckpointAsync(TrackedOrderID, DescriptionTextBoxUpdate.Text);
                    await ShowSuccessDialog("Successfully updated checkpoint");
                    DescriptionTextBoxUpdate.Text = string.Empty;
                    UpdateDetails.Visibility = Visibility.Collapsed;
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

        private void UpdateCurrentCheckpointButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (UpdateDetails != null)
            {
                UpdateDetails.Visibility = Visibility.Visible;
            }
        }

        private void ChangeEstimatedDeliveryDateButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (deliveryCalendarDatePicker != null)
            {
                deliveryCalendarDatePicker.Visibility = Visibility.Visible;
            }
            if (confirmChangeEstimatedDeliveryDateButton != null)
            {
                confirmChangeEstimatedDeliveryDateButton.Visibility = Visibility.Visible;
            }
        }

        private void AddNewCheckpointButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (AddDetails != null)
            {
                AddDetails.Visibility = Visibility.Visible;
            }
        }

        private void ManualTimestampRadio_Checked(object sender, RoutedEventArgs e)

        {
            UpdateDetails.Visibility = Visibility.Visible;
        }

        private void AutoTimestampRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (DateTimePickers != null)
            {
                DateTimePickers.Visibility = Visibility.Collapsed;
            }

        }
    }
}