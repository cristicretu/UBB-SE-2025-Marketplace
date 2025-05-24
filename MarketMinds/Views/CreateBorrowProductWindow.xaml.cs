using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using ViewModelLayer.ViewModel;
using Windows.Storage.Pickers;
using MarketMinds.Shared.Models.DTOs;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.ImagineUploadService;

namespace MarketMinds.Views
{
    public sealed partial class CreateBorrowProductWindow : Window
    {
        private readonly BorrowProductsViewModel borrowProductsViewModel;
        private readonly ProductCategoryViewModel categoryViewModel;
        private readonly ProductConditionViewModel conditionViewModel;
        private ObservableCollection<CreateBorrowProductDTO.ImageInfo> selectedImages;
        private bool isLoading = false;

        public CreateBorrowProductWindow()
        {
            this.InitializeComponent();
            this.borrowProductsViewModel = MarketMinds.App.BorrowProductsViewModel;
            this.categoryViewModel = new ProductCategoryViewModel(MarketMinds.App.CategoryService);
            this.conditionViewModel = new ProductConditionViewModel(MarketMinds.App.ConditionService);

            this.selectedImages = new ObservableCollection<CreateBorrowProductDTO.ImageInfo>();

            InitializeForm();
        }

        private void InitializeForm()
        {
            LoadCategories();
            LoadConditions();

            StartDatePicker.Date = DateTime.Now;
            EndDatePicker.Date = DateTime.Now.AddDays(7);

            ImagePreviewGrid.ItemsSource = selectedImages;
        }

        private void LoadCategories()
        {
            try
            {
                var categories = categoryViewModel.GetAllProductCategories();
                CategoryComboBox.ItemsSource = categories;
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Failed to load categories", ex.Message);
            }
        }

        private void LoadConditions()
        {
            try
            {
                var conditions = conditionViewModel.GetAllProductConditions();
                ConditionComboBox.ItemsSource = conditions;
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Failed to load conditions", ex.Message);
            }
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (isLoading)
            {
                return;
            }
            ClearErrorMessages();

            if (!ValidateForm())
            {
                return;
            }

            try
            {
                SetLoading(true);

                var productDTO = CreateProductDTO();

                var validationErrors = this.borrowProductsViewModel.ValidateCreateProductDTO(productDTO);
                if (validationErrors.Any())
                {
                    DisplayValidationErrors(validationErrors);
                    return;
                }

                var createdProduct = this.borrowProductsViewModel.CreateBorrowProduct(productDTO);

                if (createdProduct != null)
                {
                    await ShowSuccessMessage("Success", "Your product has been listed successfully!");
                    this.Close();
                }
                else
                {
                    ShowErrorMessage("Error", "Failed to create the product listing. Please try again.");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error", $"An unexpected error occurred: {ex.Message}");
            }
            finally
            {
                SetLoading(false);
            }
        }

        private bool ValidateForm()
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                ShowFieldError(TitleErrorText, "Title is required");
                isValid = false;
            }

            if (CategoryComboBox.SelectedItem == null)
            {
                ShowFieldError(CategoryErrorText, "Please select a category");
                isValid = false;
            }

            if (ConditionComboBox.SelectedItem == null)
            {
                ShowFieldError(ConditionErrorText, "Please select a condition");
                isValid = false;
            }

            if (!double.TryParse(DailyRateTextBox.Text, out double dailyRate) || dailyRate <= 0)
            {
                ShowFieldError(DailyRateErrorText, "Please enter a valid daily rate greater than 0");
                isValid = false;
            }

            if (EndDatePicker.Date < StartDatePicker.Date)
            {
                ShowFieldError(DateErrorText, "End date cannot be before start date");
                isValid = false;
            }

            return isValid;
        }

        private CreateBorrowProductDTO CreateProductDTO()
        {
            var selectedCategory = (Category)CategoryComboBox.SelectedItem;
            var selectedCondition = (Condition)ConditionComboBox.SelectedItem;
            double.TryParse(DailyRateTextBox.Text, out double dailyRate);

            return new CreateBorrowProductDTO
            {
                Title = TitleTextBox.Text.Trim(),
                Description = DescriptionTextBox.Text?.Trim() ?? string.Empty,
                SellerId = GetCurrentUserId(),
                CategoryId = selectedCategory.Id,
                ConditionId = selectedCondition.Id,
                DailyRate = dailyRate,
                StartDate = StartDatePicker.Date.DateTime,
                EndDate = EndDatePicker.Date.DateTime,
                TimeLimit = EndDatePicker.Date.DateTime,
                IsBorrowed = false,
                Images = selectedImages.ToList()
            };
        }

        private int GetCurrentUserId()
        {
            return UserSession.CurrentUserId ?? 0;
        }

        private async void UploadImagesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetLoading(true); // Show loading indicator while uploading

                var picker = new FileOpenPicker();
                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".jpeg");
                picker.FileTypeFilter.Add(".png");
                picker.FileTypeFilter.Add(".gif");

                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

                var files = await picker.PickMultipleFilesAsync();
                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        using (var stream = await file.OpenReadAsync())
                        {
                            using (var managedStream = stream.AsStreamForRead())
                            {
                                string imageUrl = await MarketMinds.App.ImageUploadService.UploadImage(managedStream, file.Name);

                                if (!string.IsNullOrEmpty(imageUrl))
                                {
                                    var imageInfo = new CreateBorrowProductDTO.ImageInfo
                                    {
                                        Url = imageUrl // Store the Imgur URL
                                    };
                                    selectedImages.Add(imageInfo);
                                }
                                else
                                {
                                    ShowErrorMessage("Upload Failed", $"Could not upload image: {file.Name}. No URL returned.");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Image Upload Error", $"Failed to upload images: {ex.Message}");
            }
            finally
            {
                SetLoading(false);
            }
        }

        private void RemoveImage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is CreateBorrowProductDTO.ImageInfo imageInfo)
            {
                selectedImages.Remove(imageInfo);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ShowFieldError(TextBlock errorTextBlock, string message)
        {
            errorTextBlock.Text = message;
            errorTextBlock.Visibility = Visibility.Visible;
        }

        private void ClearErrorMessages()
        {
            TitleErrorText.Visibility = Visibility.Collapsed;
            CategoryErrorText.Visibility = Visibility.Collapsed;
            ConditionErrorText.Visibility = Visibility.Collapsed;
            DailyRateErrorText.Visibility = Visibility.Collapsed;
            DateErrorText.Visibility = Visibility.Collapsed;
        }

        private void DisplayValidationErrors(Dictionary<string, string[]> errors)
        {
            foreach (var error in errors)
            {
                var message = string.Join(", ", error.Value);
                switch (error.Key.ToLower())
                {
                    case "title":
                        ShowFieldError(TitleErrorText, message);
                        break;
                    case "dailyrate":
                        ShowFieldError(DailyRateErrorText, message);
                        break;
                    case "enddate":
                        ShowFieldError(DateErrorText, message);
                        break;
                    default:
                        ShowErrorMessage("Validation Error", message);
                        break;
                }
            }
        }

        private async void ShowErrorMessage(string title, string message)
        {
            var dialog = new ContentDialog()
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private async Task ShowSuccessMessage(string title, string message)
        {
            var dialog = new ContentDialog()
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private void SetLoading(bool loading)
        {
            isLoading = loading;
            LoadingOverlay.Visibility = loading ? Visibility.Visible : Visibility.Collapsed;
            SubmitButton.IsEnabled = !loading;
            CancelButton.IsEnabled = !loading;
        }

        public string GetCharacterCount(string text)
        {
            int count = string.IsNullOrEmpty(text) ? 0 : text.Length;
            return $"{count}/500";
        }
    }
}