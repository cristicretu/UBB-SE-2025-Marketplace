using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Collections.Generic;
using ViewModelLayer.ViewModel;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Storage.Streams;
using Newtonsoft.Json;
using MarketMinds.Shared.Models;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using MarketMinds.Shared.Services;
using WinRT.Interop;

namespace MarketMinds
{
    public sealed partial class CreateReviewView : Window
    {
        public ReviewCreateViewModel ViewModel { get; set; }
        private readonly bool isEditing;

        private const int CLOSE_DELAY_MILLISECONDS = 1000;
        private const int DEFAULT_RATING = 0;

        public CreateReviewView(ReviewCreateViewModel viewModel, Review? review = null)
        {
            ViewModel = viewModel;
            isEditing = review != null;
            if (isEditing)
            {
                ViewModel.CurrentReview = review;
                ViewModel.Description = review.Description;
                ViewModel.Rating = review.Rating;
                ViewModel.Images = review.Images;
            }
            this.InitializeComponent();
            this.Closed += OnWindowClosed;
            // Update status message text block
            if (StatusMessageTextBlock != null)
            {
                UpdateStatusMessage();
            }
        }

        private async void HandleAddImage_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            var picker = new FileOpenPicker();
            var hwnd = WindowNative.GetWindowHandle(this);
            InitializeWithWindow.Initialize(picker, hwnd);

            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                try
                {
                    using (var stream = await file.OpenAsync(FileAccessMode.Read))
                    {
                        await ViewModel.AddImage(stream.AsStreamForRead(), file.Name);
                    }
                }
                catch (Exception ex)
                {
                    ViewModel.StatusMessage = "Error selecting or reading image: " + ex.Message;
                }
            }
            else
            {
                ViewModel.StatusMessage = "No image selected.";
            }
            UpdateStatusMessage();
        }

        private void HandleSubmit_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            if (ViewModel == null)
            {
                Debug.WriteLine("ViewModel is null!");
                return;
            }

            if (ViewModel.ImagesString.Contains("uploading...", StringComparison.OrdinalIgnoreCase))
            {
                Debug.WriteLine("Please wait for image upload to complete.");
                return;
            }

            if (isEditing)
            {
                ViewModel.UpdateReview();
            }
            else
            {
                ViewModel.SubmitReview();
            }

            UpdateStatusMessage();

            // Allow a moment to see the status message before closing
            Task.Delay(CLOSE_DELAY_MILLISECONDS).ContinueWith(_ => this.DispatcherQueue.TryEnqueue(() => this.Close()));
        }

        private void OnWindowClosed(object sender, Microsoft.UI.Xaml.WindowEventArgs windowEventArgs)
        {
            // Clear ViewModel properties when the window is closed
            ViewModel.Description = string.Empty;
            ViewModel.ImagesString = string.Empty;
            ViewModel.Rating = DEFAULT_RATING;
        }

        private async void OnUploadImageClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var picker = new FileOpenPicker();
            var hwnd = WindowNative.GetWindowHandle(this);
            InitializeWithWindow.Initialize(picker, hwnd);

            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                try
                {
                    using (var stream = await file.OpenAsync(FileAccessMode.Read))
                    {
                        await ViewModel.AddImage(stream.AsStreamForRead(), file.Name);
                    }
                }
                catch (Exception ex)
                {
                    ViewModel.StatusMessage = "Error selecting or reading image: " + ex.Message;
                }
            }
            else
            {
                ViewModel.StatusMessage = "No image selected.";
            }
            UpdateStatusMessage();
        }
        private void UpdateStatusMessage()
        {
            if (StatusMessageTextBlock != null)
            {
                StatusMessageTextBlock.Text = ViewModel.StatusMessage;
            }
        }
    }
}