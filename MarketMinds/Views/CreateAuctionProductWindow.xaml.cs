using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MarketMinds.Shared.Models;
using MarketMinds.ViewModels;
using Windows.Storage.Pickers;
using ViewModelLayer.ViewModel;

namespace MarketMinds.Views
{
    public sealed partial class CreateAuctionProductWindow : Window
    {
        private readonly CreateAuctionListingViewModel viewModel;
        private readonly ProductCategoryViewModel categoryViewModel;
        private readonly ProductConditionViewModel conditionViewModel;
        private ObservableCollection<ProductImage> selectedImages;
        private bool isLoading = false;

        public CreateAuctionProductWindow()
        {
            this.InitializeComponent();
            this.viewModel = new CreateAuctionListingViewModel(MarketMinds.App.AuctionProductsService);
            this.categoryViewModel = new ProductCategoryViewModel(MarketMinds.App.CategoryService);
            this.conditionViewModel = new ProductConditionViewModel(MarketMinds.App.ConditionService);

            this.selectedImages = new ObservableCollection<ProductImage>();
            InitializeForm();
        }

        private void InitializeForm()
        {
            LoadCategories();
            LoadConditions();
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

                var auctionProduct = CreateAuctionProduct();

                viewModel.CreateListing(auctionProduct);

                await ShowSuccessMessage("Success", "Your auction has been created!");
                this.Close();
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error", $"Failed to create auction: {ex.Message}");
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

            if (!double.TryParse(StartingPriceTextBox.Text, out double startPrice) || startPrice <= 0)
            {
                ShowFieldError(StartingPriceErrorText, "Enter a valid starting price");
                isValid = false;
            }

            if (EndDatePicker.Date < DateTime.Now)
            {
                ShowFieldError(EndDateErrorText, "End date must be in the future");
                isValid = false;
            }

            return isValid;
        }

        private AuctionProduct CreateAuctionProduct()
        {
            var selectedCategory = (Category)CategoryComboBox.SelectedItem;
            var selectedCondition = (Condition)ConditionComboBox.SelectedItem;
            double.TryParse(StartingPriceTextBox.Text, out double startPrice);

            return new AuctionProduct
            {
                Title = TitleTextBox.Text.Trim(),
                Description = DescriptionTextBox.Text?.Trim() ?? string.Empty,
                SellerId = MarketMinds.App.CurrentUser?.Id ?? 0,
                CategoryId = selectedCategory.Id,
                ConditionId = selectedCondition.Id,
                StartTime = DateTime.Now,
                EndTime = EndDatePicker.Date.DateTime,
                StartPrice = startPrice,
                CurrentPrice = startPrice,
                Images = selectedImages.ToList()
            };
        }

        private async void UploadImagesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetLoading(true);

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
                        using (var managedStream = stream.AsStreamForRead())
                        {
                            string imageUrl = await MarketMinds.App.ImageUploadService.UploadImage(managedStream, file.Name);

                            if (!string.IsNullOrEmpty(imageUrl))
                            {
                                var imageInfo = new ProductImage
                                {
                                    Url = imageUrl
                                };
                                selectedImages.Add(imageInfo);
                            }
                            else
                            {
                                ShowErrorMessage("Upload Failed", $"Could not upload image: {file.Name}");
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
            if (sender is Button button && button.Tag is ProductImage imageInfo)
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
            StartingPriceErrorText.Visibility = Visibility.Collapsed;
            EndDateErrorText.Visibility = Visibility.Collapsed;
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
    }
}
