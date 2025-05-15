// <copyright file="LoginView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MarketPlace924.View
{
    using MarketPlace924.ViewModel;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Input;
    using Microsoft.UI.Xaml.Navigation;

    /// <summary>
    /// Login page for user authentication.
    /// </summary>
    public sealed partial class LoginView : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoginView"/> class.
        /// </summary>
        public LoginView()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the view model for this view.
        /// </summary>
        public ILoginViewModel ViewModel
        {
            get => (ILoginViewModel)this.DataContext;
            set => this.DataContext = value;
        }

        /// <summary>
        /// Called when the page is navigated to.
        /// </summary>
        /// <param name="e">Navigation event arguments.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is ILoginViewModel viewModel)
            {
                this.ViewModel = viewModel;
            }
        }

        /// <summary>
        /// Handles the pointer pressed event on the register text block.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void RegisterButtonTextBlock_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            ISignUpViewModel signUpViewModel = new SignUpViewModel(this.ViewModel.UserService);
            signUpViewModel.NavigateToLogin = () =>
            {
                this.Frame.Navigate(typeof(LoginView), this.ViewModel);
            };
            this.Frame.Navigate(typeof(SignUpPage), signUpViewModel);
        }
    }
}
