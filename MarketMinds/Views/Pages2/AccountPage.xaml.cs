using System;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using MarketMinds.Shared.Models;
using MarketMinds;
using MarketMinds.ViewModels;
using Windows.UI;

namespace Marketplace_SE
{
    public sealed partial class AccountPage : Page
    {
        public AccountPageViewModel ViewModel { get; private set; }
        private ProgressRing loadingRing;
        private const double MINIMUM_BALANCE = 10.0;
        public AccountPage()
        {
            this.InitializeComponent();

            ViewModel = new AccountPageViewModel(App.AccountPageService);
            this.DataContext = ViewModel;

            loadingRing = new ProgressRing
            {
                IsActive = false,
                Width = 50,
                Height = 50,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 20)
            };

            Grid.SetRow(loadingRing, 2);
            ((Grid)this.Content).Children.Add(loadingRing);

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            this.Loaded += AccountPage_Loaded;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.CurrentUser):
                    UpdateUserInfo();
                    break;
                case nameof(ViewModel.Orders):
                    UpdateOrdersUI();
                    break;
                case nameof(ViewModel.IsLoading):
                    loadingRing.IsActive = ViewModel.IsLoading;
                    break;
                case nameof(ViewModel.ErrorMessage):
                    if (!string.IsNullOrEmpty(ViewModel.ErrorMessage))
                    {
                        ShowError(ViewModel.ErrorMessage);
                    }
                    break;
            }
        }

        private async void AccountPage_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.LoadDataCommand.Execute(null);
        }

        private void UpdateUserInfo()
        {
            if (ViewModel.CurrentUser != null)
            {
                var user = ViewModel.CurrentUser;

                UserNameText.Text = user.Username;
                UserEmailText.Text = user.Email;

                double balance = Convert.ToDouble(user.Balance);
                UserBalanceText.Text = $"${balance:F2}";

                UserBalanceText.Foreground = new SolidColorBrush(balance < MINIMUM_BALANCE ? Colors.Red : Colors.Green);
            }
            else
            {
                UserNameText.Text = "Not logged in";
                UserEmailText.Text = "Not available";
                UserBalanceText.Text = "$0.00";
                UserBalanceText.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }

        private void UpdateOrdersUI()
        {
            orderList.Children.Clear();

            if (ViewModel.Orders == null || ViewModel.Orders.Count == 0)
            {
                TextBlock noOrdersText = new TextBlock
                {
                    Text = "No orders found.",
                    Margin = new Thickness(10),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 16
                };
                orderList.Children.Add(noOrdersText);
                return;
            }

            foreach (var order in ViewModel.Orders)
            {
                CreateOrderUI(order);
            }
        }

        private void OnButtonClickNavigateAccountPageMainPage(object sender, RoutedEventArgs e)
        {
            ViewModel.NavigateToMainCommand.Execute(null);

            var mainWindow = new Microsoft.UI.Xaml.Window
            {
                Content = new MainMarketplacePage()
            };
            mainWindow.Activate();
        }

        private void CreateOrderUI(UserOrder order)
        {
            Border orderBorder = new Border
            {
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 5, 0, 5),
                Background = new SolidColorBrush(Color.FromArgb(20, 0, 0, 0)) // Light grayish background
            };

            StackPanel contentPanel = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            bool isBuyOrder = order.BuyerId == ViewModel.CurrentUser?.Id;

            Grid orderHeader = new Grid
            {
                Margin = new Thickness(0, 0, 0, 10)
            };
            orderHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            orderHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            orderHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            Border typeIndicator = new Border
            {
                Background = new SolidColorBrush(isBuyOrder ? Colors.Green : Colors.Red),
                Width = 10,
                Height = 20,
                CornerRadius = new CornerRadius(2),
                Margin = new Thickness(0, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            TextBlock typeLabel = new TextBlock
            {
                Text = isBuyOrder ? "Buy Order" : "Sell Order",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(5, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBlock orderIdText = new TextBlock
            {
                Text = $"Order #{order.Id}",
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.SemiBold
            };

            Grid.SetColumn(typeIndicator, 0);
            Grid.SetColumn(typeLabel, 1);
            Grid.SetColumn(orderIdText, 2);

            orderHeader.Children.Add(typeIndicator);
            orderHeader.Children.Add(typeLabel);
            orderHeader.Children.Add(orderIdText);

            StackPanel detailsPanel = new StackPanel
            {
                Margin = new Thickness(15, 5, 15, 10)
            };

            detailsPanel.Children.Add(new TextBlock
            {
                Text = $"Item: {order.Name}",
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 5)
            });

            detailsPanel.Children.Add(new TextBlock
            {
                Text = $"Description: {order.Description}",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 5)
            });

            detailsPanel.Children.Add(new TextBlock
            {
                Text = $"Price: ${order.Cost:F2}",
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 5)
            });

            detailsPanel.Children.Add(new TextBlock
            {
                Text = $"Status: {order.OrderStatus}",
                Foreground = new SolidColorBrush(Colors.LightGreen),
                Margin = new Thickness(0, 0, 0, 5)
            });

            contentPanel.Children.Add(orderHeader);
            contentPanel.Children.Add(new Rectangle { Height = 1, Fill = new SolidColorBrush(Colors.LightGray), Margin = new Thickness(0, 0, 0, 10) });
            contentPanel.Children.Add(detailsPanel);

            orderBorder.Child = contentPanel;
            orderBorder.Tag = order;

            orderList.Children.Add(orderBorder);
        }

        private async void ShowError(string message)
        {
            ContentDialog errorDialog = new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await errorDialog.ShowAsync();
        }
    }
}
