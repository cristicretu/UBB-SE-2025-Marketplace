using System;
using MarketMinds.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace MarketMinds.Views
{
    /// <summary>
    /// A page dedicated to managing wishlist and family sync functionality
    /// </summary>
    public sealed partial class WishlistView : Page
    {
        public WishlistView()
        {
            this.InitializeComponent();

            // Set the ViewModel from the App (reusing BuyerProfileViewModel for wishlist and family sync)
            this.ViewModel = App.BuyerProfileViewModel;
            this.ViewModel.User = App.CurrentUser;

            // Load the buyer profile data in background
            _ = this.ViewModel.LoadBuyerProfile();
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
    }
}