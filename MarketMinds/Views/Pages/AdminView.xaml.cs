using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ViewModelLayer.ViewModel;
using MarketMinds.Shared.Models;

namespace MarketMinds.Views
{
    /// <summary>
    /// Interaction logic for AdminView.xaml
    /// </summary>
    public sealed partial class AdminView : Window
    {
        private bool showingFirstPage = true;

        public AdminView()
        {
            InitializeComponent();
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            showingFirstPage = !showingFirstPage;

            // Toggle visibility of pages
            ProductsPage.Visibility = showingFirstPage ? Visibility.Visible : Visibility.Collapsed;
            UsersPage.Visibility = showingFirstPage ? Visibility.Collapsed : Visibility.Visible;

            // Toggle arrow visibility
            RightArrow.Visibility = showingFirstPage ? Visibility.Visible : Visibility.Collapsed;
            LeftArrow.Visibility = showingFirstPage ? Visibility.Collapsed : Visibility.Visible;
        }

        private void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            App.ResetLoginState();
            App.LoginWindow = new LoginWindow();
            this.Close();
            App.LoginWindow.Activate();
        }
    }
}
