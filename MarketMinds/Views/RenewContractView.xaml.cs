using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace MarketMinds.Views
{
    /// <summary>
    /// Interaction logic for the Renew Contract page.
    /// Handles UI logic for selecting and renewing a contract.
    /// </summary>
    public sealed partial class RenewContractView : Window
    {
        private readonly IContractRenewViewModel viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenewContractView"/> class.
        /// </summary>
        public RenewContractView()
        {
            this.InitializeComponent();

            // Set window size
            // this.AppWindow.Resize(new Windows.Graphics.SizeInt32(1000, 700));

            // Get the view model from App like ChatBotPage does
            this.viewModel = App.ContractRenewViewModel;

            // Set the data context for binding
            if (this.RootGrid != null)
            {
                this.RootGrid.DataContext = this.viewModel;
            }

            // Initialize UI state
            this.SetupInitialUIState();

            // Add a timeout for the API call to prevent infinite loading
            _ = LoadContractsWithTimeoutAsync();
        }

        private async Task LoadContractsWithTimeoutAsync()
        {
            try
            {
                // Show loading overlay
                if (LoadingOverlay != null)
                {
                    LoadingOverlay.Visibility = Visibility.Visible;
                }

                // Create a cancellation token that will cancel after 10 seconds
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                {
                    // Create a task that will complete when the contracts are loaded or when the timeout occurs
                    var loadTask = LoadContractsAsync();
                    var completedTask = await Task.WhenAny(loadTask, Task.Delay(10000, cts.Token));

                    if (completedTask == loadTask)
                    {
                        // Task completed successfully
                        await loadTask;
                    }
                    else
                    {
                        // Task timed out
                        Debug.WriteLine("Loading contracts timed out after 10 seconds");

                        // Hide loading overlay
                        this.DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
                        {
                            if (LoadingOverlay != null)
                            {
                                LoadingOverlay.Visibility = Visibility.Collapsed;
                            }

                            // Show error message to user
                            ShowErrorMessageAsync("Unable to load contracts",
                                "The server took too long to respond. Please try again later.").GetAwaiter();
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in LoadContractsWithTimeoutAsync: {ex.Message}");

                // Hide loading overlay
                this.DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
                {
                    if (LoadingOverlay != null)
                    {
                        LoadingOverlay.Visibility = Visibility.Collapsed;
                    }
                });
            }
        }

        private async Task ShowErrorMessageAsync(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }

        /// <summary>
        /// Sets up the initial UI state.
        /// </summary>
        private void SetupInitialUIState()
        {
            // Default visibility states
            if (ContractDetailsPanel != null)
            {
                ContractDetailsPanel.Visibility = Visibility.Collapsed;
            }

            if (RenewalForm != null)
            {
                RenewalForm.Visibility = Visibility.Collapsed;
            }

            if (FormPlaceholder != null)
            {
                FormPlaceholder.Visibility = Visibility.Visible;
            }

            if (LoadingOverlay != null)
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Loads contracts for the current buyer and sets them as ComboBox items.
        /// </summary>
        private async Task LoadContractsAsync()
        {
            try
            {
                // Show loading indicator
                if (LoadingOverlay != null)
                {
                    LoadingOverlay.Visibility = Visibility.Visible;
                }

                await this.viewModel.RefreshContractsAsync();

                // Use UI thread to update UI elements
                this.DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
                {
                    if (ContractComboBox != null && this.viewModel.Contracts != null)
                    {
                        // Create wrapper objects for display
                        var contractDisplayItems = this.viewModel.Contracts.Select(c =>
                        {
                            // Add DisplayName property to each contract
                            dynamic contractWrapper = new System.Dynamic.ExpandoObject();
                            var wrapperDict = (System.Collections.Generic.IDictionary<string, object>)contractWrapper;

                            // Copy all properties from the original contract
                            foreach (var prop in c.GetType().GetProperties())
                            {
                                wrapperDict[prop.Name] = prop.GetValue(c);
                            }

                            // Add the display name property
                            wrapperDict["DisplayName"] = $"Contract {c.ContractID} - {c.ContractStatus}";

                            return contractWrapper;
                        }).ToList();

                        ContractComboBox.ItemsSource = contractDisplayItems;
                        Debug.WriteLine($"Loaded {contractDisplayItems.Count} contracts");
                    }
                    else
                    {
                        Debug.WriteLine("ContractComboBox or Contracts collection is null");
                    }

                    // Hide loading indicator
                    if (LoadingOverlay != null)
                    {
                        LoadingOverlay.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading contracts: {ex.Message}");
                await ShowErrorDialogAsync("Error loading contracts", ex.Message);

                // Hide loading indicator even if there was an error
                if (LoadingOverlay != null)
                {
                    LoadingOverlay.Visibility = Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// Handles the selection change event of the contract ComboBox.
        /// </summary>
        /// <summary>
        /// Handles the selection change event of the contract ComboBox.
        /// </summary>
        private async void ContractComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender is ComboBox comboBox && comboBox.SelectedItem != null)
                {
                    // Show loading indicator before starting any operations
                    if (LoadingOverlay != null)
                    {
                        LoadingOverlay.Visibility = Visibility.Visible;
                    }

                    // Extract the ContractID from the dynamic wrapper object
                    dynamic selectedItem = comboBox.SelectedItem;
                    long contractId = selectedItem.ContractID;

                    Debug.WriteLine($"Contract selected, ID: {contractId}");

                    // Get the actual contract from the view model's collection
                    var actualContract = this.viewModel.Contracts.FirstOrDefault(c => c.ContractID == contractId);

                    if (actualContract != null)
                    {
                        // Set the selected contract in the view model
                        this.viewModel.SelectedContract = actualContract;

                        // Wait for the async operations to complete
                        await Task.Delay(300); // Give the LoadContractDetailsAsync method time to complete

                        // Update UI on UI thread
                        this.DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
                        {
                            UpdateUIWithContractDetails();
                            Debug.WriteLine($"Updated UI with details for contract ID: {contractId}");
                        });
                    }
                    else
                    {
                        Debug.WriteLine($"Could not find contract with ID {contractId} in Contracts collection");
                        ResetUI();
                    }
                }
                else
                {
                    Debug.WriteLine("No contract selected");
                    ResetUI();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ContractComboBox_SelectionChanged: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                ResetUI();
            }
            finally
            {
                // Hide loading indicator
                if (LoadingOverlay != null)
                {
                    this.DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
                    {
                        LoadingOverlay.Visibility = Visibility.Collapsed;
                    });
                }
            }
        }

        /// <summary>
        /// Updates the UI with contract details.
        /// </summary>
        private void UpdateUIWithContractDetails()
        {
            Debug.WriteLine($"UpdateUIWithContractDetails - StartDate from viewModel: {this.viewModel.StartDate}");
            Debug.WriteLine($"UpdateUIWithContractDetails - EndDate from viewModel: {this.viewModel.EndDate}");

            // Update date text blocks
            if (StartDateTextBlock != null)
            {
                string startDateText = this.viewModel.StartDate.HasValue
                    ? $"Start Date: {this.viewModel.StartDate.Value.ToString("MM/dd/yyyy")}"
                    : "Start Date: Not available";

                Debug.WriteLine($"Setting StartDateTextBlock.Text to: {startDateText}");
                StartDateTextBlock.Text = startDateText;
            }
            else
            {
                Debug.WriteLine("StartDateTextBlock is null");
            }

            if (EndDateTextBlock != null)
            {
                string endDateText = this.viewModel.EndDate.HasValue
                    ? $"End Date: {this.viewModel.EndDate.Value.ToString("MM/dd/yyyy")}"
                    : "End Date: Not available";

                Debug.WriteLine($"Setting EndDateTextBlock.Text to: {endDateText}");
                EndDateTextBlock.Text = endDateText;
            }
            else
            {
                Debug.WriteLine("EndDateTextBlock is null");
            }

            // Update status text and color
            if (StatusTextBlock != null)
            {
                StatusTextBlock.Text = this.viewModel.StatusText ?? "Status: Unknown";
                StatusTextBlock.Foreground = this.viewModel.StatusColor == "Green"
                    ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green)
                    : new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red);
            }

            // Set start date for renewal
            if (StartDateValueTextBlock != null && this.viewModel.EndDate.HasValue)
            {
                StartDateValueTextBlock.Text = this.viewModel.EndDate.Value.ToString("MM/dd/yyyy");
            }
            else if (StartDateValueTextBlock != null)
            {
                StartDateValueTextBlock.Text = "Not available";
            }

            // Configure date picker
            if (EndDatePicker != null && this.viewModel.EndDate.HasValue)
            {
                // Set default date (one year from end date)
                var newDate = this.viewModel.EndDate.Value.AddYears(1);
                EndDatePicker.Date = new DateTimeOffset(newDate);
            }
            else if (EndDatePicker != null)
            {
                // Fallback if end date is not available
                EndDatePicker.Date = DateTimeOffset.Now.AddYears(1);
            }

            // Show/hide panels based on contract status
            if (ContractDetailsPanel != null)
            {
                ContractDetailsPanel.Visibility = Visibility.Visible;
            }

            if (RenewalForm != null && FormPlaceholder != null)
            {
                if (this.viewModel.IsRenewalAllowed)
                {
                    RenewalForm.Visibility = Visibility.Visible;
                    FormPlaceholder.Visibility = Visibility.Collapsed;
                    Debug.WriteLine("Showing renewal form");
                }
                else
                {
                    RenewalForm.Visibility = Visibility.Collapsed;
                    FormPlaceholder.Text = "This contract is not eligible for renewal at this time";
                    FormPlaceholder.Visibility = Visibility.Visible;
                    Debug.WriteLine("Showing 'not eligible' message");
                }
            }

            Debug.WriteLine("UI updated with contract details");
        }

        /// <summary>
        /// Resets the UI when no contract is selected.
        /// </summary>
        private void ResetUI()
        {
            if (ContractDetailsPanel != null)
            {
                ContractDetailsPanel.Visibility = Visibility.Collapsed;
            }

            if (RenewalForm != null)
            {
                RenewalForm.Visibility = Visibility.Collapsed;
            }

            if (FormPlaceholder != null)
            {
                FormPlaceholder.Text = "Please select a contract to continue";
                FormPlaceholder.Visibility = Visibility.Visible;
            }
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (EndDatePicker?.Date == null)
                {
                    await ShowErrorDialogAsync("Validation Error", "Please select a new end date.");
                    return;
                }

                // Show loading overlay
                if (LoadingOverlay != null)
                {
                    LoadingOverlay.Visibility = Visibility.Visible;
                }

                // Update viewModel with the selected date
                this.viewModel.NewEndDate = EndDatePicker.Date.DateTime;

                // Try to set a reasonable seller ID if it's not already set
                if (this.viewModel.SellerId <= 0)
                {
                    this.viewModel.SellerId = 1; // Use a fallback seller ID
                }

                // Submit the renewal request with timeout protection
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15)))
                {
                    var submitTask = this.viewModel.SubmitRenewalRequestAsync();
                    var completedTask = await Task.WhenAny(submitTask, Task.Delay(15000, cts.Token));

                    if (completedTask == submitTask)
                    {
                        // Task completed successfully
                        await submitTask;

                        // Check if the operation was successful
                        if (this.viewModel.IsSuccessMessage)
                        {
                            await ShowSuccessDialogAsync("Success", this.viewModel.Message);

                            // Reset the UI
                            if (ContractComboBox != null)
                            {
                                ContractComboBox.SelectedItem = null;
                            }
                            ResetUI();
                        }
                        else if (!string.IsNullOrEmpty(this.viewModel.Message))
                        {
                            await ShowErrorDialogAsync("Error", this.viewModel.Message);
                        }
                        else
                        {
                            await ShowErrorDialogAsync("Error", "Unknown error occurred while submitting renewal request");
                        }
                    }
                    else
                    {
                        // Task timed out
                        await ShowErrorDialogAsync("Timeout", "The operation took too long to complete. Please try again.");
                    }
                }
            }
            catch (Exception ex)
            {
                await ShowErrorDialogAsync("Error submitting renewal", $"An error occurred: {ex.Message}");
            }
            finally
            {
                // Hide loading overlay
                if (LoadingOverlay != null)
                {
                    LoadingOverlay.Visibility = Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// Displays an error dialog with the specified title and message.
        /// </summary>
        private async Task ShowErrorDialogAsync(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }

        /// <summary>
        /// Displays a success dialog with the specified title and message.
        /// </summary>
        private async Task ShowSuccessDialogAsync(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}
