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
            this.viewModel = App.SellerProfileViewModel;
            this.viewModel.User = App.CurrentUser;
            this.viewModel.LoadProfileAsync();
            this.Loaded += SellerProfileView_Loaded;
            this.InitializeComponent();
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
            var usernameErrorText = new TextBlock
            {
                Text = string.Empty,
                Foreground = new SolidColorBrush(Colors.Red),
                Visibility = Visibility.Collapsed,
                FontSize = 12
            };
            var usernameBox = new TextBox { Text = vm.Username ?? string.Empty, PlaceholderText = "Enter username" };
            usernameBox.LostFocus += (s, args) =>
            {
                if (string.IsNullOrWhiteSpace(usernameBox.Text))
                {
                    usernameBox.BorderBrush = new SolidColorBrush(Colors.Red);
                    usernameErrorText.Text = "Username is required";
                    usernameErrorText.Visibility = Visibility.Visible;
                }
                else
                {
                    usernameBox.BorderBrush = new SolidColorBrush(Colors.Gray);
                    usernameErrorText.Visibility = Visibility.Collapsed;
                }
            };
            usernamePanel.Children.Add(usernameBox);
            usernamePanel.Children.Add(usernameErrorText);
            updateProfileContent.Children.Add(usernamePanel);

            // Store Name
            var storeNamePanel = new StackPanel { Orientation = Orientation.Vertical };
            storeNamePanel.Children.Add(new TextBlock { Text = "Store Name:" });
            var storeNameErrorText = new TextBlock
            {
                Text = string.Empty,
                Foreground = new SolidColorBrush(Colors.Red),
                Visibility = Visibility.Collapsed,
                FontSize = 12
            };
            var storeNameBox = new TextBox { Text = vm.StoreName ?? string.Empty, PlaceholderText = "Enter store name" };
            storeNameBox.LostFocus += (s, args) =>
            {
                if (string.IsNullOrWhiteSpace(storeNameBox.Text))
                {
                    storeNameBox.BorderBrush = new SolidColorBrush(Colors.Red);
                    storeNameErrorText.Text = "Store name is required";
                    storeNameErrorText.Visibility = Visibility.Visible;
                }
                else
                {
                    storeNameBox.BorderBrush = new SolidColorBrush(Colors.Gray);
                    storeNameErrorText.Visibility = Visibility.Collapsed;
                }
            };
            storeNamePanel.Children.Add(storeNameBox);
            storeNamePanel.Children.Add(storeNameErrorText);
            updateProfileContent.Children.Add(storeNamePanel);

            // Email
            var emailPanel = new StackPanel { Orientation = Orientation.Vertical };
            emailPanel.Children.Add(new TextBlock { Text = "Email:" });
            var emailErrorText = new TextBlock
            {
                Text = string.Empty,
                Foreground = new SolidColorBrush(Colors.Red),
                Visibility = Visibility.Collapsed,
                FontSize = 12
            };
            var emailBox = new TextBox { Text = vm.Email ?? string.Empty, PlaceholderText = "Enter email" };
            emailBox.LostFocus += (s, args) =>
            {
                if (string.IsNullOrWhiteSpace(emailBox.Text))
                {
                    emailBox.BorderBrush = new SolidColorBrush(Colors.Red);
                    emailErrorText.Text = "Email is required";
                    emailErrorText.Visibility = Visibility.Visible;
                }
                else if (!IsValidEmail(emailBox.Text))
                {
                    emailBox.BorderBrush = new SolidColorBrush(Colors.Red);
                    emailErrorText.Text = "Please enter a valid email address";
                    emailErrorText.Visibility = Visibility.Visible;
                }
                else
                {
                    emailBox.BorderBrush = new SolidColorBrush(Colors.Gray);
                    emailErrorText.Visibility = Visibility.Collapsed;
                }
            };
            emailPanel.Children.Add(emailBox);
            emailPanel.Children.Add(emailErrorText);
            updateProfileContent.Children.Add(emailPanel);

            // Phone Number
            var phonePanel = new StackPanel { Orientation = Orientation.Vertical };
            phonePanel.Children.Add(new TextBlock { Text = "Phone Number:" });
            var phoneErrorText = new TextBlock
            {
                Text = string.Empty,
                Foreground = new SolidColorBrush(Colors.Red),
                Visibility = Visibility.Collapsed,
                FontSize = 12
            };
            var phoneBox = new TextBox { Text = vm.PhoneNumber ?? string.Empty, PlaceholderText = "Enter phone number" };
            phoneBox.LostFocus += (s, args) =>
            {
                if (string.IsNullOrWhiteSpace(phoneBox.Text))
                {
                    phoneBox.BorderBrush = new SolidColorBrush(Colors.Gray);
                    phoneErrorText.Visibility = Visibility.Collapsed;
                }
                else if (!IsValidPhoneNumber(phoneBox.Text))
                {
                    phoneBox.BorderBrush = new SolidColorBrush(Colors.Red);
                    phoneErrorText.Text = "Please enter a valid phone number";
                    phoneErrorText.Visibility = Visibility.Visible;
                }
                else
                {
                    phoneBox.BorderBrush = new SolidColorBrush(Colors.Gray);
                    phoneErrorText.Visibility = Visibility.Collapsed;
                }
            };
            phonePanel.Children.Add(phoneBox);
            phonePanel.Children.Add(phoneErrorText);
            updateProfileContent.Children.Add(phonePanel);

            // Address
            var addressPanel = new StackPanel { Orientation = Orientation.Vertical };
            addressPanel.Children.Add(new TextBlock { Text = "Address:" });
            var addressErrorText = new TextBlock
            {
                Text = string.Empty,
                Foreground = new SolidColorBrush(Colors.Red),
                Visibility = Visibility.Collapsed,
                FontSize = 12
            };
            var addressBox = new TextBox { Text = vm.StoreAddress ?? string.Empty, PlaceholderText = "Enter address" };
            addressBox.LostFocus += (s, args) =>
            {
                if (string.IsNullOrWhiteSpace(addressBox.Text))
                {
                    addressBox.BorderBrush = new SolidColorBrush(Colors.Red);
                    addressErrorText.Text = "Store address is required";
                    addressErrorText.Visibility = Visibility.Visible;
                }
                else
                {
                    addressBox.BorderBrush = new SolidColorBrush(Colors.Gray);
                    addressErrorText.Visibility = Visibility.Collapsed;
                }
            };
            addressPanel.Children.Add(addressBox);
            addressPanel.Children.Add(addressErrorText);
            updateProfileContent.Children.Add(addressPanel);

            // Description
            var descriptionPanel = new StackPanel { Orientation = Orientation.Vertical };
            descriptionPanel.Children.Add(new TextBlock { Text = "Description:" });
            var descriptionErrorText = new TextBlock
            {
                Text = string.Empty,
                Foreground = new SolidColorBrush(Colors.Red),
                Visibility = Visibility.Collapsed,
                FontSize = 12
            };
            var descriptionBox = new TextBox
            {
                Text = vm.StoreDescription ?? string.Empty,
                PlaceholderText = "Enter description",
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                Height = 100
            };
            descriptionBox.LostFocus += (s, args) =>
            {
                if (string.IsNullOrWhiteSpace(descriptionBox.Text))
                {
                    descriptionBox.BorderBrush = new SolidColorBrush(Colors.Red);
                    descriptionErrorText.Text = "Store description is required";
                    descriptionErrorText.Visibility = Visibility.Visible;
                }
                else if (descriptionBox.Text.Length < 10)
                {
                    descriptionBox.BorderBrush = new SolidColorBrush(Colors.Red);
                    descriptionErrorText.Text = "Description must be at least 10 characters";
                    descriptionErrorText.Visibility = Visibility.Visible;
                }
                else
                {
                    descriptionBox.BorderBrush = new SolidColorBrush(Colors.Gray);
                    descriptionErrorText.Visibility = Visibility.Collapsed;
                }
            };
            descriptionPanel.Children.Add(descriptionBox);
            descriptionPanel.Children.Add(descriptionErrorText);
            updateProfileContent.Children.Add(descriptionPanel);

            dialog.Content = updateProfileContent;

            // Save values back to the view model
            dialog.PrimaryButtonClick += (s, args) =>
            {
                bool isValid = true;
                // Check all validations
                if (string.IsNullOrWhiteSpace(usernameBox.Text))
                {
                    usernameErrorText.Text = "Username is required";
                    usernameErrorText.Visibility = Visibility.Visible;
                    isValid = false;
                }
                if (string.IsNullOrWhiteSpace(storeNameBox.Text))
                {
                    storeNameErrorText.Text = "Store name is required";
                    storeNameErrorText.Visibility = Visibility.Visible;
                    isValid = false;
                }
                if (string.IsNullOrWhiteSpace(emailBox.Text) || !IsValidEmail(emailBox.Text))
                {
                    emailErrorText.Text = string.IsNullOrWhiteSpace(emailBox.Text) ?
                        "Email is required" : "Please enter a valid email address";
                    emailErrorText.Visibility = Visibility.Visible;
                    isValid = false;
                }
                if (!string.IsNullOrWhiteSpace(phoneBox.Text) && !IsValidPhoneNumber(phoneBox.Text))
                {
                    phoneErrorText.Text = "Please enter a valid phone number";
                    phoneErrorText.Visibility = Visibility.Visible;
                    isValid = false;
                }
                if (string.IsNullOrWhiteSpace(addressBox.Text))
                {
                    addressErrorText.Text = "Store address is required";
                    addressErrorText.Visibility = Visibility.Visible;
                    isValid = false;
                }
                if (string.IsNullOrWhiteSpace(descriptionBox.Text) || descriptionBox.Text.Length < 10)
                {
                    descriptionErrorText.Text = string.IsNullOrWhiteSpace(descriptionBox.Text) ?
                        "Store description is required" : "Description must be at least 10 characters";
                    descriptionErrorText.Visibility = Visibility.Visible;
                    isValid = false;
                }
                // Only save if all validations pass
                if (isValid)
                {
                    vm.Username = usernameBox.Text;
                    vm.StoreName = storeNameBox.Text;
                    vm.Email = emailBox.Text;
                    vm.PhoneNumber = phoneBox.Text;
                    vm.StoreAddress = addressBox.Text;
                    vm.StoreDescription = descriptionBox.Text;

                    vm.UpdateProfile();
                }
                else
                {
                    // Prevent dialog from closing if validation fails
                    args.Cancel = true;
                }
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

        private async void OnManageOrderTrackingClick(object sender, RoutedEventArgs e)
        {
            if (this.viewModel == null)
            {
                await ShowErrorDialog("Unable to access order tracking at this time. Please try again later.");
                return;
            }

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
                try
                {
                    // Validate that the tracked order exists
                    var trackedOrder = await App.TrackedOrderViewModel.GetTrackedOrderByIDAsync(trackedOrderID);
                    if (trackedOrder == null)
                    {
                        await ShowErrorDialog($"No tracked order found with ID {trackedOrderID}. Please verify the order ID and try again.");
                        return;
                    }

                    var trackedOrderWindow = new TrackedOrderWindow();
                    var trackedOrderControlPage = new TrackedOrderControlPage();
                    trackedOrderControlPage.SetTrackedOrderID(trackedOrderID);
                    trackedOrderWindow.Content = trackedOrderControlPage;
                    trackedOrderWindow.Activate();
                }
                catch (Exception ex)
                {
                    await ShowErrorDialog($"Error loading tracked order: {ex.Message}");
                }
            }
            else if (result == ContentDialogResult.Primary && !parseSuccessful)
            {
                await ShowErrorDialog("Please enter a valid order ID number!");
            }
        }

        private async Task ShowErrorDialog(string errorMessage)
        {
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = errorMessage,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
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
