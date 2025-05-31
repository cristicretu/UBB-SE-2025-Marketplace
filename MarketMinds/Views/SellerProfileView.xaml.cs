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

            // Set default phone number prefix if empty
            if (string.IsNullOrEmpty(this.PhoneNumberTextBox.Text))
            {
                this.PhoneNumberTextBox.Text = "+40";
            }
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

            Debug.WriteLine("Calling UpdateProfile from form");
            
            try
            {
                bool success = await vm.UpdateProfile();
                
                if (success)
                {
                    // Show success dialog
                    var successDialog = new ContentDialog
                    {
                        Title = "Success",
                        Content = "Your profile has been updated successfully!",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };

                    await successDialog.ShowAsync();
                    Debug.WriteLine("Profile update successful - success dialog shown");
                }
                // Note: Error dialogs are already handled in the ViewModel
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error during profile update: {ex.Message}");
                
                var errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"An unexpected error occurred: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await errorDialog.ShowAsync();
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

        /// <summary>
        /// Event handler for phone number text changes to enforce format restrictions.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The event arguments.</param>
        private void PhoneNumberTextBox_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            var textBox = sender;
            var newText = textBox.Text;

            // Ensure text always starts with "+40"
            if (!newText.StartsWith("+40"))
            {
                textBox.Text = "+40";
                textBox.SelectionStart = textBox.Text.Length;
                return;
            }

            // Limit to 12 characters total
            if (newText.Length > 12)
            {
                textBox.Text = newText.Substring(0, 12);
                textBox.SelectionStart = textBox.Text.Length;
                return;
            }

            // Only allow digits after "+40"
            if (newText.Length > 3)
            {
                var digitsOnly = newText.Substring(3);
                var filteredDigits = string.Empty;

                foreach (char c in digitsOnly)
                {
                    if (char.IsDigit(c))
                    {
                        filteredDigits += c;
                    }
                }

                var finalText = "+40" + filteredDigits;
                if (finalText != newText)
                {
                    textBox.Text = finalText;
                    textBox.SelectionStart = textBox.Text.Length;
                }
            }
        }

        /// <summary>
        /// Event handler for when phone number TextBox gets focus.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void PhoneNumberTextBox_GotFocus(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var textBox = sender as TextBox;

            // If text is empty or just "+40", position cursor at the end
            if (string.IsNullOrEmpty(textBox.Text) || textBox.Text == "+40")
            {
                textBox.Text = "+40";
                textBox.SelectionStart = textBox.Text.Length;
            }
            // If cursor is positioned before "+40", move it to after "+40"
            else if (textBox.SelectionStart < 3)
            {
                textBox.SelectionStart = 3;
            }
        }
    }
}
