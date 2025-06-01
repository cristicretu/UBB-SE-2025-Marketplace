using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using BusinessLogicLayer.ViewModel;

namespace MarketMinds.Views.Pages
{
    /// <summary>
    /// Page that displays reviews received by the current seller
    /// </summary>
    public sealed partial class MyReviewsView : Page
    {
        private SeeSellerReviewsViewModel viewModel;
        private const int NO_REVIEWS = 0;

        // Public property for binding
        public SeeSellerReviewsViewModel ViewModel => viewModel;

        public MyReviewsView()
        {
            // Initialize with current user as seller
            if (App.CurrentUser != null && App.SeeSellerReviewsViewModel != null)
            {
                // Set the current user as the seller to view their own reviews
                App.SeeSellerReviewsViewModel.Seller = App.CurrentUser;
                App.SeeSellerReviewsViewModel.Viewer = App.CurrentUser;

                viewModel = App.SeeSellerReviewsViewModel;
                viewModel.RefreshData();
            }
            else
            {
                // Create a fallback ViewModel if none exists
                viewModel = new SeeSellerReviewsViewModel(
                    App.ReviewsService,
                    App.CurrentUser ?? new MarketMinds.Shared.Models.User(),
                    App.CurrentUser ?? new MarketMinds.Shared.Models.User());
            }

            this.InitializeComponent();

            // Show/hide elements based on review count
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            if (viewModel?.Reviews != null)
            {
                ReviewsListView.Visibility = viewModel.Reviews.Count > NO_REVIEWS ? Visibility.Visible : Visibility.Collapsed;
                EmptyStatePanel.Visibility = viewModel.Reviews.Count == NO_REVIEWS ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Refresh data when page loads
            viewModel?.RefreshData();
            UpdateVisibility();
        }
    }
}
