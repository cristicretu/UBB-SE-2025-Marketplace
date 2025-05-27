using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;

namespace MarketMinds.Views
{
    /// <summary>
    /// Interaction logic for the Renew Contract page.
    /// Handles UI logic for selecting and renewing a contract.
    /// </summary>
    public sealed partial class RenewContractView : Page
    {
        private readonly IContractRenewViewModel viewModel;
        private ObservableCollection<IContract> filteredContracts;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenewContractView"/> class.
        /// </summary>
        public RenewContractView()
        {
            this.InitializeComponent();

            // Initialize filtered contracts collection
            this.filteredContracts = new ObservableCollection<IContract>();

            // Get the view model from App like ChatBotPage does
            this.viewModel = App.ContractRenewViewModel;
            this.viewModel.BuyerId = App.CurrentUser.Id;

            // Set the data context for binding
            if (this.RootGrid != null)
            {
                this.RootGrid.DataContext = this.viewModel;
            }

            // Initialize UI state
            this.SetupInitialUIState();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Load contracts when navigating to this page
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
                XamlRoot = this.XamlRoot
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
        /// Loads contracts for the current buyer and updates the ListView.
        /// </summary>
        private async Task LoadContractsAsync()
        {
            try
            {
                Debug.WriteLine("=== STARTING CONTRACT LOAD ===");

                // Show loading indicator
                if (LoadingOverlay != null)
                {
                    LoadingOverlay.Visibility = Visibility.Visible;
                }

                // Clear current selection and UI
                if (ContractListView != null)
                {
                    ContractListView.SelectedItem = null;
                }
                this.ResetUI();

                // Refresh contracts from the view model
                await this.viewModel.RefreshContractsAsync();

                // Small delay to ensure ViewModel has finished updating
                await Task.Delay(100);

                // Update the UI on the UI thread
                this.DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
                {
                    UpdateContractsList();
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR in LoadContractsAsync: {ex.Message}");
                await ShowErrorDialogAsync("Error Loading Contracts", $"Failed to load contracts: {ex.Message}");
            }
            finally
            {
                // Always hide loading indicator
                this.DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
                {
                    if (LoadingOverlay != null)
                    {
                        LoadingOverlay.Visibility = Visibility.Collapsed;
                    }
                });
            }
        }

        /// <summary>
        /// Updates the contracts list in the UI.
        /// </summary>
        private void UpdateContractsList()
        {
            try
            {
                if (ContractListView != null && this.viewModel.Contracts != null)
                {
                    Debug.WriteLine($"Updating UI with {this.viewModel.Contracts.Count} contracts from ViewModel");

                    // Clear and repopulate the filtered contracts
                    this.filteredContracts.Clear();
                    foreach (var contract in this.viewModel.Contracts)
                    {
                        this.filteredContracts.Add(contract);
                    }

                    // Force update the ListView source
                    ContractListView.ItemsSource = null;
                    ContractListView.ItemsSource = this.filteredContracts;

                    Debug.WriteLine($"UI Updated: {this.filteredContracts.Count} contracts displayed in ListView");
                }
                else
                {
                    Debug.WriteLine($"ContractListView is null: {ContractListView == null}, Contracts is null: {this.viewModel.Contracts == null}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR updating contracts list: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the selection change event for the contract ListView.
        /// </summary>
        private async void ContractListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender is ListView listView && listView.SelectedItem is IContract selectedContract)
                {
                    Debug.WriteLine($"=== CONTRACT SELECTED: ID {selectedContract.ContractID} ===");

                    // Set the selected contract in the view model (this triggers LoadContractDetailsAsync)
                    this.viewModel.SelectedContract = selectedContract;

                    // Wait longer for the async loading to complete
                    await Task.Delay(1000);

                    // Ensure we're still on the same contract
                    if (this.viewModel.SelectedContract?.ContractID == selectedContract.ContractID)
                    {
                        // Update UI with the loaded details
                        this.DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
                        {
                            UpdateUIWithContractDetails();
                            Debug.WriteLine($"UI updated for contract ID: {selectedContract.ContractID}");
                        });
                    }
                    else
                    {
                        Debug.WriteLine($"Contract selection changed during loading, skipping UI update");
                    }
                }
                else
                {
                    Debug.WriteLine("No contract selected - resetting UI");
                    this.viewModel.SelectedContract = null;
                    ResetUI();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR in ContractListView_SelectionChanged: {ex.Message}");
                ResetUI();
            }
        }

        /// <summary>
        /// Handles the refresh button click to reload all contracts.
        /// </summary>
        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("=== REFRESH BUTTON CLICKED ===");

                // Clear search box
                if (SearchBox != null)
                {
                    SearchBox.Text = string.Empty;
                }

                // Reload contracts
                await LoadContractsAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR in RefreshButton_Click: {ex.Message}");
                await ShowErrorDialogAsync("Refresh Error", $"Failed to refresh contracts: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the text change event for the search box to filter contracts.
        /// </summary>
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (sender is TextBox searchBox && this.viewModel.Contracts != null)
                {
                    string searchText = searchBox.Text?.ToLower() ?? string.Empty;

                    // Clear and repopulate filtered contracts
                    this.filteredContracts.Clear();

                    var filtered = this.viewModel.Contracts.Where(contract =>
                        string.IsNullOrEmpty(searchText) ||
                        contract.ContractID.ToString().Contains(searchText) ||
                        contract.ContractStatus.ToLower().Contains(searchText) ||
                        contract.OrderID.ToString().Contains(searchText));

                    foreach (var contract in filtered)
                    {
                        this.filteredContracts.Add(contract);
                    }

                    Debug.WriteLine($"Filtered to {this.filteredContracts.Count} contracts");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in SearchBox_TextChanged: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the UI with contract details.
        /// </summary>
        private void UpdateUIWithContractDetails()
        {
            Debug.WriteLine($"=== UPDATING UI WITH CONTRACT DETAILS ===");
            Debug.WriteLine($"StartDate: {this.viewModel.StartDate}");
            Debug.WriteLine($"EndDate: {this.viewModel.EndDate}");
            Debug.WriteLine($"StatusText: {this.viewModel.StatusText}");
            Debug.WriteLine($"IsRenewalAllowed: {this.viewModel.IsRenewalAllowed}");

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

                // Handle different status colors
                var color = this.viewModel.StatusColor switch
                {
                    "Green" => Microsoft.UI.Colors.Green,
                    "Orange" => Microsoft.UI.Colors.Orange,
                    "Gray" => Microsoft.UI.Colors.Gray,
                    _ => Microsoft.UI.Colors.Red
                };

                StatusTextBlock.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(color);
                Debug.WriteLine($"Set status: {StatusTextBlock.Text} with color: {this.viewModel.StatusColor}");
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

                            // Force refresh the contracts list in the UI
                            Debug.WriteLine("=== FORCING UI REFRESH AFTER RENEWAL ===");
                            await LoadContractsAsync();
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
                XamlRoot = this.XamlRoot
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
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}
