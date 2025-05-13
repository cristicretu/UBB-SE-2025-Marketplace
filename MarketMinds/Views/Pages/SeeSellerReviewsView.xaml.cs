using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
// using Microsoft.UI.Xaml.Controls.Primitives;
// using Microsoft.UI.Xaml.Data;
// using Microsoft.UI.Xaml.Input;
// using Microsoft.UI.Xaml.Media;
// using Microsoft.UI.Xaml.Navigation;
// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Runtime.InteropServices.WindowsRuntime;
// using Windows.Foundation;
// using Windows.Foundation.Collections;
using BusinessLogicLayer.ViewModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace MarketMinds.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SeeSellerReviewsView : Page
    {
        private SeeSellerReviewsViewModel viewModel;
        private const int NO_REVIEWS = 0;
        // Public property for binding
        public SeeSellerReviewsViewModel ViewModel => viewModel;
        public SeeSellerReviewsView(SeeSellerReviewsViewModel sellerReviewsViewModel)
        {
            viewModel = sellerReviewsViewModel;
            viewModel.RefreshData();
            this.InitializeComponent();
            // Show/hide elements based on review count
            ReviewsListView.Visibility = viewModel.Reviews.Count > NO_REVIEWS ? Visibility.Visible : Visibility.Collapsed;
            EmptyMessageTextBlock.Visibility = viewModel.Reviews.Count == NO_REVIEWS ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
