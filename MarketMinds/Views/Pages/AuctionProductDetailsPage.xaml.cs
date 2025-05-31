using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MarketMinds.Shared.Models;

namespace MarketMinds.Views.Pages
{
    public sealed partial class AuctionProductDetailsPage : Page
    {
        public AuctionProductDetailsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is AuctionProduct product)
            {
                // Load the product details in the UserControl
                _ = DetailsView.ViewModel?.LoadProductAsync(product.Id);
            }
            else if (e.Parameter is int productId)
            {
                // Load the product details by ID
                _ = DetailsView.ViewModel?.LoadProductAsync(productId);
            }
        }
    }
} 