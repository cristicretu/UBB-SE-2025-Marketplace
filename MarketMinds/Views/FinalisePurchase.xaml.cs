using System.Diagnostics.CodeAnalysis;
using MarketMinds.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MarketMinds.Views
{
    [ExcludeFromCodeCoverage]

    public sealed partial class FinalisePurchase : Page
    {
        /// <summary>
        /// The view model for the FinalisePurchase page
        /// </summary>
        private FinalizePurchaseViewModel viewModel;
        private NotificationViewModel notifViewModel;
        public FinalisePurchase(int orderHistoryID)
        {
            this.InitializeComponent();
            viewModel = new FinalizePurchaseViewModel(orderHistoryID);
            notifViewModel = new NotificationViewModel(1);
            DataContext = viewModel;
        }

        /// <summary>
        /// Handles the click event for the continue shopping button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnContinueShopping_Clicked(object sender, RoutedEventArgs e)
        {
            viewModel.HandleFinish();
        }
    }
}
