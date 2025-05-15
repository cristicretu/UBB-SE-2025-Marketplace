using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using MarketPlace924.ViewModel;
using Microsoft.UI.Xaml.Controls;

namespace MarketPlace924.View
{
    [ExcludeFromCodeCoverage]
    public sealed partial class TrackedOrderBuyerPage : Page
    {
        internal ITrackedOrderViewModel ViewModel { get; set; }
        public int TrackedOrderID { get; set; }

        internal TrackedOrderBuyerPage(ITrackedOrderViewModel viewModel, int trackedOrderID)
        {
            this.InitializeComponent();
            ViewModel = viewModel;
            TrackedOrderID = trackedOrderID;
            DataContext = ViewModel;
        }

        private async Task ShowErrorDialog(string errorMessage)
        {
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = errorMessage,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}
