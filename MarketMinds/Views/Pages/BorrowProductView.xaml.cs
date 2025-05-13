using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Diagnostics;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MarketMinds.Shared.Models;
using Microsoft.UI.Xaml.Media.Imaging;
using ViewModelLayer.ViewModel;
using MarketMinds.Views.Pages;

namespace MarketMinds
{
    public sealed partial class BorrowProductView : Window
    {
        public BorrowProduct Product { get; private set; }
        private Window? seeSellerReviewsView;
        private Window? leaveReviewWindow;
        private readonly User currentUser;
        public DateTime? SelectedEndDate { get; private set; }

        private const int IMAGE_HEIGHT = 250;  // magic numbers removal
        private const int TEXT_BLOCK_MARGIN = 4;
        private const int TEXT_BLOCK_PADDING_LEFT = 8;
        private const int TEXT_BLOCK_PADDING_TOP = 4;
        private const int TEXT_BLOCK_PADDING_RIGHT = 8;
        private const int TEXT_BLOCK_PADDING_BOTTOM = 4;

        public BorrowProductView(BorrowProduct product)
        {
            Debug.WriteLine($"[BorrowProductView] Constructor - Product: {product?.Title ?? "null"}");
            Debug.WriteLine($"[BorrowProductView] Product StartDate: {product?.StartDate.ToString() ?? "null"}");
            Debug.WriteLine($"[BorrowProductView] Product TimeLimit: {product?.TimeLimit.ToString() ?? "null"}");

            this.InitializeComponent();
            Product = product;
            currentUser = MarketMinds.App.CurrentUser;

            // Initialize date controls
            Debug.WriteLine("[BorrowProductView] Initializing date controls");
            try
            {
                // Ensure valid date range
                if (Product.StartDate.HasValue && Product.StartDate.Value > Product.TimeLimit)
                {
                    Debug.WriteLine("[BorrowProductView] Warning: StartDate is after TimeLimit, swapping dates");
                    var temp = Product.StartDate;
                    Product.StartDate = Product.TimeLimit;
                    Product.TimeLimit = temp.Value;
                }

                StartDateTextBlock.Text = Product.StartDate.HasValue
                    ? Product.StartDate.Value.ToString("d")
                    : DateTime.Now.ToString("d");

                TimeLimitTextBlock.Text = Product.TimeLimit.ToString("d");
                // Set the DatePicker's range
                EndDatePicker.MinDate = Product.StartDate ?? DateTime.Now;
                EndDatePicker.MaxDate = new DateTimeOffset(Product.TimeLimit);
                Debug.WriteLine($"[BorrowProductView] Date controls initialized - StartDate: {StartDateTextBlock.Text}, TimeLimit: {TimeLimitTextBlock.Text}");
                Debug.WriteLine($"[BorrowProductView] DatePicker range set - Min: {EndDatePicker.MinDate}, Max: {EndDatePicker.MaxDate}");
            }
            catch (Exception borrowProductViewException)
            {
                Debug.WriteLine($"[BorrowProductView] Error initializing date controls: {borrowProductViewException.Message}");
                Debug.WriteLine($"[BorrowProductView] Stack trace: {borrowProductViewException.StackTrace}");
            }

            LoadProductDetails();
            LoadImages();
        }

        private void LoadProductDetails()
        {
            // Basic Info
            TitleTextBlock.Text = Product.Title;
            CategoryTextBlock.Text = Product.Category.DisplayTitle;
            ConditionTextBlock.Text = Product.Condition.DisplayTitle;

            // Seller Info
            SellerTextBlock.Text = Product.Seller.Username;
            DescriptionTextBox.Text = Product.Description;

            // Tags
            TagsItemsControl.ItemsSource = Product.Tags.Select(tag =>
            {
                return new TextBlock
                {
                    Text = tag.DisplayTitle,
                    Margin = new Thickness(TEXT_BLOCK_MARGIN),
                    Padding = new Thickness(TEXT_BLOCK_PADDING_LEFT, TEXT_BLOCK_PADDING_TOP, TEXT_BLOCK_PADDING_RIGHT, TEXT_BLOCK_PADDING_BOTTOM)
                };
            }).ToList();
        }

        private void OnJoinWaitListClicked(object sender, RoutedEventArgs routedEventArgs)
        {
        }

        private void OnLeaveWaitListClicked(object sender, RoutedEventArgs routedEventArgs)
        {
        }

        private void LoadImages()
        {
            ImageCarousel.Items.Clear();
            foreach (var image in Product.Images)
            {
                var newImage = new Microsoft.UI.Xaml.Controls.Image
                {
                    Source = new BitmapImage(new Uri(image.Url)),
                    Stretch = Stretch.Uniform,
                    Height = IMAGE_HEIGHT,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center
                };

                ImageCarousel.Items.Add(newImage);
            }
        }

        private void OnSeeReviewsClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            if (App.SeeSellerReviewsViewModel != null)
            {
                App.SeeSellerReviewsViewModel.Seller = Product.Seller;
                // Create a window to host the SeeSellerReviewsView page
                var window = new Window();
                window.Content = new SeeSellerReviewsView(App.SeeSellerReviewsViewModel);
                window.Activate();
                // Store reference to window
                seeSellerReviewsView = window;
            }
            else
            {
                ShowErrorDialog("Cannot view reviews at this time. Please try again later.");
            }
        }

        private void OnLeaveReviewClicked(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser != null)
            {
                if (App.ReviewCreateViewModel != null)
                {
                    App.ReviewCreateViewModel.Seller = Product.Seller;

                    leaveReviewWindow = new CreateReviewView(App.ReviewCreateViewModel);
                    leaveReviewWindow.Activate();
                }
                else
                {
                    ShowErrorDialog("Cannot create review at this time. Please try again later.");
                }
            }
            else
            {
                ShowErrorDialog("You must be logged in to leave a review.");
            }
        }

        private async void ShowErrorDialog(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }

        private void EndDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs calendarDatePickerChangedEventArgs)
        {
            Debug.WriteLine("[EndDatePicker] DateChanged event started");
            Debug.WriteLine($"[EndDatePicker] Sender null? {sender == null}");
            Debug.WriteLine($"[EndDatePicker] Args null? {calendarDatePickerChangedEventArgs == null}");
            Debug.WriteLine($"[EndDatePicker] Product null? {Product == null}");
            if (sender == null)
            {
                Debug.WriteLine("[EndDatePicker] Error: sender is null");
                return;
            }

            Debug.WriteLine($"[EndDatePicker] Current EndDatePicker.Date: {sender.Date?.ToString() ?? "null"}");
            Debug.WriteLine($"[EndDatePicker] Product.StartDate: {Product?.StartDate.ToString() ?? "null"}");
            Debug.WriteLine($"[EndDatePicker] Product.TimeLimit: {Product?.TimeLimit.ToString() ?? "null"}");

            try
            {
                if (EndDatePicker.Date != null)
                {
                    SelectedEndDate = EndDatePicker.Date.Value.DateTime;
                    Debug.WriteLine($"[EndDatePicker] Selected date set to: {SelectedEndDate}");
                    CalculatePriceButton.IsEnabled = true;
                }
                else
                {
                    SelectedEndDate = null;
                    Debug.WriteLine("[EndDatePicker] Selected date cleared");
                    CalculatePriceButton.IsEnabled = false;
                }
            }
            catch (Exception endDatePickerException)
            {
                Debug.WriteLine($"[EndDatePicker] Error in date processing: {endDatePickerException.Message}");
                Debug.WriteLine($"[EndDatePicker] Stack trace: {endDatePickerException.StackTrace}");
            }
        }

        private void OnCalculatePriceClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            if (SelectedEndDate.HasValue)
            {
                // Calculate price based on days difference and daily rate
                // Make sure StartDate is not null, default to DateTime.Now if it is
                DateTime startDate = Product.StartDate ?? DateTime.Now;
                TimeSpan duration = SelectedEndDate.Value - startDate;
                int days = duration.Days + 1; // Include both start and end dates
                double totalPrice = days * Product.DailyRate;

                PriceTextBlock.Text = totalPrice.ToString("C"); // Format as currency
            }
        }

        public DateTimeOffset ConvertToDateTimeOffset(DateTime? dateTime)
        {
            if (dateTime.HasValue)
            {
                return new DateTimeOffset(dateTime.Value);
            }
            return DateTimeOffset.MinValue;
        }
    }
}