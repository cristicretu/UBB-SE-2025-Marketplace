using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MarketMinds.ViewModels;
using MarketMinds.Shared.Models;
using MarketMinds;
using MarketMinds.Helpers;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using Windows.Storage.Pickers;
using Windows.Storage;
using MarketMinds.Shared.Services.ImagineUploadService;
using MarketMinds.Shared.Services.ListingFormValidationService;
using ProductCategory = MarketMinds.Shared.Models.Category;
using ProductCondition = MarketMinds.Shared.Models.Condition;
using MarketMinds.ViewModels;
using WinRT.Interop;
using MarketMinds.Shared.Helper;

namespace MarketMinds.Views
{
    public sealed partial class CreateListingView : Page
    {
        private CreateListingViewModelBase viewModel;
        private ProductCategoryViewModel productCategoryViewModel;
        private ProductConditionViewModel productConditionViewModel;
        private ProductTagViewModel productTagViewModel;
        private ObservableCollection<string> tags;
        private HomePageView homePageWindow;
        private readonly string imgurClientId;
        private static readonly HttpClient HttpClient = new HttpClient();
        private const int NO_ITEMS = 0;
        private const int BASE_PAGE = 1;
        private const int NO_REVIEWS = 0;
        private const int NO_MB = 10;
        private const int NO_MB_LIMIT = 10 * 1024 * 1024;
        private const int MAX_RETRIES = 3;
        private const int MAX_RETRY_DELAY = 2;
        private const int MAX_CLIENT_ID_LENGTH = 20;
        private const int NO_RETRY = 0;
        private const int UPLOAD_IMAGE_BUTTON_WIDTH = 150;
        private readonly TagManagementViewModelHelper tagManagementHelper;
        private readonly ImageUploadService imageUploadService;
        private readonly ListingFormValidationService validationService;
        private Border currentSelectedCard;

        // Properties for binding
        public CreateListingViewModelBase ViewModel => viewModel;
        public ObservableCollection<string> Tags => tags;

        public CreateListingView()
        {
            this.InitializeComponent();

            imgurClientId = App.Configuration.GetSection("ImgurSettings:ClientId").Value;
            tags = new ObservableCollection<string>();

            // Use singleton instances from App class
            productCategoryViewModel = App.ProductCategoryViewModel;
            productConditionViewModel = App.ProductConditionViewModel;
            productTagViewModel = App.ProductTagViewModel;

            // Initialize services
            tagManagementHelper = new TagManagementViewModelHelper(productTagViewModel);
            imageUploadService = new ImageUploadService(AppConfig.Configuration);
            validationService = new ListingFormValidationService();

            // Set up tags list
            TagsListView.ItemsSource = tags;
            homePageWindow = App.HomePageWindow;

            // Load data and set default selection when page loads
            this.Loaded += CreateListingView_Loaded;
        }

        private void CreateListingView_Loaded(object sender, RoutedEventArgs e)
        {
            // Load categories and conditions into ComboBoxes
            LoadCategories();
            LoadConditions();

            // Initialize default viewModel
            viewModel = new CreateBuyListingViewModel { BuyProductsService = App.BuyProductsService };
            viewModel.SelectedListingType = "Buy";

            // Default select the Sell card
            UpdateCardSelectionState(SellCard);
            ListingTypeComboBox.SelectedIndex = 0; // Select "Buy" which corresponds to Sell

            // Set date restrictions
            SetDateRestrictions();

            // Notify bindings to update
            this.Bindings.Update();
        }

        private void SetDateRestrictions()
        {
            // Set minimum date to today for all date pickers
            var today = DateTimeOffset.Now.Date;
            
            // For borrow product start date - can only select from today onwards
            StartDatePicker.MinDate = today;
            
            // For borrow product end date - initially set to today, will be updated when start date is selected
            TimeLimitDatePicker.MinDate = today;
            // Disable end date picker until start date is selected
            TimeLimitDatePicker.IsEnabled = false;
            
            // For auction end date - can only select from today onwards
            AuctionEndDatePicker.MinDate = today;
        }

        private void StartDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            // When start date changes, update the minimum date for end date picker
            if (args.NewDate.HasValue)
            {
                // Enable the end date picker and set minimum date to the day after start date
                TimeLimitDatePicker.IsEnabled = true;
                TimeLimitDatePicker.MinDate = args.NewDate.Value.AddDays(1);
                
                // If the current end date is before the new minimum, clear it
                if (TimeLimitDatePicker.Date.HasValue && TimeLimitDatePicker.Date.Value < TimeLimitDatePicker.MinDate)
                {
                    TimeLimitDatePicker.Date = null;
                }
            }
            else
            {
                // If start date is cleared, disable end date picker and reset minimum to today
                TimeLimitDatePicker.IsEnabled = false;
                TimeLimitDatePicker.Date = null; // Clear any selected end date
                TimeLimitDatePicker.MinDate = DateTimeOffset.Now.Date;
            }
        }

        private void LoadCategories()
        {
            List<Category> categories = productCategoryViewModel.GetAllProductCategories();
            CategoryComboBox.ItemsSource = categories;
            CategoryComboBox.DisplayMemberPath = "DisplayTitle";
            CategoryComboBox.SelectedValuePath = "Id";
        }

        private void LoadConditions()
        {
            List<Condition> conditions = productConditionViewModel.GetAllProductConditions();
            ConditionComboBox.ItemsSource = conditions;
            ConditionComboBox.DisplayMemberPath = "DisplayTitle";
            ConditionComboBox.SelectedValuePath = "Id";
        }

        private void ListingTypeCard_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // Get the border that was clicked
            if (sender is Border clickedCard)
            {
                // Get the type from the Tag property
                string selectedType = clickedCard.Tag?.ToString();

                // Find and select the corresponding ComboBox item
                foreach (ComboBoxItem item in ListingTypeComboBox.Items)
                {
                    if (item.Content.ToString() == selectedType)
                    {
                        ListingTypeComboBox.SelectedItem = item;
                        break;
                    }
                }

                // Update visual selection state
                UpdateCardSelectionState(clickedCard);
            }
        }

        private void UpdateCardSelectionState(Border selectedCard)
        {
            // Reset all cards to default state
            AuctionCard.Background = Application.Current.Resources["CardBackgroundFillColorDefaultBrush"] as SolidColorBrush
                ?? new SolidColorBrush(Colors.Transparent);
            AuctionCard.BorderBrush = Application.Current.Resources["CardStrokeColorDefaultBrush"] as SolidColorBrush
                ?? new SolidColorBrush(Colors.LightGray);
            AuctionCard.BorderThickness = new Thickness(1);

            SellCard.Background = Application.Current.Resources["CardBackgroundFillColorDefaultBrush"] as SolidColorBrush
                ?? new SolidColorBrush(Colors.Transparent);
            SellCard.BorderBrush = Application.Current.Resources["CardStrokeColorDefaultBrush"] as SolidColorBrush
                ?? new SolidColorBrush(Colors.LightGray);
            SellCard.BorderThickness = new Thickness(1);

            LendCard.Background = Application.Current.Resources["CardBackgroundFillColorDefaultBrush"] as SolidColorBrush
                ?? new SolidColorBrush(Colors.Transparent);
            LendCard.BorderBrush = Application.Current.Resources["CardStrokeColorDefaultBrush"] as SolidColorBrush
                ?? new SolidColorBrush(Colors.LightGray);
            LendCard.BorderThickness = new Thickness(1);

            // Apply selected state to the clicked card
            // Use a light variant of the accent color for the background
            Windows.UI.Color accentColor = Colors.Blue;
            if (Application.Current.Resources["SystemAccentColor"] is Windows.UI.Color color)
            {
                accentColor = color;
            }

            // Create a lighter version of the accent color (20% opacity)
            var lightAccentColor = new Windows.UI.Color
            {
                A = 50, // 20% opacity
                R = accentColor.R,
                G = accentColor.G,
                B = accentColor.B
            };

            selectedCard.Background = new SolidColorBrush(lightAccentColor);
            selectedCard.BorderBrush = Application.Current.Resources["SystemAccentColor"] is Windows.UI.Color accent
                ? new SolidColorBrush(accent)
                : Application.Current.Resources["SystemAccentColor"] as SolidColorBrush
                    ?? new SolidColorBrush(Colors.Blue);
            selectedCard.BorderThickness = new Thickness(2);

            // Save current selected card
            currentSelectedCard = selectedCard;
        }

        private void ListingTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            listingTypeErrorTextBlock.Visibility = Visibility.Collapsed;

            // If selection was triggered by UI, update card state
            if (selectionChangedEventArgs.AddedItems.Count > 0)
            {
                var selectedType = (selectionChangedEventArgs.AddedItems[0] as ComboBoxItem)?.Content.ToString();

                // If selection was triggered programmatically, update card selection
                if (currentSelectedCard == null || currentSelectedCard.Tag.ToString() != selectedType)
                {
                    // Find and update the corresponding card
                    if (selectedType == "Buy")
                    {
                        UpdateCardSelectionState(SellCard);
                    }
                    else if (selectedType == "Borrow")
                    {
                        UpdateCardSelectionState(LendCard);
                    }
                    else if (selectedType == "Auction")
                    {
                        UpdateCardSelectionState(AuctionCard);
                    }
                }

                switch (selectedType)
                {
                    case "Buy":
                        viewModel = new CreateBuyListingViewModel { BuyProductsService = App.BuyProductsService };
                        viewModel.SelectedListingType = "Buy";
                        break;
                    case "Borrow":
                        viewModel = new CreateBorrowListingViewModel { BorrowProductsService = App.BorrowProductsService };
                        viewModel.SelectedListingType = "Borrow";
                        break;
                    case "Auction":
                        viewModel = new CreateAuctionListingViewModel(App.AuctionProductsService);
                        viewModel.SelectedListingType = "Auction";
                        break;
                }

                // Notify bindings to update
                this.Bindings.Update();
            }
        }

        private void TagsAddButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            string tag = TagsTextBox.Text.Trim();
            if (tagManagementHelper.AddTagToCollectionBeginning(tag, tags))
            {
                TagsTextBox.Text = string.Empty;
            }
            TagsTextBox.Focus(FocusState.Programmatic);
        }

        private void TagsTextBox_KeyDown(object sender, KeyRoutedEventArgs keyRoutedEventArgs)
        {
            if (keyRoutedEventArgs.Key == Windows.System.VirtualKey.Enter)
            {
                string tag = TagsTextBox.Text.Trim();
                if (tagManagementHelper.AddTagToCollectionBeginning(tag, tags))
                {
                    TagsTextBox.Text = string.Empty;
                }
            }
        }

        private void TagsListView_ItemClick(object sender, ItemClickEventArgs itemClickEventArgs)
        {
            string tag = itemClickEventArgs.ClickedItem as string;
            tagManagementHelper.RemoveTagFromCollection(tag, tags);
            TagsTextBox.Focus(FocusState.Programmatic);
        }

        private void UploadBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            OnUploadImageClick(sender, null);
        }

        private void UploadBorder_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border)
            {
                border.Opacity = 0.8;
            }
        }

        private void UploadBorder_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Border border)
            {
                border.Opacity = 1.0;
            }
        }

        private async void OnUploadImageClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var picker = new FileOpenPicker();

            // Get the current homePageWindow handle (HWND)
            var hwnd = WindowNative.GetWindowHandle(this.homePageWindow);
            InitializeWithWindow.Initialize(picker, hwnd);

            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".gif");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                try
                {
                    using (var stream = await file.OpenAsync(FileAccessMode.Read))
                    {
                        // Pass the stream, file name, and current images string to the service
                        string updatedImagesString = await imageUploadService.AddImageToCollection(stream.AsStreamForRead(), file.Name, viewModel?.ImagesString ?? string.Empty);
                        if (updatedImagesString != (viewModel?.ImagesString ?? string.Empty))
                        {
                            if (viewModel != null)
                            {
                                viewModel.ImagesString = updatedImagesString;
                                // Force binding update
                                this.Bindings.Update();
                            }
                        }
                    }
                }
                catch (Exception imageUploadException)
                {
                    await ShowErrorDialog("Image Upload Error", imageUploadException.Message);
                }
            }
        }

        private async Task ShowErrorDialog(string title, string message)
        {
            ContentDialog errorDialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = App.HomePageWindow?.Content.XamlRoot
            };
            await errorDialog.ShowAsync();
        }

        private async void ShowSuccessMessage(string message)
        {
            ContentDialog successDialog = new ContentDialog
            {
                Title = "Success",
                Content = message,
                CloseButtonText = "Ok",
                XamlRoot = App.HomePageWindow?.Content.XamlRoot
            };
            await successDialog.ShowAsync();
        }

        private void CreateListingButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            // Reset error messages
            TitleErrorTextBlock.Visibility = Visibility.Collapsed;
            CategoryErrorTextBlock.Visibility = Visibility.Collapsed;
            DescriptionErrorTextBlock.Visibility = Visibility.Collapsed;
            TagsErrorTextBlock.Visibility = Visibility.Collapsed;
            ConditionErrorTextBlock.Visibility = Visibility.Collapsed;
            listingTypeErrorTextBlock.Visibility = Visibility.Collapsed;

            // Reset product-specific error messages
            PriceErrorTextBlock.Visibility = Visibility.Collapsed;
            StockErrorTextBlock.Visibility = Visibility.Collapsed;
            StartDateErrorTextBlock.Visibility = Visibility.Collapsed;
            EndDateErrorTextBlock.Visibility = Visibility.Collapsed;
            DailyRateErrorTextBlock.Visibility = Visibility.Collapsed;
            StartingBidErrorTextBlock.Visibility = Visibility.Collapsed;
            AuctionEndDateErrorTextBlock.Visibility = Visibility.Collapsed;

            // Check if user is logged in
            if (App.CurrentUser == null || App.CurrentUser.Id <= 0)
            {
                _ = ShowErrorDialog("Authentication Error", "You must be logged in to create a listing. Please log in and try again.");
                return;
            }

            // Check if a listing type is selected
            if (ListingTypeComboBox.SelectedItem == null)
            {
                listingTypeErrorTextBlock.Visibility = Visibility.Visible;
                return;
            }

            // Collect common data from XAML controls
            string title = TitleTextBox.Text?.Trim();
            Category category = (ProductCategory)CategoryComboBox.SelectedItem;
            string description = DescriptionTextBox.Text?.Trim() ?? string.Empty;
            Condition condition = (ProductCondition)ConditionComboBox.SelectedItem;

            // Validate category selection
            if (category == null || category.Id <= 0)
            {
                CategoryErrorTextBlock.Text = "Please select a valid category.";
                CategoryErrorTextBlock.Visibility = Visibility.Visible;
                return;
            }

            // Validate condition selection
            if (condition == null || condition.Id <= 0)
            {
                ConditionErrorTextBlock.Text = "Please select a valid condition.";
                ConditionErrorTextBlock.Visibility = Visibility.Visible;
                return;
            }

            // Validate common fields
            bool isValid = validationService.ValidateCommonFields(title, category, description, tags, condition, out string errorMessage, out string errorField);

            if (!isValid)
            {
                switch (errorField)
                {
                    case "Title":
                        TitleErrorTextBlock.Text = errorMessage;
                        TitleErrorTextBlock.Visibility = Visibility.Visible;
                        break;
                    case "Category":
                        CategoryErrorTextBlock.Text = errorMessage;
                        CategoryErrorTextBlock.Visibility = Visibility.Visible;
                        break;
                    case "Description":
                        DescriptionErrorTextBlock.Text = errorMessage;
                        DescriptionErrorTextBlock.Visibility = Visibility.Visible;
                        break;
                    case "Tags":
                        TagsErrorTextBlock.Text = errorMessage;
                        TagsErrorTextBlock.Visibility = Visibility.Visible;
                        break;
                    case "Condition":
                        ConditionErrorTextBlock.Text = errorMessage;
                        ConditionErrorTextBlock.Visibility = Visibility.Visible;
                        break;
                }
                return;
            }

            // Convert string tags to ProductTag objects (temporarily skip for testing)
            List<ProductTag> productTags = new List<ProductTag>(); // Empty for now to test basic product creation

            // Collect specific data based on the selected type
            if (viewModel is CreateBuyListingViewModel)
            {
                string priceText = PriceTextBox.Text?.Trim();
                string stockText = StockTextBox.Text?.Trim();

                if (string.IsNullOrWhiteSpace(priceText) || !validationService.ValidateBuyProductFields(priceText, out float price))
                {
                    PriceErrorTextBlock.Text = "Please enter a valid price.";
                    PriceErrorTextBlock.Visibility = Visibility.Visible;
                    return;
                }

                if (string.IsNullOrWhiteSpace(stockText) || !int.TryParse(stockText, out int stockQuantity) || stockQuantity <= 0)
                {
                    StockErrorTextBlock.Text = "Please enter a valid stock quantity (must be greater than 0).";
                    StockErrorTextBlock.Visibility = Visibility.Visible;
                    return;
                }

                // Create BuyProduct with proper validation
                try
                {
                    var product = new BuyProduct
                    {
                        Id = 0, // New product
                        Title = title,
                        Description = description,
                        SellerId = App.CurrentUser.Id, // Use current user ID
                        Seller = App.CurrentUser,
                        ConditionId = condition.Id,
                        Condition = condition,
                        CategoryId = category.Id,
                        Category = category,
                        Price = price,
                        Stock = stockQuantity,
                        Tags = productTags,
                        NonMappedImages = viewModel.Images ?? new List<MarketMinds.Shared.Models.Image>()
                    };

                    Console.WriteLine($"Creating BuyProduct with: SellerId={product.SellerId}, " +
                                    $"CategoryId={product.CategoryId}, ConditionId={product.ConditionId}, " +
                                    $"Title='{product.Title}', Price={product.Price}, Stock={product.Stock}");

                    viewModel.CreateListing(product);
                    ShowSuccessMessage("Listing created successfully!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating buy product: {ex.Message}");
                    _ = ShowErrorDialog("Error Creating Listing", $"Failed to create listing: {ex.Message}");
                    return;
                }
            }
            else if (viewModel is CreateBorrowListingViewModel)
            {
                if (!StartDatePicker.Date.HasValue)
                {
                    StartDateErrorTextBlock.Text = "Please select a start date.";
                    StartDateErrorTextBlock.Visibility = Visibility.Visible;
                    return;
                }

                if (!TimeLimitDatePicker.Date.HasValue)
                {
                    EndDateErrorTextBlock.Text = "Please select an end date.";
                    EndDateErrorTextBlock.Visibility = Visibility.Visible;
                    return;
                }

                DateTime startDate = StartDatePicker.Date.Value.DateTime;
                DateTime endDate = TimeLimitDatePicker.Date.Value.DateTime;

                Console.WriteLine($"CreateListingView - User selected dates: StartDate={startDate}, EndDate={endDate}");

                if (endDate <= startDate)
                {
                    EndDateErrorTextBlock.Text = "End date must be after start date.";
                    EndDateErrorTextBlock.Visibility = Visibility.Visible;
                    return;
                }

                if (!float.TryParse(DailyRateTextBox.Text, out float dailyRate) || dailyRate <= 0)
                {
                    DailyRateErrorTextBlock.Text = "Please enter a valid daily rate (must be greater than 0).";
                    DailyRateErrorTextBlock.Visibility = Visibility.Visible;
                    return;
                }

                Console.WriteLine($"CreateListingView - Creating BorrowProduct with: " +
                                $"StartDate={startDate}, EndDate={endDate}, TimeLimit={endDate}");

                var product = new BorrowProduct(0, title, description, App.CurrentUser, condition, category, productTags, viewModel.Images, endDate, startDate, endDate, dailyRate, false);

                Console.WriteLine($"CreateListingView - BorrowProduct created with: " +
                                $"StartDate={product.StartDate}, EndDate={product.EndDate}, TimeLimit={product.TimeLimit}");

                viewModel.CreateListing(product);
                ShowSuccessMessage("Rental listing created successfully!");
            }
            else if (viewModel is CreateAuctionListingViewModel)
            {
                if (!float.TryParse(StartingBidTextBox.Text, out float startingPrice) || startingPrice <= 0)
                {
                    StartingBidErrorTextBlock.Text = "Please enter a valid starting price (must be greater than 0).";
                    StartingBidErrorTextBlock.Visibility = Visibility.Visible;
                    return;
                }

                if (!AuctionEndDatePicker.Date.HasValue)
                {
                    AuctionEndDateErrorTextBlock.Text = "Please select an end auction date.";
                    AuctionEndDateErrorTextBlock.Visibility = Visibility.Visible;
                    return;
                }
                DateTime endAuctionDate = AuctionEndDatePicker.Date.Value.DateTime;

                if (endAuctionDate <= DateTime.Now)
                {
                    AuctionEndDateErrorTextBlock.Text = "Auction end date must be in the future.";
                    AuctionEndDateErrorTextBlock.Visibility = Visibility.Visible;
                    return;
                }

                var product = new AuctionProduct(0, title, description, App.CurrentUser, condition, category, productTags, viewModel.Images, DateTime.Now, endAuctionDate, startingPrice);
                viewModel.CreateListing(product);
                ShowSuccessMessage("Auction listing created successfully!");
            }
        }
    }
}

