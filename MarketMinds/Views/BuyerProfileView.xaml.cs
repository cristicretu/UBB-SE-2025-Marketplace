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
        public BuyerProfileView()
        {
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

        private async void RenewContractButton_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Frame.Navigate(typeof(RenewContractView));
            }
            catch (Exception ex)
            {
                await ShowErrorDialogAsync("Error opening Renew Contract", ex.Message);
            }
        }
    }
}
