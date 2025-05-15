using System.Threading.Tasks;
using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MarketMinds.Shared.Models;
using Marketplace_SE.Utilities; // For Notification, FrameNavigation
using MarketMinds.ViewModels; // For MarketplaceViewModel
using MarketMinds;
using MarketMinds.Views.Pages2;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info
namespace Marketplace_SE
{
    public sealed partial class MainMarketplacePage : Page
    {
        private readonly MainMarketplaceViewModel mainMarketplaceViewModel;
        private User currentUser;

        public MainMarketplacePage()
        {
            this.InitializeComponent();

            mainMarketplaceViewModel = App.MainMarketplaceViewModel;
            currentUser = App.CurrentUser;
        }

        protected override void OnNavigatedTo(NavigationEventArgs routedEventArgs)
        {
            base.OnNavigatedTo(routedEventArgs);
            LoadAvailableItems();
        }

        private void LoadAvailableItems()
        {
            LoadingIndicator.IsActive = true;
            ItemsListView.ItemsSource = null;

            try
            {
                if (mainMarketplaceViewModel != null)
                {
                    var items = mainMarketplaceViewModel.GetAvailableItems();
                    ItemsListView.ItemsSource = items;
                }
                else
                {
                    ShowNotification("Error", "Failed to load items.");
                }
            }
            catch (Exception itemsLoadException)
            {
                ShowNotification("Error", $"An error occurred while loading items: {itemsLoadException.Message}");
            }
            finally
            {
                LoadingIndicator.IsActive = false;
            }
        }

        private void OpenAccountButton_Click(object sender, RoutedEventArgs e)
        {
            var accountWindow = new Window();
            accountWindow.Content = new AccountPage();
            accountWindow.Activate();
        }

        private void OpenHelpButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            var helpWindow = new Window();
            helpWindow.Content = new GetHelpPage();
            helpWindow.Activate();
        }

        private async Task ShowNotification(string title, string message)
        {
            Notification notification = new Notification(title, message);
            var window = notification.GetWindow();

            if (window != null)
            {
                window.Activate();
            }
            else
            {
                var dialog = new ContentDialog
                {
                    Title = title,
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }
    }
}