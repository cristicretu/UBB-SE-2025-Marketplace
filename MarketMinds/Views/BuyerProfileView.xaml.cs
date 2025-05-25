namespace MarketMinds.Views
{
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using MarketMinds.ViewModels;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;

    public sealed partial class BuyerProfileView : Page
    {
        public IContractViewModel contractViewModel { get; private set; }
        private ITrackedOrderViewModel? trackedOrderViewModel;

        public BuyerProfileView()
        {
            this.InitializeComponent();

            // Initialize contractViewModel
            contractViewModel = new ContractViewModel();

            // Initialize trackedOrderViewModel
            trackedOrderViewModel = new TrackedOrderViewModel();
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

        private void MyCartButton_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create a new window for the cart view
                var cartWindow = new Window();

                // Create the content for the window
                var cartView = new MyCartView();

                // Set the content of the window to the cart view
                cartWindow.Content = cartView;

                // Set a title for the window (optional)
                cartWindow.Title = "My Shopping Cart";

                // Activate (show) the window
                cartWindow.Activate();
            }
            catch (Exception ex)
            {
                // Show error message if something goes wrong
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Unable to open cart: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                _ = dialog.ShowAsync();
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
            var inputID = await ShowTrackedOrderInputDialogAsync();
            if (inputID == null)
            {
                return;
            }

            if (inputID == -1)
            {
                await ShowNoTrackedOrderDialogAsync("Please enter an integer!");
            }
            else
            {
                int trackedOrderID = (int)inputID.Value;
                try
                {
                    var order = await trackedOrderViewModel.GetTrackedOrderByIDAsync(trackedOrderID);
                    bool hasControlAccess = true;

                    TrackedOrderWindow trackedOrderWindow = new TrackedOrderWindow();
                    if (hasControlAccess)
                    {
                        var controlp = new TrackedOrderControlPage(trackedOrderViewModel, trackedOrderID);
                        trackedOrderWindow.Content = controlp;
                    }
                    else
                    {
                        var buyerp = new TrackedOrderBuyerPage(trackedOrderViewModel, trackedOrderID);
                        trackedOrderWindow.Content = buyerp;
                    }

                    trackedOrderWindow.Activate();
                }
                catch (Exception)
                {
                    await ShowNoTrackedOrderDialogAsync("No TrackedOrder has been found with ID " + trackedOrderID.ToString());
                }
            }
        }

        private void BidProductButton_Clicked(object sender, RoutedEventArgs e)
        {
            BillingInfoWindow billingInfoWindow = new BillingInfoWindow();
            // merge-nicusor FIX :)
            var bp = new BillingInfo();
            billingInfoWindow.Content = bp;
            billingInfoWindow.Activate();
        }

        private void WalletRefillButton_Clicked(object sender, RoutedEventArgs e)
        {
            BillingInfoWindow billingInfoWindow = new BillingInfoWindow();
            // merge-nicusor FIX :)
            var bp = new BillingInfo();
            billingInfoWindow.Content = bp;
            billingInfoWindow.Activate();
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

        private async void GenerateContractButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (long.TryParse(this.contractID.Text, out long contractId))
            {
                await contractViewModel.GenerateAndSaveContractAsync(contractId);

                // Check if the ViewModel set an error message
                if (string.IsNullOrEmpty(contractViewModel.GenerateContractErrorMessage))
                {
                    var successDialog = new ContentDialog
                    {
                        Title = "Success",
                        Content = "Contract generated and saved successfully.",
                        CloseButtonText = "OK",
                        XamlRoot = this.Content.XamlRoot
                    };
                    await successDialog.ShowAsync();
                }
                else
                {
                    var successDialog = new ContentDialog
                    {
                        Title = "Fatal fumble",
                        Content = contractViewModel.GenerateContractErrorMessage,
                        CloseButtonText = "I shall proceed",
                        XamlRoot = this.Content.XamlRoot
                    };
                    await successDialog.ShowAsync();
                }
                // If contractViewModel.GenerateContractErrorMessage is not empty,
                // the TextBlock in XAML bound to it will display the error.
            }
            else
            {
                await ShowErrorDialogAsync("Invalid Input", "The contract ID must be a valid number.");
            }
        }

        private async void BorrowButton_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                // merge-nicusor FIX :)
                int productId = 1;

                var borrowWindow = new BorrowProductWindow(productId);
                borrowWindow.Activate();
            }
            catch (Exception ex)
            {
                await ShowErrorDialogAsync("Failed to open Borrow Product", ex.Message);
            }
        }

        private void NotificationButton_Clicked(object sender, RoutedEventArgs e)
        {
            MainNotificationWindow mainNotificationWindow = new MainNotificationWindow();
            mainNotificationWindow.Activate();
        }

        private void OrderHistoryButton_Clicked(object sender, RoutedEventArgs e)
        {
            // merge-nicusor FIX :)
            int user_id = 1;
            var orderhistorywindow = new OrderHistoryView(user_id);
            orderhistorywindow.Activate();
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
