// <copyright file="UpdateProfileView.xaml.cs" company="YourCompanyName">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.View
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using MarketPlace924.ViewModel;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;

    /// <summary>
    /// Page for updating user profile information.
    /// </summary>
    public sealed partial class UpdateProfileView : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateProfileView"/> class.
        /// </summary>
        public UpdateProfileView()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the view model for this view.
        /// </summary>
        public ISellerProfileViewModel ViewModel
        {
            get => (ISellerProfileViewModel)this.DataContext;
            set => this.DataContext = value;
        }

        /// <summary>
        /// Called when the page is navigated to.
        /// </summary>
        /// <param name="e">Navigation event arguments.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is ISellerProfileViewModel viewModel)
            {
                this.DataContext = viewModel;
            }
        }

        /// <summary>
        /// Handles the save button click event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private async void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            List<string> validationErrors = this.ViewModel.ValidateFields();

            if (validationErrors.Any())
            {
                string errorMessage = string.Join("\n", validationErrors);
                await this.ShowDialog("Validation Errors", errorMessage);
            }
            else
            {
                this.ViewModel.UpdateProfile();
            }
        }

        /// <summary>
        /// Handles the back button click event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void BackButtonClick(object sender, RoutedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }

        /// <summary>
        /// Shows a dialog with the given title and message.
        /// </summary>
        /// <param name="title">The dialog title.</param>
        /// <param name="message">The dialog message.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ShowDialog(string title, string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = App.MainWindow?.Content.XamlRoot,
            };

            await dialog.ShowAsync();
        }
    }
}
