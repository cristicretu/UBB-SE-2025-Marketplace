namespace MarketPlace924.View
{
    using System;
    using System.Threading.Tasks;
    using SharedClassLibrary.Domain;
    using SharedClassLibrary.Shared;
    using MarketPlace924.ViewModel;
    using MarketPlace924;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;
    using MarketPlace924.ViewModel;

    public sealed partial class BuyerProfileView : Page
    {
        private IContractViewModel? contractViewModel;
        private ITrackedOrderViewModel? trackedOrderViewModel;

        public BuyerProfileView()
        {
            this.InitializeComponent();

            // Initialize contract and contractViewModel
            contractViewModel = new ContractViewModel(Configuration.CONNECTION_STRING);

            // Initialize trackedOrderViewModel
            trackedOrderViewModel = new TrackedOrderViewModel(Configuration.CONNECTION_STRING);
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
            var bp = new BillingInfo(Constants.OrderHistoryIDBid);
            billingInfoWindow.Content = bp;
            billingInfoWindow.Activate();
        }

        private void WalletRefillButton_Clicked(object sender, RoutedEventArgs e)
        {
            BillingInfoWindow billingInfoWindow = new BillingInfoWindow();
            var bp = new BillingInfo(Constants.OrderHistoryIDWalletRefill);
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
                await contractViewModel.GenerateAndSaveContractAsync();

                var successDialog = new ContentDialog
                {
                    Title = "Success",
                    Content = "Contract generated and saved successfully.",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                await successDialog.ShowAsync();

        }

        private async void BorrowButton_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                int productId = Constants.ProductID;

                var borrowWindow = new BorrowProductWindow(Configuration.CONNECTION_STRING, productId);
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
            int user_id = Constants.CurrentUserID;
            var orderhistorywindow = new OrderHistoryView(Configuration.CONNECTION_STRING, user_id);
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
