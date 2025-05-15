using System.Diagnostics.CodeAnalysis;
using MarketPlace924.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MarketPlace924
{
    [ExcludeFromCodeCoverage]
    public sealed partial class CardInfo : Page
    {
        /// <summary>
        /// The view model for the CardInfo page
        /// </summary>
        private CardInfoViewModel viewModel;

        public CardInfo(int orderHistoryID)
        {
            this.InitializeComponent();
            viewModel = new CardInfoViewModel(orderHistoryID);
            DataContext = viewModel;
        }

        /// <summary>
        /// Handles the click event for the card button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnPayButtonClickedAsync(object sender, RoutedEventArgs e)
        {
            if (DataContext is CardInfoViewModel viewModel)
            {
                await viewModel.OnPayButtonClickedAsync();
            }
        }
    }
}
