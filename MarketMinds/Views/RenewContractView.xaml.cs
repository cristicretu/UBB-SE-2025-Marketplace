using System.Diagnostics.CodeAnalysis;
using System;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using SharedClassLibrary.Shared;
using MarketPlace924.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MarketPlace924
{
    [ExcludeFromCodeCoverage]
    /// <summary>
    /// Interaction logic for the Renew Contract page.
    /// Handles UI logic for selecting and renewing a contract.
    /// </summary>
    public sealed partial class RenewContractView : Window
    {
        private readonly ContractRenewViewModel viewModel;

        private const int BuyerID = 1;
        private const int SellerID = 2;

        /// <summary>
        /// Initializes the RenewContractView, sets up the ViewModel, and loads buyer contracts.
        /// </summary>
        public RenewContractView()
        {
            this.InitializeComponent();
            ContractDetailsPanel.Visibility = Visibility.Collapsed;

            string connectionString = Configuration.CONNECTION_STRING;
            viewModel = new ContractRenewViewModel(connectionString);

            _ = LoadContractsAsync();
        }

        /// <summary>
        /// Loads the contracts for the predefined buyer and binds them to the ComboBox.
        /// </summary>
        private async Task LoadContractsAsync()
        {
            await viewModel.LoadContractsForBuyerAsync(BuyerID);
            ContractComboBox.ItemsSource = viewModel.BuyerContracts;
        }

        /// <summary>
        /// Handles contract selection change, updates contract detail panel and renewal status.
        /// </summary>
        private async void ContractComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContractComboBox.SelectedItem is IContract selected)
            {
                await viewModel.SelectContractAsync(selected.ContractID);

                var dates = await viewModel.GetProductDetailsByContractIdAsync(selected.ContractID);

                if (dates.HasValue)
                {
                    StartDateTextBlock.Text = $"Start Date: {dates.Value.StartDate:MM/dd/yyyy}";
                    EndDateTextBlock.Text = $"End Date: {dates.Value.EndDate:MM/dd/yyyy}";

                    StartDateValueTextBlock.Text = $"{dates.Value.EndDate:MM/dd/yyyy}";

                    bool isValid = await viewModel.IsRenewalPeriodValidAsync();
                    StatusTextBlock.Text = isValid
                        ? "Status: Available (for renewal)"
                        : "Status: Not available (for renewal)";

                    ContractDetailsPanel.Visibility = Visibility.Visible;
                }
            }
            else
            {
                ContractDetailsPanel.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Handles the renewal submission button click event.
        /// Validates user inputs and submits the renewal request through the ViewModel.
        /// </summary>
        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (ContractComboBox.SelectedItem is not IContract selectedContract)
            {
                await ShowMessage("Please select a contract.");
                return;
            }

            if (EndDatePicker.Date == null)
            {
                await ShowMessage("Please select a new end date.");
                return;
            }

            var newEndDate = EndDatePicker.Date.Value.DateTime;

            var (success, message) = await viewModel.SubmitRenewalRequestAsync(newEndDate, BuyerID, selectedContract.OrderID, SellerID);

            await ShowMessage(message);
        }

        /// <summary>
        /// Displays a message in a ContentDialog popup.
        /// </summary>
        private async Task ShowMessage(string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Renewal Status",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}
