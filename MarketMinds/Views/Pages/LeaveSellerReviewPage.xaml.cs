using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using MarketMinds.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MarketMinds.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LeaveSellerReviewPage : Page, INotifyPropertyChanged
    {        public event PropertyChangedEventHandler PropertyChanged;

        private ReviewDataModel _reviewData;
        private double _ratingValue;
        private string _reviewText;

        public ReviewDataModel ReviewData 
        { 
            get => _reviewData;
            set => SetProperty(ref _reviewData, value);
        }
        
        public double RatingValue 
        { 
            get => _ratingValue;
            set => SetProperty(ref _ratingValue, value);
        }
        
        public string ReviewText 
        { 
            get => _reviewText;
            set => SetProperty(ref _reviewText, value);
        }

        public SellerReviewViewModel SellerReviewViewModel { get; set; }          public LeaveSellerReviewPage()
        {
            this.InitializeComponent();
            this.DataContext = this; // Set DataContext to enable binding

            // Initialize SellerReviewViewModel with required services from App
            SellerReviewViewModel = new SellerReviewViewModel(
                App.ReviewsService,
                new MarketMinds.Shared.Services.ReviewCalculationService.ReviewCalculationService(),
                App.UserService);
            // Don't set sample data - wait for real data from navigation
            ReviewData = new ReviewDataModel(); // Empty initially
            RatingValue = 0;
            ReviewText = string.Empty;
        }protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is MarketMinds.Views.BuyProductDetailsPage.SellerReviewData data)
            {
                // Update ReviewData properties individually to trigger PropertyChanged events
                ReviewData.ProductTitle = data.ProductTitle;
                ReviewData.SellerName = data.SellerName;

                // Assign SellerId and ProductId directly from the parameter
                SellerReviewViewModel.SellerId = data.SellerId;
                SellerReviewViewModel.ProductId = data.ProductId;
                
                Debug.WriteLine($"LeaveSellerReviewPage: Loaded review data for product '{data.ProductTitle}' by seller '{data.SellerName}'");
            }
            else
            {
                Debug.WriteLine("LeaveSellerReviewPage: No review data provided");
                // Navigate back if no data is provided
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                }
            }
        }

        private async void SubmitReview_Click(object sender, RoutedEventArgs e)
        {
            if (ReviewData == null || SellerReviewViewModel.SellerId == 0 || SellerReviewViewModel.ProductId == 0) return;

            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(ReviewText))
                {
                    var validationDialog = new ContentDialog
                    {
                        Title = "Validation Error",
                        Content = "Please enter your review text.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await validationDialog.ShowAsync();
                    return;
                }

                // Submit the review
                bool success = await SellerReviewViewModel.SubmitReviewAsync(
                    SellerReviewViewModel.SellerId,
                    (int)RatingValue,
                    ReviewText,
                    SellerReviewViewModel.ProductId);

                if (success)
                {
                    var successDialog = new ContentDialog
                    {
                        Title = "Success",
                        Content = "Your review has been submitted. Thank you for your feedback!",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await successDialog.ShowAsync();

                    // Go back to product page
                    if (Frame.CanGoBack)
                    {
                        Frame.GoBack();
                    }
                }
                else
                {
                    throw new Exception("Failed to submit review.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error submitting review: {ex.Message}");

                var errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "An error occurred while submitting your review. Please try again later.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Go back without submitting
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, value))
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }    public class ReviewDataModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _productTitle;
        private string _sellerName;

        public string ProductTitle 
        { 
            get => _productTitle;
            set => SetProperty(ref _productTitle, value);
        }
        
        public string SellerName 
        { 
            get => _sellerName;
            set => SetProperty(ref _sellerName, value);
        }

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, value))
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
