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

            // Set default phone number prefix
            this.PhoneNumberTextBox.Text = "+40";
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
