// <copyright file="MyMarketView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace MarketPlace924.View
{
    using SharedClassLibrary.Domain;
    using MarketPlace924.ViewModel;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;

    /// <summary>
    /// Represents the view for displaying the market data of a buyer.
    /// </summary>
    public sealed partial class MyMarketView : Page
    {
        /// <summary>
        /// The ViewModel that provides data to this view.
        /// </summary>
        private IMyMarketViewModel? viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyMarketView"/> class.
        /// </summary>
        public MyMarketView()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets the ViewModel associated with this view.
        /// </summary>
        public IMyMarketViewModel? ViewModel => this.viewModel;

        /// <summary>
        /// Called when the page is navigated to.
        /// This method assigns the provided ViewModel to the view's DataContext and
        /// triggers the data refresh.
        /// </summary>
        /// <param name="navigationEvent">Event data for the navigation event.</param>
        protected override void OnNavigatedTo(NavigationEventArgs navigationEvent)
        {
            base.OnNavigatedTo(navigationEvent);

            // If the navigation parameter contains a ViewModel, assign it to the viewModel
            if (navigationEvent.Parameter is MyMarketViewModel viewModel)
            {
                this.viewModel = viewModel;
                this.DataContext = this.viewModel;

                if (this.viewModel != null)
                {
                    // Call the RefreshData method when navigating to the MyMarket page
                    _ = this.viewModel.RefreshData();
                }
            }
        }

        // Handles the text change event in the product search text box.
        private void OnSearchProductTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (this.viewModel != null && textBox != null)
            {
                this.viewModel.FilterProducts(textBox.Text); // Pass the search text to the ViewModel
            }
        }

        // Handles the text change event in the followed seller search text box.
        private void OnSearchFollowedSellerTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (this.viewModel != null && textBox != null)
            {
                this.viewModel.FilterFollowing(textBox.Text); // Pass the search text to the ViewModel
            }
        }

        // Handles the text change event in the all seller search text box.
        private void OnSearchAnySellerTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (this.viewModel != null && textBox != null)
            {
                this.viewModel.FilterAllSellers(textBox.Text); // Pass the search text to the ViewModel
            }
        }

        // Navigates to the seller's profile page.
        private void OnPersonPictureTapped(object sender, RoutedEventArgs e)
        {
            // Get the tapped Seller object (the DataContext of the tapped item)
            var tappedSeller = (sender as FrameworkElement)?.DataContext as Seller;

            if (tappedSeller != null && this.viewModel != null)
            {
                IMyMarketProfileViewModel profileViewModel = new MyMarketProfileViewModel(this.viewModel.BuyerService, this.viewModel.Buyer, tappedSeller);

                // Navigate to the Seller's profile page and pass the seller data
                this.Frame.Navigate(typeof(MyMarketProfileView), profileViewModel);
            }
        }
    }
}
