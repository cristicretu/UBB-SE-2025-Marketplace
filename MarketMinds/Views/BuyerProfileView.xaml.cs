namespace MarketMinds.Views
{
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using MarketMinds.Shared.Models;
    using MarketMinds.ViewModels;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;

    public sealed partial class BuyerProfileView : Page
    {
        private IContractViewModel? contractViewModel;
        private ITrackedOrderViewModel trackedOrderViewModel;

        public BuyerProfileView()
        {
            // Initialize contract and contractViewModel
            contractViewModel = App.ContractViewModel;

            // Use the static ViewModel from App.xaml.cs
            trackedOrderViewModel = App.TrackedOrderViewModel;

            // Set the ViewModel from the App
            this.ViewModel = App.BuyerProfileViewModel;
            this.ViewModel.User = App.CurrentUser;
            // load in background, will essentially load the ui after the profile page appears
            _ = this.ViewModel.LoadBuyerProfile();

             this.InitializeComponent();
        }

        public IBuyerProfileViewModel? ViewModel { get; set; }

        protected override void OnNavigatedTo(NavigationEventArgs eventArgs)
        {
            base.OnNavigatedTo(eventArgs);
            if (eventArgs.Parameter is IBuyerProfileViewModel viewModel)
            {
                this.ViewModel = viewModel;
            }
        }

        private async Task<int?> ShowTrackedOrderInputDialogAsync()
        {
            var contentDialog = new ContentDialog
            {
                Title = "Enter Tracked Order ID",
                PrimaryButtonText = "Confirm",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.Content.XamlRoot
            };

            TextBox inputTextBox = new TextBox { PlaceholderText = "Enter Tracked Order ID" };
            contentDialog.Content = inputTextBox;

            var result = await contentDialog.ShowAsync();
            bool parseSuccessful = int.TryParse(inputTextBox.Text, out int trackedOrderID);

            if (result == ContentDialogResult.Primary && parseSuccessful)
            {
                return trackedOrderID;
            }

            if (result == ContentDialogResult.Primary && !parseSuccessful)
            {
                return -1;
            }

            return null;
        }

        private async Task ShowNoTrackedOrderDialogAsync(string message)
        {
            var contentDialog = new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };

            await contentDialog.ShowAsync();
        }

        private async void TrackOrderButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (trackedOrderViewModel == null)
            {
                await ShowNoTrackedOrderDialogAsync("Unable to track orders at this time. Please try again later.");
                return;
            }

            var inputID = await ShowTrackedOrderInputDialogAsync();
            if (inputID == null)
            {
                return;
            }

            if (inputID == -1)
            {
                await ShowNoTrackedOrderDialogAsync("Please enter a valid order ID number!");
            }
            else
            {
                int trackedOrderID = (int)inputID.Value;
                try
                {
                    var order = await trackedOrderViewModel.GetTrackedOrderByIDAsync(trackedOrderID);
                    if (order == null)
                    {
                        await ShowNoTrackedOrderDialogAsync($"No tracked order found with ID {trackedOrderID}. Please verify the order ID and try again.");
                        return;
                    }

                    // Determine if the user has control access based on their role
                    bool hasControlAccess = ViewModel.User.UserType == (int)UserRole.Seller || ViewModel.User.UserType == (int)UserRole.Admin;

                    TrackedOrderWindow trackedOrderWindow = new TrackedOrderWindow();
                    if (hasControlAccess)
                    {
                        var trackedOrderControlPage = new TrackedOrderControlPage();
                        trackedOrderControlPage.SetTrackedOrderID(trackedOrderID);
                        trackedOrderWindow.Content = trackedOrderControlPage;
                    }
                    else
                    {
                        var trackedOrderBuyerPage = new TrackedOrderBuyerPage();
                        trackedOrderBuyerPage.SetTrackedOrderID(trackedOrderID);
                        trackedOrderWindow.Content = trackedOrderBuyerPage;
                    }

                    trackedOrderWindow.Activate();
                }
                catch (Exception ex)
                {
                    await ShowNoTrackedOrderDialogAsync($"Unable to retrieve tracked order {trackedOrderID}. Error: {ex.Message}");
                }
            }
        }

        private async Task ShowNoContractDialogAsync()
        {
            var contentDialog = new ContentDialog
            {
                Title = "Error",
                Content = "No Contract has been found.",
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };

            await contentDialog.ShowAsync();
        }

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

        private async Task ShowSuccessDialogAsync(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Success",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }

        private async void GenerateContractButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (long.TryParse(this.contractID.Text, out long contractId))
            {
                try
                {
                    await contractViewModel.GenerateAndSaveContractAsync(contractId);
                    await ShowSuccessDialogAsync("Contract generated and saved successfully.");
                }
                catch (Exception ex)
                {
                    await ShowErrorDialogAsync("Failed to generate contract", ex.Message);
                }
            }
            else
            {
                await ShowErrorDialogAsync("Invalid Input", "The contract ID must be a valid number.");
            }
        }

        private async void RenewContractButton_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                var renewContractWindow = new RenewContractView();
                renewContractWindow.Activate();
            }
            catch (Exception ex)
            {
                await ShowErrorDialogAsync("Error opening Renew Contract", ex.Message);
            }
        }
    }
}
