// -----------------------------------------------------------------------
// <copyright file="SellerProfileView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace MarketMinds.Views
{
    using System.Diagnostics;
    using System;
    using MarketMinds.ViewModels;
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
            Debug.WriteLine("SellerProfileView constructor called");
            this.Loaded += SellerProfileView_Loaded;
        }

        /// <summary>
        /// Gets the view model associated with the view.
        /// </summary>
        public ISellerProfileViewModel? ViewModel => this.viewModel;

        /// <summary>
        /// Invoked when the page is navigated to.
        /// </summary>
        /// <param name="navigationEventArgs">The navigation event arguments.</param>
        protected override void OnNavigatedTo(NavigationEventArgs navigationEventArgs)
        {
            base.OnNavigatedTo(navigationEventArgs);
            Debug.WriteLine("SellerProfileView.OnNavigatedTo called");

            if (navigationEventArgs.Parameter is ISellerProfileViewModel viewModel)
            {
                this.viewModel = viewModel;
                this.DataContext = viewModel;

                Debug.WriteLine("ViewModel set as DataContext");
                Debug.WriteLine($"ViewModel.Email: {viewModel.Email}");
                Debug.WriteLine($"ViewModel.PhoneNumber: {viewModel.PhoneNumber}");
                Debug.WriteLine($"ViewModel.Address: {viewModel.StoreAddress}");
            }
            else
            {
                Debug.WriteLine($"ERROR: navigationEventArgs.Parameter is not ISellerProfileViewModel, it is: {navigationEventArgs.Parameter?.GetType().Name ?? "null"}");
            }
        }

        /// <summary>
        /// Handles the text changed event for the search box.
        /// </summary>
        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            Debug.WriteLine("SearchTextBox text changed");

            if (sender is TextBox textBox && this.DataContext is ISellerProfileViewModel viewModel)
            {
                viewModel.FilterProducts(textBox.Text);
            }
        }

        /// <summary>
        /// Handles the click event for the sort button.
        /// </summary>
        private void OnSortButtonClick(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is ISellerProfileViewModel viewModel)
            {
                viewModel.SortProducts();
            }
        }

        /// <summary>
        /// Handles the update profile button click and shows a dialog for profile editing.
        /// </summary>
        private async void OnUpdateProfileButtonClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Update Profile button clicked");

            var vm = this.viewModel ?? this.DataContext as ISellerProfileViewModel;

            if (vm == null)
            {
                Debug.WriteLine("ERROR: viewModel is null when attempting to show update profile dialog");

                var errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Could not load profile data. Please try again later.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await errorDialog.ShowAsync();
                return;
            }

            Debug.WriteLine($"Using viewModel with Email: {vm.Email}, Username: {vm.Username}");

            var dialog = new ContentDialog
            {
                Title = "Update Profile",
                PrimaryButtonText = "Save",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var updateProfileContent = new StackPanel { Spacing = 10 };

            // Username field
            var usernamePanel = new StackPanel { Orientation = Orientation.Vertical };
            usernamePanel.Children.Add(new TextBlock { Text = "Username:" });
            var usernameBox = new TextBox { Text = vm.Username ?? string.Empty, PlaceholderText = "Enter username" };
            usernamePanel.Children.Add(usernameBox);
            updateProfileContent.Children.Add(usernamePanel);

            // Store Name
            var storeNamePanel = new StackPanel { Orientation = Orientation.Vertical };
            storeNamePanel.Children.Add(new TextBlock { Text = "Store Name:" });
            var storeNameBox = new TextBox { Text = vm.StoreName ?? string.Empty, PlaceholderText = "Enter store name" };
            storeNamePanel.Children.Add(storeNameBox);
            updateProfileContent.Children.Add(storeNamePanel);

            // Email
            var emailPanel = new StackPanel { Orientation = Orientation.Vertical };
            emailPanel.Children.Add(new TextBlock { Text = "Email:" });
            var emailBox = new TextBox { Text = vm.Email ?? string.Empty, PlaceholderText = "Enter email" };
            emailPanel.Children.Add(emailBox);
            updateProfileContent.Children.Add(emailPanel);

            // Phone Number
            var phonePanel = new StackPanel { Orientation = Orientation.Vertical };
            phonePanel.Children.Add(new TextBlock { Text = "Phone Number:" });
            var phoneBox = new TextBox { Text = vm.PhoneNumber ?? string.Empty, PlaceholderText = "Enter phone number" };
            phonePanel.Children.Add(phoneBox);
            updateProfileContent.Children.Add(phonePanel);

            // Address
            var addressPanel = new StackPanel { Orientation = Orientation.Vertical };
            addressPanel.Children.Add(new TextBlock { Text = "Address:" });
            var addressBox = new TextBox { Text = vm.StoreAddress ?? string.Empty, PlaceholderText = "Enter address" };
            addressPanel.Children.Add(addressBox);
            updateProfileContent.Children.Add(addressPanel);

            // Description
            var descriptionPanel = new StackPanel { Orientation = Orientation.Vertical };
            descriptionPanel.Children.Add(new TextBlock { Text = "Description:" });
            var descriptionBox = new TextBox
            {
                Text = vm.StoreDescription ?? string.Empty,
                PlaceholderText = "Enter description",
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                Height = 100
            };
            descriptionPanel.Children.Add(descriptionBox);
            updateProfileContent.Children.Add(descriptionPanel);

            dialog.Content = updateProfileContent;

            // Save values back to the view model
            dialog.PrimaryButtonClick += (s, args) =>
            {
                vm.Username = usernameBox.Text;
                vm.StoreName = storeNameBox.Text;
                vm.Email = emailBox.Text;
                vm.PhoneNumber = phoneBox.Text;
                vm.StoreAddress = addressBox.Text;
                vm.StoreDescription = descriptionBox.Text;

                vm.UpdateProfile();
            };

            try
            {
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error showing dialog: {ex.Message}");
            }
        }

        /// <summary>
        /// Handler for when the view is fully loaded.
        /// </summary>
        private void SellerProfileView_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("SellerProfileView_Loaded event fired");

            if (this.viewModel == null && this.DataContext is ISellerProfileViewModel vm)
            {
                this.viewModel = vm;
                Debug.WriteLine("Successfully retrieved viewModel from DataContext");
            }
        }
    }
}
