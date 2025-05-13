using System;
using System.Threading.Tasks;
using MarketMinds.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Marketplace_SE
{
    public sealed partial class LoginPage : Page
    {
        public LoginViewModel ViewModel { get; set; }
        public LoginPage()
        {
            this.InitializeComponent();
            ViewModel = MarketMinds.App.LoginViewModel;
            this.DataContext = ViewModel;
        }

        private void RevealModeCheckbox_Changed(object sender, RoutedEventArgs routedEventArgse)
        {
            PasswordBoxWithRevealMode.PasswordRevealMode = RevealModeCheckBox.IsChecked == true ? PasswordRevealMode.Visible : PasswordRevealMode.Hidden;
        }

        private void NavigateToSignUpPage(object sender, RoutedEventArgs routedEventArgs)
        {
            Frame.Navigate(typeof(RegisterPage));
        }

        private async void OnLoginButtonClick(object sender, RoutedEventArgs routedEventArgs)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBoxWithRevealMode.Password;
            // Clear previous status
            LoginStatusMessage.Text = string.Empty;
            LoginStatusMessage.Visibility = Visibility.Collapsed;
            // Set login button to loading state
            LoginButton.IsEnabled = false;
            LoginProgressRing.IsActive = true;
            try
            {
                // Use the async method from the ViewModel
                await ViewModel.AttemptLogin(username, password);
                if (ViewModel.LoginStatus)
                {
                    MarketMinds.App.CurrentUser = ViewModel.LoggedInUser;
                    LoginStatusMessage.Text = "You have successfully logged in!";
                    LoginStatusMessage.Visibility = Visibility.Visible;
                    // Navigate to main window
                    MarketMinds.App.ShowMainWindow();
                }
                else
                {
                    LoginStatusMessage.Text = "Invalid username or password.";
                    LoginStatusMessage.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                LoginStatusMessage.Text = $"Login error: {ex.Message}";
                LoginStatusMessage.Visibility = Visibility.Visible;
            }
            finally
            {
                // Reset login button state
                LoginButton.IsEnabled = true;
                LoginProgressRing.IsActive = false;
            }
        }

        private async Task ShowDialog(string title, string content)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}
