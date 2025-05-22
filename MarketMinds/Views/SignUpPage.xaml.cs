// <copyright file="SignUpPage.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketMinds.Views
{
    using MarketMinds.ViewModels;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Input;
    using Microsoft.UI.Xaml.Navigation;

    /// <summary>
    /// Page for user registration.
    /// </summary>
    public sealed partial class SignUpPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SignUpPage"/> class.
        /// </summary>
        public SignUpPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Called when the page is navigated to.
        /// </summary>
        /// <param name="e">Navigation event arguments.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ISignUpViewModel signUpViewModel)
            {
                this.DataContext = signUpViewModel;
            }
        }

        /// <summary>
        /// Event handler for the login link.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void LoginButtonTextBlock_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (this.DataContext is ISignUpViewModel viewModel)
            {
                viewModel.NavigateToLogin?.Invoke();
            }
        }
    }
}
