using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using MarketMinds.ViewModels;
using Microsoft.UI.Xaml.Controls;
using MarketMinds.Shared.Models;

namespace MarketMinds.Views
{
    [ExcludeFromCodeCoverage]
    public sealed partial class TrackedOrderBuyerPage : Page
    {
        public int TrackedOrderID { get; set; }
        public int OrderID { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
        public DateTime EstimatedDeliveryDate { get; set; }
        public string DeliveryAddress { get; set; } = string.Empty;
        public ObservableCollection<OrderCheckpoint> Checkpoints { get; set; } = new ObservableCollection<OrderCheckpoint>();

        public TrackedOrderBuyerPage()
        {
            this.InitializeComponent();
            DataContext = App.TrackedOrderViewModel;
        }

        public void SetTrackedOrderID(int trackedOrderID)
        {
            TrackedOrderID = trackedOrderID;

            if (DataContext is TrackedOrderViewModel viewModel)
            {
                _ = viewModel.LoadOrderDataAsync(trackedOrderID);
            }
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
