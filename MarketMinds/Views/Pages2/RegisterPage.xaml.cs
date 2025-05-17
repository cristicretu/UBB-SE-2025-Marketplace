using System;
using System.Threading.Tasks;
using System.Diagnostics;
using MarketMinds.Shared.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MarketMinds.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace Marketplace_SE
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RegisterPage : Page
    {
        public RegisterViewModel ViewModel { get; set; }
        public User NewUser { get; set; }

        private const int MINIMUM_PASSWORD_LENGTH = 6;

        public RegisterPage()
        {
            this.InitializeComponent();
            ViewModel = MarketMinds.App.RegisterViewModel;
            this.DataContext = ViewModel;
            NewUser = new User(0, string.Empty, string.Empty, string.Empty);
        }

        private async void OnCreateUserClick(object sender, RoutedEventArgs routedEventArgs)
        {
            // Clear any previous validation messages
            UsernameValidationTextBlock.Text = string.Empty;
            ConfirmPasswordValidationTextBlock.Text = string.Empty;
            // Set loading state
            CreateAccountButton.IsEnabled = false;
            RegisterProgressRing.IsActive = true;
            try
            {
                NewUser.Username = UsernameTextBox.Text;
                NewUser.Email = EmailTextBox.Text;
                NewUser.Password = PasswordBoxWithRevealMode.Password;
                string confirmPassword = ConfirmPasswordBox.Password;
                // Client-side validation
                if (!ViewModel.IsValidUsername(NewUser.Username))
                {
                    UsernameValidationTextBlock.Text = "Username must be 5-20 characters and contain only letters, digits, or underscores.";
                    return;
                }

                // Check if username is already taken
                if (await ViewModel.IsUsernameTaken(NewUser.Username))
                {
                    await ShowDialog("Username Taken", "This username is already in use. Please choose another.");
                    return;
                }

                // Password strength validation
                string passwordStrength = ViewModel.GetPasswordStrength(PasswordBoxWithRevealMode.Password);
                if (passwordStrength == "Weak")
                {
                    await ShowDialog("Weak Password", "Password must be at least Medium strength. Include an uppercase letter, a special character, and a digit.");
                    return;
                }

                // Password confirmation validation
                if (!ViewModel.PasswordsMatch(NewUser.Password, confirmPassword))
                {
                    ConfirmPasswordValidationTextBlock.Text = "Passwords do not match.";
                    return;
                }

                // Attempt to create the user
                User registeredUser = await ViewModel.CreateNewUser(NewUser);
                if (registeredUser != null)
                {
                    // Store the *returned* user (with ID and other server-set details) in the app context
                    MarketMinds.App.CurrentUser = registeredUser;
                    await ShowDialog("Account Created", "Your account has been successfully created!");
                    // Optionally, navigate to login or directly to the main app page
                    // For now, navigating to LoginPage to ensure the login flow also works with the new user
                    Frame.Navigate(typeof(LoginPage));
                }
                else
                {
                    await ShowDialog("Error", "Failed to create account. Please try again or check details.");
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"[RegisterPage] Inner exception: {ex.InnerException.Message}");
                }
                await ShowDialog("Error", $"An error occurred: {ex.Message}");
            }
            finally
            {
                // Reset loading state
                CreateAccountButton.IsEnabled = true;
                RegisterProgressRing.IsActive = false;
            }
        }

        private void PasswordBoxWithRevealMode_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordStrengthTextBlock.Text = ViewModel.GetPasswordStrength(PasswordBoxWithRevealMode.Password);
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

        private void RevealModeCheckbox_Changed(object sender, RoutedEventArgs routedEventArgs)
        {
            PasswordBoxWithRevealMode.PasswordRevealMode = RevealModeCheckBox.IsChecked == true ? PasswordRevealMode.Visible : PasswordRevealMode.Hidden;
            ConfirmPasswordBox.PasswordRevealMode = RevealModeCheckBox.IsChecked == true ? PasswordRevealMode.Visible : PasswordRevealMode.Hidden;
        }

        private void NavigateToLoginPage(object sender, RoutedEventArgs routedEventArgs)
        {
            Frame.Navigate(typeof(LoginPage));
        }
    }
}
