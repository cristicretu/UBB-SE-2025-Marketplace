// -----------------------------------------------------------------------
// <copyright file="SellerProfileView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace MarketMinds.Views
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using MarketMinds.ViewModels;
    using Microsoft.UI;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Media;
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
            Debug.WriteLine("SellerProfileView constructor called");
            this.InitializeComponent();
            this.viewModel = App.SellerProfileViewModel;
            this.viewModel.User = App.CurrentUser;
            this.DataContext = this.viewModel; // Set DataContext so bindings work
            this.Loaded += SellerProfileView_Loaded;
            
            // Load profile data asynchronously after UI is initialized
            _ = LoadProfileDataAsync();
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
        /// Handles the save changes button click.
        /// </summary>
        private async void OnUpdateProfileButtonClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Save Changes button clicked");

            var vm = this.viewModel ?? this.DataContext as ISellerProfileViewModel;

            if (vm == null)
            {
                Debug.WriteLine("ERROR: viewModel is null when attempting to save profile");

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

            Debug.WriteLine("Calling UpdateProfile directly from form");
            vm.UpdateProfile();
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

        /// <summary>
        /// Loads profile data asynchronously after the UI is initialized.
        /// </summary>
        private async Task LoadProfileDataAsync()
        {
            try
            {
                // await Task.Delay(100); // Small delay to ensure UI is ready
                await this.viewModel.LoadProfileAsync();
                Debug.WriteLine("Profile data loading completed");
                
                // Debug the RatingControl after loading
                await Task.Delay(500); // Small delay to ensure UI updates
                this.DebugRatingControl();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading profile data: {ex.Message}");
            }
        }

        /// <summary>
        /// Debug method to check RatingControl state.
        /// </summary>
        private void DebugRatingControl()
        {
            try
            {
                if (this.FindName("TrustScoreRating") is RatingControl ratingControl)
                {
                    Debug.WriteLine($"RatingControl found - Value: {ratingControl.Value}");
                    Debug.WriteLine($"RatingControl - IsReadOnly: {ratingControl.IsReadOnly}");
                    Debug.WriteLine($"RatingControl - PlaceholderValue: {ratingControl.PlaceholderValue}");
                    
                    // Try to force update
                    if (this.viewModel != null)
                    {
                        Debug.WriteLine($"ViewModel TrustScore: {this.viewModel.TrustScore}");
                        Debug.WriteLine($"ViewModel RatingValue: {this.viewModel.RatingValue}");
                        
                        // Force set the value directly as a test
                        ratingControl.Value = this.viewModel.RatingValue;
                        Debug.WriteLine($"Manually set RatingControl.Value to: {this.viewModel.RatingValue}");
                    }
                }
                else
                {
                    Debug.WriteLine("TrustScoreRating RatingControl not found!");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in DebugRatingControl: {ex.Message}");
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            // Simple regex for phone validation
            return System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, @"^\+?[0-9\s\-\(\)]{6,20}$");
        }
    }
}
