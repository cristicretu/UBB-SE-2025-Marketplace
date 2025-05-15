// -----------------------------------------------------------------------
// <copyright file="SellerProfileView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace MarketPlace924.View
{
    using MarketPlace924.ViewModel;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;

    /// <summary>
    /// Seller profile view page.
    /// </summary>
    public sealed partial class SellerProfileView : Page
    {
        /// <summary>
        /// The view model for this view.
        /// </summary>
        private ISellerProfileViewModel? viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="SellerProfileView"/> class.
        /// </summary>
        public SellerProfileView()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets the view model associated with the view.
        /// </summary>
        public ISellerProfileViewModel? ViewModel => this.viewModel;

        /// <summary>
        /// Invoked when the page is loaded.
        /// </summary>
        /// <param name="navigationEventArgs">The navigation event arguments.</param>
        protected override void OnNavigatedTo(NavigationEventArgs navigationEventArgs)
        {
            base.OnNavigatedTo(navigationEventArgs);
            if (navigationEventArgs.Parameter is ISellerProfileViewModel viewModel)
            {
                this.viewModel = viewModel;
                this.DataContext = viewModel;
            }
        }

        /// <summary>
        /// Handles the text changed event for the search box.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="textChangedEventArguments">The text changed event arguments.</param>
        private void OnSearchTextChanged(object sender, TextChangedEventArgs textChangedEventArguments)
        {
            var textBox = sender as TextBox;
            var viewModel = this.DataContext as ISellerProfileViewModel;
            if (viewModel != null && textBox != null)
            {
                viewModel.FilterProducts(textBox.Text);
            }
        }

        /// <summary>
        /// Handles the click event for the update profile button.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="routedEventArguments">The routed event arguments.</param>
        private void OnUpdateProfileButtonClick(object sender, RoutedEventArgs routedEventArguments)
        {
            var viewModel = this.DataContext as ISellerProfileViewModel;
            if (viewModel != null)
            {
                this.Frame.Navigate(typeof(UpdateProfileView), viewModel);
            }
        }

        /// <summary>
        /// Handles the click event for the sort button.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="routedEventArguments">The routed event arguments.</param>
        private void OnSortButtonClick(object sender, RoutedEventArgs routedEventArguments)
        {
            var viewModel = this.DataContext as ISellerProfileViewModel;
            if (viewModel != null)
            {
                viewModel.SortProducts();
            }
        }
    }
}