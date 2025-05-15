// <copyright file="MyMarketProfileView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.View
{
    using SharedClassLibrary.Domain;
    using MarketPlace924.ViewModel;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;

    /// <summary>
    /// Represents the view for displaying the profile of a specific seller.
    /// </summary>
    public sealed partial class MyMarketProfileView : Page
    {
        /// <summary>
        /// The view model that provides data and logic for the market profile view.
        /// </summary>
        private IMyMarketProfileViewModel? viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyMarketProfileView"/> class.
        /// </summary>
        public MyMarketProfileView()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets the view model associated with this view.
        /// </summary>
        public IMyMarketProfileViewModel ViewModel => this.viewModel;

        /// <summary>
        /// Called when the page is navigated to.
        /// Sets the view model and data context if a view model is passed as a navigation parameter.
        /// </summary>
        /// <param name="e">The navigation event arguments.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // If the navigation parameter contains a MyMarketProfileViewModel, assign it to viewModel
            if (e.Parameter is MyMarketProfileViewModel viewModel)
            {
                this.viewModel = viewModel;
                this.DataContext = this.viewModel;
            }
        }

        /// <summary>
        /// Handles the click event to navigate back to the "MyMarket" page or the previous page in the navigation stack.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void GoBackToMyMarket(object sender, RoutedEventArgs e)
        {
            // Navigate back to the "MyMarket" page (or the previous page in the navigation stack)
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }

        /// <summary>
        /// Handles the text change event in the product search text box.
        /// Filters the products displayed in the view based on the entered search text.
        /// </summary>
        /// <param name="sender">The text box that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSearchProductTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (this.viewModel != null && textBox != null)
            {
                this.viewModel.FilterProducts(textBox.Text); // Pass the search text to the ViewModel
            }
        }
    }
}
