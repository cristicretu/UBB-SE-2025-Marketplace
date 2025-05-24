using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using ViewModelLayer.ViewModel;
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
        private TextBox titleTextBox;
        private TextBlock titleErrorTextBlock;
        private ComboBox categoryComboBox;
        private TextBlock categoryErrorTextBlock;
        private TextBox descriptionTextBox;
        private TextBlock descriptionErrorTextBlock;
        private TextBox tagsTextBox;
        private TextBlock tagsErrorTextBlock;
        private ListView tagsListView;
        private TextBox imagesTextBox;
        private ComboBox conditionComboBox;
        private TextBlock conditionErrorTextBlock;
        private ObservableCollection<string> tags;
        private Button uploadImageButton;
        private TextBlock imagesTextBlock;
        private TextBlock categoryTextBlock;
        private TextBlock conditionTextBlock;
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

        public CreateListingView()
        {
            this.InitializeComponent();
            imgurClientId = App.Configuration.GetSection("ImgurSettings:ClientId").Value;
            titleTextBox = new TextBox { PlaceholderText = "Title" };
            titleErrorTextBlock = new TextBlock { Text = "Title cannot be empty.", Foreground = new SolidColorBrush(Colors.Red), Visibility = Visibility.Collapsed };
            categoryTextBlock = new TextBlock { Text = "Select Category" };
            categoryComboBox = new ComboBox();
            categoryErrorTextBlock = new TextBlock { Text = "Please select a category.", Foreground = new SolidColorBrush(Colors.Red), Visibility = Visibility.Collapsed };
            descriptionTextBox = new TextBox { PlaceholderText = "Description" };
            descriptionErrorTextBlock = new TextBlock { Text = "Description cannot be empty.", Foreground = new SolidColorBrush(Colors.Red), Visibility = Visibility.Collapsed };
            tagsTextBox = new TextBox { PlaceholderText = "Tags" };
            tagsErrorTextBlock = new TextBlock { Text = "Please add at least one tag.", Foreground = new SolidColorBrush(Colors.Red), Visibility = Visibility.Collapsed };
            tagsListView = new ListView();
            imagesTextBox = new TextBox { PlaceholderText = "Images" };
            conditionTextBlock = new TextBlock { Text = "Select Condition" };
            conditionComboBox = new ComboBox();
            conditionErrorTextBlock = new TextBlock { Text = "Please select a condition.", Foreground = new SolidColorBrush(Colors.Red), Visibility = Visibility.Collapsed };
            tags = new ObservableCollection<string>();
            uploadImageButton = new Button { Content = "Upload Image", Width = UPLOAD_IMAGE_BUTTON_WIDTH };
            uploadImageButton.Click += OnUploadImageClick;
            imagesTextBlock = new TextBlock { TextWrapping = TextWrapping.Wrap };

            // Use singleton instances from App class
            productCategoryViewModel = App.ProductCategoryViewModel;
            productConditionViewModel = App.ProductConditionViewModel;
            productTagViewModel = App.ProductTagViewModel;

            // Initialize services
            tagManagementHelper = new TagManagementViewModelHelper(productTagViewModel);
            imageUploadService = new ImageUploadService(AppConfig.Configuration);
            validationService = new ListingFormValidationService();

            // Load categories and conditions into ComboBoxes
            LoadCategories();
            LoadConditions();

            // Set up event handler for tagsTextBox
            tagsTextBox.KeyDown += TagsTextBox_KeyDown;

            // Set up the ListView item template
            tagsListView.ItemClick += TagsListView_ItemClick;
            tagsListView.IsItemClickEnabled = true;

            tagsListView.ItemsSource = tags;
            homePageWindow = App.HomePageWindow;
        }

        private void LoadCategories()
        {
            List<Category> categories = productCategoryViewModel.GetAllProductCategories();
            categoryComboBox.ItemsSource = categories;
            categoryComboBox.DisplayMemberPath = "DisplayTitle";
            categoryComboBox.SelectedValuePath = "Id";
        }

        private void LoadConditions()
        {
            List<Condition> conditions = productConditionViewModel.GetAllProductConditions();
            conditionComboBox.ItemsSource = conditions;
            conditionComboBox.DisplayMemberPath = "DisplayTitle";
            conditionComboBox.SelectedValuePath = "Id";
        }

        private void ListingTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            listingTypeErrorTextBlock.Visibility = Visibility.Collapsed;

            var selectedType = (selectionChangedEventArgs.AddedItems[0] as ComboBoxItem)?.Content.ToString();
            FormContainer.Children.Clear();

            switch (selectedType)
            {
                case "Buy":
                    viewModel = new CreateBuyListingViewModel { BuyProductsService = App.BuyProductsService };
                    // clear common fields
                    AddBuyProductFields();
                    break;
                case "Borrow":
                    viewModel = new CreateBorrowListingViewModel { BorrowProductsService = App.BorrowProductsService };
                    AddBorrowProductFields();
                    break;
                case "Auction":
                    viewModel = new CreateAuctionListingViewModel(App.AuctionProductsService);
                    AddAuctionProductFields();
                    break;
            }
        }

        private void AddCommonFields()
        {
            FormContainer.Children.Add(titleTextBox);
            FormContainer.Children.Add(titleErrorTextBlock);
            FormContainer.Children.Add(categoryTextBlock);
            FormContainer.Children.Add(categoryComboBox);
            FormContainer.Children.Add(categoryErrorTextBlock);
            FormContainer.Children.Add(descriptionTextBox);
            FormContainer.Children.Add(descriptionErrorTextBlock);
            FormContainer.Children.Add(tagsTextBox);
            FormContainer.Children.Add(tagsErrorTextBlock);
            FormContainer.Children.Add(tagsListView);
            FormContainer.Children.Add(uploadImageButton);
            FormContainer.Children.Add(imagesTextBlock);
            FormContainer.Children.Add(conditionTextBlock);
            FormContainer.Children.Add(conditionComboBox);
            FormContainer.Children.Add(conditionErrorTextBlock);
        }

        private void AddBuyProductFields()
        {
            AddCommonFields();
            FormContainer.Children.Add(new TextBox { PlaceholderText = "Price", Name = "PriceTextBox" });
        }

        private void AddBorrowProductFields()
        {
            AddCommonFields();
            var timeLimistDatePicker = new CalendarDatePicker
            {
                PlaceholderText = "Time Limit",
                Name = "TimeLimitDatePicker",
                MinDate = DateTimeOffset.Now
            };
            FormContainer.Children.Add(timeLimistDatePicker);
            FormContainer.Children.Add(new TextBox { PlaceholderText = "Daily Rate", Name = "DailyRateTextBox" });
        }

        private void AddAuctionProductFields()
        {
            AddCommonFields();
            FormContainer.Children.Add(new TextBox { PlaceholderText = "Starting Price", Name = "StartingPriceTextBox" });
            var endAuctionDatePicker = new CalendarDatePicker
            {
                PlaceholderText = "End Auction Date",
                Name = "EndAuctionDatePicker",
                MinDate = DateTimeOffset.Now
            };
            FormContainer.Children.Add(endAuctionDatePicker);
        }

        private void TagsTextBox_KeyDown(object sender, KeyRoutedEventArgs keyRoutedEventArgs)
        {
            if (keyRoutedEventArgs.Key == Windows.System.VirtualKey.Enter)
            {
                string tag = tagsTextBox.Text.Trim();
                if (tagManagementHelper.AddTagToCollection(tag, tags))
                {
                    tagsTextBox.Text = string.Empty;
                }
            }
        }

        private void TagsListView_ItemClick(object sender, ItemClickEventArgs itemClickEventArgs)
        {
            string tag = itemClickEventArgs.ClickedItem as string;
            tagManagementHelper.RemoveTagFromCollection(tag, tags);
        }

        private async void OnUploadImageClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var picker = new FileOpenPicker();

            // Get the current homePageWindow handle (HWND)
            var hwnd = WindowNative.GetWindowHandle(this.homePageWindow); // Assuming 'this.homePageWindow' is the correct MainWindow instance
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
                        string updatedImagesString = await imageUploadService.AddImageToCollection(stream.AsStreamForRead(), file.Name, viewModel.ImagesString);
                        if (updatedImagesString != viewModel.ImagesString)
                        {
                            viewModel.ImagesString = updatedImagesString;
                            imagesTextBlock.Text = viewModel.ImagesString; // Update the UI
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
            titleErrorTextBlock.Visibility = Visibility.Collapsed;
            categoryErrorTextBlock.Visibility = Visibility.Collapsed;
            descriptionErrorTextBlock.Visibility = Visibility.Collapsed;
            tagsErrorTextBlock.Visibility = Visibility.Collapsed;
            conditionErrorTextBlock.Visibility = Visibility.Collapsed;
            listingTypeErrorTextBlock.Visibility = Visibility.Collapsed;

            // Check if a listing type is selected
            if (ListingTypeComboBox.SelectedItem == null)
            {
                listingTypeErrorTextBlock.Visibility = Visibility.Visible;
                return;
            }

            // Collect common data
            string title = titleTextBox.Text;
            Category category = (ProductCategory)categoryComboBox.SelectedItem;
            string description = descriptionTextBox.Text;
            Condition condition = (ProductCondition)conditionComboBox.SelectedItem;

            // ---> ADD EXPLICIT ID CHECK <---
            if (category == null || category.Id == 0)
            {
                 categoryErrorTextBlock.Text = "Please select a valid category.";
                 categoryErrorTextBlock.Visibility = Visibility.Visible;
                 return; // Stop if category is null or ID is 0
            }
            if (condition == null || condition.Id == 0)
            {
                 conditionErrorTextBlock.Text = "Please select a valid condition.";
                 conditionErrorTextBlock.Visibility = Visibility.Visible;
                 return;
            }
            // Validate common fields
            bool isValid = validationService.ValidateCommonFields(title, category, description, tags, condition, out string errorMessage, out string errorField);

            if (!isValid)
            {
                switch (errorField)
                {
                    case "Title":
                        titleErrorTextBlock.Text = errorMessage;
                        titleErrorTextBlock.Visibility = Visibility.Visible;
                        break;
                    case "Category":
                        categoryErrorTextBlock.Text = errorMessage;
                        categoryErrorTextBlock.Visibility = Visibility.Visible;
                        break;
                    case "Description":
                        descriptionErrorTextBlock.Text = errorMessage;
                        descriptionErrorTextBlock.Visibility = Visibility.Visible;
                        break;
                    case "Tags":
                        tagsErrorTextBlock.Text = errorMessage;
                        tagsErrorTextBlock.Visibility = Visibility.Visible;
                        break;
                    case "Condition":
                        conditionErrorTextBlock.Text = errorMessage;
                        conditionErrorTextBlock.Visibility = Visibility.Visible;
                        break;
                }
                return;
            }

            // Convert string tags to ProductTag objects
            List<ProductTag> productTags = tagManagementHelper.ConvertStringTagsToProductTags(tags);

            // Collect specific data based on the selected type
            if (viewModel is CreateBuyListingViewModel)
            {
                string priceText = ((TextBox)FormContainer.FindName("PriceTextBox")).Text;
                if (!validationService.ValidateBuyProductFields(priceText, out float price))
                {
                    titleErrorTextBlock.Text = "Please enter a valid price.";
                    titleErrorTextBlock.Visibility = Visibility.Visible;
                    return;
                }
                var product = new BuyProduct(0, title, description, App.CurrentUser, condition, category, productTags, viewModel.Images, price);
                viewModel.CreateListing(product);
            }
            else if (viewModel is CreateBorrowListingViewModel)
            {
                var timeLimitDatePicker = (CalendarDatePicker)FormContainer.FindName("TimeLimitDatePicker");
                if (!timeLimitDatePicker.Date.HasValue)
                {
                    titleErrorTextBlock.Text = "Please select a time limit.";
                    titleErrorTextBlock.Visibility = Visibility.Visible;
                    return;
                }
                DateTime endDate = timeLimitDatePicker.Date.Value.DateTime;

                if (!float.TryParse(((TextBox)FormContainer.FindName("DailyRateTextBox")).Text, out float dailyRate))
                {
                    titleErrorTextBlock.Text = "Please enter a valid daily rate.";
                    titleErrorTextBlock.Visibility = Visibility.Visible;
                    return;
                }

                var product = new BorrowProduct(0, title, description, App.CurrentUser, condition, category, productTags, viewModel.Images, DateTime.Now, endDate, endDate, dailyRate, false);
                viewModel.CreateListing(product);
            }
            else if (viewModel is CreateAuctionListingViewModel)
            {
                if (!float.TryParse(((TextBox)FormContainer.FindName("StartingPriceTextBox")).Text, out float startingPrice))
                {
                    titleErrorTextBlock.Text = "Please enter a valid starting price.";
                    titleErrorTextBlock.Visibility = Visibility.Visible;
                    return;
                }

                if (!((CalendarDatePicker)FormContainer.FindName("EndAuctionDatePicker")).Date.HasValue)
                {
                    titleErrorTextBlock.Text = "Please select an end auction date.";
                    titleErrorTextBlock.Visibility = Visibility.Visible;
                    return;
                }
                DateTime endAuctionDate = ((CalendarDatePicker)FormContainer.FindName("EndAuctionDatePicker")).Date.Value.DateTime;

                var product = new AuctionProduct(0, title, description, App.CurrentUser, condition, category, productTags, viewModel.Images, DateTime.Now, endAuctionDate, startingPrice);
                viewModel.CreateListing(product);
            }
            ShowSuccessMessage("Listing created successfully!");
        }
    }
}

