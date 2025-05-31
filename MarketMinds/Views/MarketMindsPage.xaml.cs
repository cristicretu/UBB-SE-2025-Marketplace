using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
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
using MarketMinds.ViewModels;
using MarketMinds.Shared.Services.ProductCategoryService;
using MarketMinds.Shared.Services.ProductConditionService;

namespace MarketMinds.Views
{
    public sealed partial class MarketMindsPage : Page, INotifyPropertyChanged
    {
        // Property changed event
        public event PropertyChangedEventHandler PropertyChanged;

        // view models with backing fields for debugging
        private BuyProductsViewModel buyProductsViewModel;
        public BuyProductsViewModel BuyProductsViewModel
        {
            get => buyProductsViewModel;
            set
            {
                Debug.WriteLine($"BuyProductsViewModel setter called. Old: {buyProductsViewModel?.GetType().Name ?? "NULL"}, New: {value?.GetType().Name ?? "NULL"}");
                Debug.WriteLine($"Setter called from: {Environment.StackTrace}");
                buyProductsViewModel = value;
            }
        }

        public AuctionProductsViewModel AuctionProductsViewModel { get; set; }
        public BorrowProductsViewModel BorrowProductsViewModel { get; set; }
        public BuyerWishlistItemViewModel BuyerWishlistItemViewModel { get; set; }
        public ShoppingCartViewModel ShoppingCartViewModel { get; set; }
        public ProductCategoryViewModel ProductCategoryViewModel { get; set; }
        public ProductConditionViewModel ProductConditionViewModel { get; set; }

        // observable collections
        public ObservableCollection<BuyProduct> BuyProductsCollection { get; private set; }
        public ObservableCollection<AuctionProduct> AuctionProductsCollection { get; private set; }
        public ObservableCollection<BorrowProduct> BorrowProductsCollection { get; private set; }
        public ObservableCollection<Category> Categories { get; private set; }
        public ObservableCollection<Condition> Conditions { get; private set; }

        // User role properties
        public bool IsCurrentUserBuyer => App.CurrentUser?.UserType == 2;
        public bool IsCurrentUserSeller => App.CurrentUser?.UserType == 3;

        // Current active tab tracking
        private int activeTabIndex = 0;
        public int ActiveTabIndex
        {
            get => activeTabIndex;
            private set
            {
                if (SetProperty(ref activeTabIndex, value))
                {
                    // Update pagination visibility when tab changes
                    OnPropertyChanged(nameof(ShowPaginationForCurrentTab));

                    // Update all computed visibility properties
                    OnPropertyChanged(nameof(ShowBuyLoadingIndicator));
                    OnPropertyChanged(nameof(ShowAuctionLoadingIndicator));
                    OnPropertyChanged(nameof(ShowBorrowLoadingIndicator));
                    OnPropertyChanged(nameof(ShowBuyEmptyState));
                    OnPropertyChanged(nameof(ShowAuctionEmptyStateForTab));
                    OnPropertyChanged(nameof(ShowBorrowEmptyStateForTab));
                }
            }
        }

        // Property to control pagination visibility based on current tab
        public bool ShowPaginationForCurrentTab
        {
            get
            {
                // Show pagination for Buy Products (0), Auction Products (1), and Borrow Products (2)
                return ActiveTabIndex == 0 || ActiveTabIndex == 1 || ActiveTabIndex == 2;
            }
        }

        // Properties to control which loading indicator and empty state to show
        public bool ShowBuyLoadingIndicator => ActiveTabIndex == 0 && IsLoading;
        public bool ShowAuctionLoadingIndicator => ActiveTabIndex == 1 && IsAuctionLoading;
        public bool ShowBorrowLoadingIndicator => ActiveTabIndex == 2 && IsBorrowLoading;

        public bool ShowBuyEmptyState => ActiveTabIndex == 0 && ShowEmptyState;
        public bool ShowAuctionEmptyStateForTab => ActiveTabIndex == 1 && ShowAuctionEmptyState;
        public bool ShowBorrowEmptyStateForTab => ActiveTabIndex == 2 && ShowBorrowEmptyState;

        // Pagination properties - now shared between tabs
        private int currentPageIndex = 0;
        private bool isInitializing = true; // Flag to prevent data loading during initialization

        public int CurrentPageIndex
        {
            get => currentPageIndex;
            set
            {
                if (SetProperty(ref currentPageIndex, value) && !isInitializing)
                {
                    // Load data based on active tab
                    switch (ActiveTabIndex)
                    {
                        case 0: // Buy Products
                            LoadBuyDataAsync();
                            break;
                        case 1: // Auction Products
                            LoadAuctionDataAsync();
                            break;
                        case 2: // Borrow Products
                            LoadBorrowDataAsync();
                            break;
                    }
                }
            }
        }

        private int totalPages = 1;
        public int TotalPages
        {
            get => totalPages;
            private set => SetProperty(ref totalPages, value);
        }

        private int itemsPerPage = 10;
        public int ItemsPerPage
        {
            get => itemsPerPage;
            private set
            {
                if (SetProperty(ref itemsPerPage, value) && !isInitializing)
                {
                    // Reset to first page and trigger data loading based on active tab
                    CurrentPageIndex = 0;
                    // Explicitly call load method based on active tab
                    switch (ActiveTabIndex)
                    {
                        case 0: // Buy Products
                            LoadBuyDataAsync();
                            break;
                        case 1: // Auction Products
                            LoadAuctionDataAsync();
                            break;
                        case 2: // Borrow Products
                            LoadBorrowDataAsync();
                            break;
                    }
                }
            }
        }

        private string pageInfoText = string.Empty;
        public string PageInfoText
        {
            get => pageInfoText;
            private set => SetProperty(ref pageInfoText, value);
        }

        // properties
        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            private set
            {
                if (SetProperty(ref isLoading, value))
                {
                    OnPropertyChanged(nameof(ShowBuyLoadingIndicator));
                }
            }
        }

        private bool showEmptyState;
        public bool ShowEmptyState
        {
            get => showEmptyState;
            private set
            {
                if (SetProperty(ref showEmptyState, value))
                {
                    OnPropertyChanged(nameof(ShowBuyEmptyState));
                }
            }
        }

        private bool isAuctionLoading;
        public bool IsAuctionLoading
        {
            get => isAuctionLoading;
            private set
            {
                if (SetProperty(ref isAuctionLoading, value))
                {
                    OnPropertyChanged(nameof(ShowAuctionLoadingIndicator));
                }
            }
        }

        private bool showAuctionEmptyState;
        public bool ShowAuctionEmptyState
        {
            get => showAuctionEmptyState;
            private set
            {
                if (SetProperty(ref showAuctionEmptyState, value))
                {
                    OnPropertyChanged(nameof(ShowAuctionEmptyStateForTab));
                }
            }
        }

        private bool isBorrowLoading;
        public bool IsBorrowLoading
        {
            get => isBorrowLoading;
            private set
            {
                if (SetProperty(ref isBorrowLoading, value))
                {
                    OnPropertyChanged(nameof(ShowBorrowLoadingIndicator));
                }
            }
        }

        private bool showBorrowEmptyState;
        public bool ShowBorrowEmptyState
        {
            get => showBorrowEmptyState;
            private set
            {
                if (SetProperty(ref showBorrowEmptyState, value))
                {
                    OnPropertyChanged(nameof(ShowBorrowEmptyStateForTab));
                }
            }
        }

        // Filter properties
        private string searchTerm = string.Empty;
        public string SearchTerm
        {
            get => searchTerm;
            set => SetProperty(ref searchTerm, value);
        }

        private double maxPrice = 100101;
        public double MaxPrice
        {
            get => maxPrice;
            set => SetProperty(ref maxPrice, value);
        }

        private List<int> selectedCategoryIds = new List<int>();
        private List<int> selectedConditionIds = new List<int>();

        // Debouncing timers for filters
        private DispatcherTimer priceFilterTimer;
        private DispatcherTimer searchFilterTimer;

        public MarketMindsPage()
        {
            this.InitializeComponent();

            // Get ViewModels from App with null checks to ensure they're initialized
            this.BuyProductsViewModel = App.BuyProductsViewModel ?? throw new InvalidOperationException("BuyProductsViewModel not initialized");
            this.AuctionProductsViewModel = App.AuctionProductsViewModel ?? throw new InvalidOperationException("AuctionProductsViewModel not initialized");
            this.BorrowProductsViewModel = App.BorrowProductsViewModel ?? throw new InvalidOperationException("BorrowProductsViewModel not initialized");
            this.BuyerWishlistItemViewModel = App.BuyerWishlistItemViewModel ?? throw new InvalidOperationException("BuyerWishlistItemViewModel not initialized");
            this.ShoppingCartViewModel = App.ShoppingCartViewModel ?? throw new InvalidOperationException("ShoppingCartViewModel not initialized");
            this.ProductCategoryViewModel = App.ProductCategoryViewModel ?? throw new InvalidOperationException("ProductCategoryViewModel not initialized");
            this.ProductConditionViewModel = App.ProductConditionViewModel ?? throw new InvalidOperationException("ProductConditionViewModel not initialized");

            this.ShoppingCartViewModel.BuyerId = App.CurrentUser.Id;

            this.BuyProductsCollection = new ObservableCollection<BuyProduct>();
            this.AuctionProductsCollection = new ObservableCollection<AuctionProduct>();
            this.BorrowProductsCollection = new ObservableCollection<BorrowProduct>();
            this.Categories = new ObservableCollection<Category>();
            this.Conditions = new ObservableCollection<Condition>();

            // Load categories and conditions
            LoadCategoriesAndConditions();
            
            // Initialize debouncing timers
            InitializeFilterTimers();
            
            // Set up the page loaded event to initialize UI elements
            this.Loaded += MarketMindsPage_Loaded;

            // Set Buy Products as the default selection - this will trigger ProductsPivot_SelectionChanged
            ProductsPivot.SelectedIndex = 0;
        }

        /// <summary>
        /// Initializes the debouncing timers for filters
        /// </summary>
        private void InitializeFilterTimers()
        {
            // Initialize price filter timer
            priceFilterTimer = new DispatcherTimer();
            priceFilterTimer.Interval = TimeSpan.FromMilliseconds(500);
            priceFilterTimer.Tick += PriceFilterTimer_Tick;

            // Initialize search filter timer
            searchFilterTimer = new DispatcherTimer();
            searchFilterTimer.Interval = TimeSpan.FromMilliseconds(500);
            searchFilterTimer.Tick += SearchFilterTimer_Tick;
        }

        /// <summary>
        /// Handles the page loaded event to initialize UI elements
        /// </summary>
        private void MarketMindsPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize the price text display now that the UI is loaded
            if (SelectedPriceText != null)
            {
                SelectedPriceText.Text = $"${MaxPrice:F0}";
            }
        }

        /// <summary>
        /// Loads categories and conditions from the services
        /// </summary>
        private void LoadCategoriesAndConditions()
        {
            try
            {
                // Load categories
                var categories = this.ProductCategoryViewModel.GetAllProductCategories();
                Categories.Clear();
                foreach (var category in categories)
                {
                    Categories.Add(category);
                }

                // Load conditions
                var conditions = this.ProductConditionViewModel.GetAllProductConditions();
                Conditions.Clear();
                foreach (var condition in conditions)
                {
                    Conditions.Add(condition);
                }

                Debug.WriteLine($"Loaded {Categories.Count} categories and {Conditions.Count} conditions");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading categories and conditions: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles search box query submission
        /// </summary>
        private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            SearchTerm = args.QueryText;
            ApplyFilters();
        }

        /// <summary>
        /// Handles search box text changes with debouncing
        /// </summary>
        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // Only apply filters if user is typing (not if the text was programmatically changed)
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                SearchTerm = sender.Text;
                
                // Reset the timer - this creates a debouncing effect
                searchFilterTimer.Stop();
                searchFilterTimer.Start();
            }
        }

        /// <summary>
        /// Handles price slider value changes with debouncing
        /// </summary>
        private void PriceRangeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            MaxPrice = e.NewValue;
            
            // Update the price text display only if the control is initialized
            if (SelectedPriceText != null)
            {
                SelectedPriceText.Text = $"${MaxPrice:F0}";
            }
            
            // Reset the timer - this creates a debouncing effect
            if (priceFilterTimer != null)
            {
                priceFilterTimer.Stop();
                priceFilterTimer.Start();
            }
        }

        /// <summary>
        /// Handles the price filter timer tick event
        /// </summary>
        private void PriceFilterTimer_Tick(object sender, object e)
        {
            priceFilterTimer.Stop();
            ApplyFilters();
        }

        /// <summary>
        /// Handles the search filter timer tick event
        /// </summary>
        private void SearchFilterTimer_Tick(object sender, object e)
        {
            searchFilterTimer.Stop();
            ApplyFilters();
        }

        /// <summary>
        /// Handles category checkbox changes
        /// </summary>
        private void CategoryCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                int categoryId = (int)checkBox.Tag;
                
                if (checkBox.IsChecked == true)
                {
                    if (!selectedCategoryIds.Contains(categoryId))
                    {
                        selectedCategoryIds.Add(categoryId);
                    }
                }
                else
                {
                    selectedCategoryIds.Remove(categoryId);
                }
                
                ApplyFilters();
            }
        }

        /// <summary>
        /// Handles condition checkbox changes
        /// </summary>
        private void ConditionCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                int conditionId = (int)checkBox.Tag;
                
                if (checkBox.IsChecked == true)
                {
                    if (!selectedConditionIds.Contains(conditionId))
                    {
                        selectedConditionIds.Add(conditionId);
                    }
                }
                else
                {
                    selectedConditionIds.Remove(conditionId);
                }
                
                ApplyFilters();
            }
        }

        /// <summary>
        /// Applies all filters and reloads data
        /// </summary>
        private void ApplyFilters()
        {
            CurrentPageIndex = 0; // Reset to first page when filters change
            
            // Reload data for the current tab with the applied filters
            switch (ActiveTabIndex)
            {
                case 0: // Buy Products
                    LoadBuyDataAsync();
                    break;
                case 1: // Auction Products
                    LoadAuctionDataAsync();
                    break;
                case 2: // Borrow Products
                    LoadBorrowDataAsync();
                    break;
            }
        }

        /// <summary>
        /// Handles the Clear Filters button click
        /// </summary>
        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            // Reset filter values
            SearchBox.Text = string.Empty;
            SearchTerm = string.Empty;
            PriceRangeSlider.Value = PriceRangeSlider.Maximum;
            MaxPrice = PriceRangeSlider.Maximum;
            SelectedPriceText.Text = $"${MaxPrice}";
            
            // Clear selected categories and conditions
            selectedCategoryIds.Clear();
            selectedConditionIds.Clear();
            
            // Uncheck all category checkboxes by iterating through the data source
            ClearCategoryCheckboxes();
            ClearConditionCheckboxes();

            // Reset pagination to first page and reload data for the active tab
            ApplyFilters();
        }

        /// <summary>
        /// Helper method to clear category checkboxes
        /// </summary>
        private void ClearCategoryCheckboxes()
        {
            // Find all CheckBox elements in the categories repeater
            var categoryContainer = CategoriesRepeater.Parent as ScrollViewer;
            if (categoryContainer != null)
            {
                ClearCheckboxesInContainer(categoryContainer);
            }
        }

        /// <summary>
        /// Helper method to clear condition checkboxes
        /// </summary>
        private void ClearConditionCheckboxes()
        {
            // Find all CheckBox elements in the conditions repeater
            var conditionContainer = ConditionsRepeater.Parent as ScrollViewer;
            if (conditionContainer != null)
            {
                ClearCheckboxesInContainer(conditionContainer);
            }
        }

        /// <summary>
        /// Recursively finds and unchecks all checkboxes in a container
        /// </summary>
        private void ClearCheckboxesInContainer(DependencyObject container)
        {
            if (container == null) return;

            int childCount = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(container);
            for (int i = 0; i < childCount; i++)
            {
                var child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(container, i);
                
                if (child is CheckBox checkBox)
                {
                    checkBox.IsChecked = false;
                }
                else
                {
                    // Recursively search in child elements
                    ClearCheckboxesInContainer(child);
                }
            }
        }

        /// <summary>
        /// Loads data asynchronously with filters applied
        /// </summary>
        private async void LoadBuyDataAsync()
        {
            try
            {
                IsLoading = true;

                // Retry mechanism with limit (safer than recursion)
                // Alex: I ran with debug and found out that this function is called before the constructor is finished, so I added a delay to the retry, 100% success rate
                const int maxRetries = 5;
                int retryCount = 0;

                while (this.BuyProductsViewModel == null && retryCount < maxRetries)
                {
                    // Try to get it again
                    this.BuyProductsViewModel = App.BuyProductsViewModel;

                    if (this.BuyProductsViewModel == null)
                    {
                        retryCount++;
                        await Task.Delay(50); // Short delay between retries
                    }
                    else
                    {
                        Debug.WriteLine($"Successfully got ViewModel on retry {retryCount + 1}");
                    }
                }

                if (this.BuyProductsViewModel == null)
                {
                    Debug.WriteLine("BuyProductsViewModel is still null after all retry attempts");
                    ShowEmptyState = true;
                    return;
                }

                // Calculate offset for current page
                int offset = CurrentPageIndex * ItemsPerPage;

                // Get total count with filters applied
                int totalCount;
                try {
                    totalCount = await this.BuyProductsViewModel.GetFilteredProductCountAsync(
                        selectedConditionIds.Count > 0 ? selectedConditionIds : null,
                        selectedCategoryIds.Count > 0 ? selectedCategoryIds : null,
                        MaxPrice < 100101 ? MaxPrice : null,
                        !string.IsNullOrWhiteSpace(SearchTerm) ? SearchTerm : null
                    );
                    
                    if (totalCount < 0) { // Sanity check
                        Debug.WriteLine("Warning: Got negative count from API, defaulting to 0");
                        totalCount = 0;
                    }
                }
                catch (Exception countEx) {
                    Debug.WriteLine($"Error getting product count: {countEx.Message}");
                    totalCount = 0; // Default to zero if count fails
                }

                // Calculate total pages (minimum 1 page)
                TotalPages = Math.Max(1, (int)Math.Ceiling((double)totalCount / ItemsPerPage));

                // Ensure current page is valid
                if (CurrentPageIndex >= TotalPages)
                {
                    CurrentPageIndex = Math.Max(0, TotalPages - 1);
                    offset = CurrentPageIndex * ItemsPerPage;
                }

                // Get filtered products for current page
                List<BuyProduct> products = new List<BuyProduct>();
                try
                {
                    products = await this.BuyProductsViewModel.GetFilteredProductsAsync(
                        offset, 
                        ItemsPerPage, // Ensure we're only fetching the exact amount needed for the page
                        selectedConditionIds.Count > 0 ? selectedConditionIds : null,
                        selectedCategoryIds.Count > 0 ? selectedCategoryIds : null,
                        MaxPrice < 100101 ? MaxPrice : null,
                        !string.IsNullOrWhiteSpace(SearchTerm) ? SearchTerm : null
                    );
                    Debug.WriteLine($"Fetched {products.Count} products for page {CurrentPageIndex + 1} with {ItemsPerPage} items per page");
                }
                catch (Exception fetchEx)
                {
                    Debug.WriteLine($"Error fetching products: {fetchEx.Message}");
                    products = new List<BuyProduct>(); // Empty list on error
                }

                // Update UI on the UI thread
                BuyProductsCollection.Clear();
                foreach (var product in products)
                {
                    BuyProductsCollection.Add(product);
                }

                // Update pagination info
                UpdatePaginationInfo(totalCount);

                // Update empty state and pagination controls visibility
                ShowEmptyState = BuyProductsCollection.Count == 0 && totalCount == 0;

                Debug.WriteLine($"BuyProductsCollection: {BuyProductsCollection.Count}, Total: {totalCount}, Page: {CurrentPageIndex + 1}/{TotalPages}");
                Debug.WriteLine($"Applied Filters - Categories: {string.Join(", ", selectedCategoryIds)}, Conditions: {string.Join(", ", selectedConditionIds)}, MaxPrice: {MaxPrice}, SearchTerm: {SearchTerm}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading products: {ex.Message}");
                ShowEmptyState = true;
            }
            finally
            {
                IsLoading = false;
                isInitializing = false; // Allow future page changes to trigger data loading
            }
        }

        /// <summary>
        /// Updates pagination information text
        /// </summary>
        private void UpdatePaginationInfo(int totalCount)
        {
            int startItem = totalCount > 0 ? (CurrentPageIndex * ItemsPerPage) + 1 : 0;
            int endItem = Math.Min((CurrentPageIndex + 1) * ItemsPerPage, totalCount);
            PageInfoText = $"Showing {startItem}-{endItem} of {totalCount} products";
        }

        /// <summary>
        /// Handles items per page selection changes
        /// </summary>
        private void ItemsPerPageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                if (int.TryParse(selectedItem.Tag.ToString(), out int newItemsPerPage))
                {
                    ItemsPerPage = newItemsPerPage; // This will reset CurrentPageIndex to 0 and trigger LoadBuyDataAsync()
                }
            }
        }

        /// <summary>
        /// Loads auction data asynchronously with filters applied
        /// </summary>
        private async void LoadAuctionDataAsync()
        {
            try
            {
                IsAuctionLoading = true;

                // Calculate offset for current page
                int offset = CurrentPageIndex * ItemsPerPage;

                // Get total count with filters applied
                int totalCount;
                try {
                    totalCount = await this.AuctionProductsViewModel.GetFilteredProductCountAsync(
                        selectedConditionIds.Count > 0 ? selectedConditionIds : null,
                        selectedCategoryIds.Count > 0 ? selectedCategoryIds : null,
                        MaxPrice < 100101 ? MaxPrice : null,
                        !string.IsNullOrWhiteSpace(SearchTerm) ? SearchTerm : null
                    );
                    
                    if (totalCount < 0) { // Sanity check
                        Debug.WriteLine("Warning: Got negative auction count from API, defaulting to 0");
                        totalCount = 0;
                    }
                }
                catch (Exception countEx) {
                    Debug.WriteLine($"Error getting auction product count: {countEx.Message}");
                    totalCount = 0; // Default to zero if count fails
                }

                // Calculate total pages (minimum 1 page)
                TotalPages = Math.Max(1, (int)Math.Ceiling((double)totalCount / ItemsPerPage));

                // Ensure current page is valid
                if (CurrentPageIndex >= TotalPages)
                {
                    CurrentPageIndex = Math.Max(0, TotalPages - 1);
                    offset = CurrentPageIndex * ItemsPerPage;
                }

                // Get auction products for current page with filters
                List<AuctionProduct> auctionProducts = new List<AuctionProduct>();
                try
                {
                    auctionProducts = await this.AuctionProductsViewModel.GetFilteredProductsAsync(
                        offset, 
                        ItemsPerPage, // Ensure we're only fetching the exact amount needed for the page
                        selectedConditionIds.Count > 0 ? selectedConditionIds : null,
                        selectedCategoryIds.Count > 0 ? selectedCategoryIds : null,
                        MaxPrice < 100101 ? MaxPrice : null,
                        !string.IsNullOrWhiteSpace(SearchTerm) ? SearchTerm : null
                    );
                    Debug.WriteLine($"Fetched {auctionProducts.Count} auction products for page {CurrentPageIndex + 1} with {ItemsPerPage} items per page");
                }
                catch (Exception fetchEx)
                {
                    Debug.WriteLine($"Error fetching auction products: {fetchEx.Message}");
                    auctionProducts = new List<AuctionProduct>(); // Empty list on error
                }

                // Update UI on the UI thread
                AuctionProductsCollection.Clear();
                foreach (var product in auctionProducts)
                {
                    AuctionProductsCollection.Add(product);
                }

                // Update pagination info
                UpdatePaginationInfo(totalCount);

                // Update empty state based on collection
                ShowAuctionEmptyState = AuctionProductsCollection.Count == 0 && totalCount == 0;
                Debug.WriteLine($"AuctionProductsCollection: {AuctionProductsCollection.Count}, Total: {totalCount}, Page: {CurrentPageIndex + 1}/{TotalPages}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading auction products: {ex.Message}");
                ShowAuctionEmptyState = true;
            }
            finally
            {
                IsAuctionLoading = false;
            }
        }

        /// <summary>
        /// Loads borrow data asynchronously with filters applied
        /// </summary>
        private async void LoadBorrowDataAsync()
        {
            try
            {
                IsBorrowLoading = true;

                // Calculate offset for current page
                int offset = CurrentPageIndex * ItemsPerPage;

                // Get total count with filters applied
                int totalCount;
                try {
                    totalCount = await this.BorrowProductsViewModel.GetFilteredProductCountAsync(
                        selectedConditionIds.Count > 0 ? selectedConditionIds : null,
                        selectedCategoryIds.Count > 0 ? selectedCategoryIds : null,
                        MaxPrice < 100101 ? MaxPrice : null,
                        !string.IsNullOrWhiteSpace(SearchTerm) ? SearchTerm : null
                    );
                    
                    if (totalCount < 0) { // Sanity check
                        Debug.WriteLine("Warning: Got negative borrow count from API, defaulting to 0");
                        totalCount = 0;
                    }
                }
                catch (Exception countEx) {
                    Debug.WriteLine($"Error getting borrow product count: {countEx.Message}");
                    totalCount = 0; // Default to zero if count fails
                }

                // Calculate total pages (minimum 1 page)
                TotalPages = Math.Max(1, (int)Math.Ceiling((double)totalCount / ItemsPerPage));

                // Ensure current page is valid
                if (CurrentPageIndex >= TotalPages)
                {
                    CurrentPageIndex = Math.Max(0, TotalPages - 1);
                    offset = CurrentPageIndex * ItemsPerPage;
                }

                // Get borrow products for current page with filters
                List<BorrowProduct> borrowProducts = new List<BorrowProduct>();
                try
                {
                    borrowProducts = await this.BorrowProductsViewModel.GetFilteredProductsAsync(
                        offset, 
                        ItemsPerPage, // Ensure we're only fetching the exact amount needed for the page
                        selectedConditionIds.Count > 0 ? selectedConditionIds : null,
                        selectedCategoryIds.Count > 0 ? selectedCategoryIds : null,
                        MaxPrice < 100101 ? MaxPrice : null,
                        !string.IsNullOrWhiteSpace(SearchTerm) ? SearchTerm : null
                    );
                    Debug.WriteLine($"Fetched {borrowProducts.Count} borrow products for page {CurrentPageIndex + 1} with {ItemsPerPage} items per page");
                }
                catch (Exception fetchEx)
                {
                    Debug.WriteLine($"Error fetching borrow products: {fetchEx.Message}");
                    borrowProducts = new List<BorrowProduct>(); // Empty list on error
                }

                // Update UI on the UI thread
                BorrowProductsCollection.Clear();
                foreach (var product in borrowProducts)
                {
                    BorrowProductsCollection.Add(product);
                }

                // Update pagination info
                UpdatePaginationInfo(totalCount);

                // Update empty state based on collection
                ShowBorrowEmptyState = BorrowProductsCollection.Count == 0 && totalCount == 0;
                Debug.WriteLine($"BorrowProductsCollection: {BorrowProductsCollection.Count}, Total: {totalCount}, Page: {CurrentPageIndex + 1}/{TotalPages}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading borrow products: {ex.Message}");
                ShowBorrowEmptyState = true;
            }
            finally
            {
                IsBorrowLoading = false;
            }
        }

        /// <summary>
        /// Helper method to set property and notify changes
        /// </summary>
        private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        /// <summary>
        /// Helper method to manually trigger property change notifications
        /// </summary>
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Handle pivot selection changes
        /// </summary>
        private void ProductsPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Update active tab index
            ActiveTabIndex = ProductsPivot.SelectedIndex;

            // Reset pagination when switching tabs
            CurrentPageIndex = 0;

            // Always load data when switching tabs to ensure fresh data with current pagination settings
            switch (ProductsPivot.SelectedIndex)
            {
                case 0: // Buy Products
                    LoadBuyDataAsync();
                    break;
                case 1: // Auction Products
                    LoadAuctionDataAsync();
                    break;
                case 2: // Borrow Products
                    LoadBorrowDataAsync();
                    break;
            }
        }

        private void BuyProductCard_Click(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is BuyProduct product)
            {
                // Navigate to product details page
                Frame.Navigate(typeof(BuyProductDetailsPage), product);
            }
        }

        private void BuyProductCard_AddToCart_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
            {
                return;
            }

            var product = button.DataContext as BuyProduct;
            if (product == null)
            {
                Debug.WriteLine("[BuyProductCard_AddToCart_Click] Product is null");
                return;
            }

            this.ShoppingCartViewModel.AddToCartAsync(product, 1);
        }

        private void BuyProductCard_AddToWishlist_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
            {
                return;
            }

            var product = button.DataContext as BuyProduct;
            if (product == null)
            {
                Debug.WriteLine("[BuyProductCard_AddToWishlist_Click] Product is null");
                return;
            }

            if (this.BuyerWishlistItemViewModel.IsInWishlist(product.Id))
            {
                this.BuyerWishlistItemViewModel.RemoveFromWishlist(product.Id);
            }
            else
            {
                this.BuyerWishlistItemViewModel.AddToWishlist(product.Id);
            }
        }

        private void AuctionProductCard_Click(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is AuctionProduct product)
            {
                // Navigate to auction product details page
                // Frame.Navigate(typeof(AuctionProductDetailsPage), product);
            }
        }

        private void BorrowProductCard_Click(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is BorrowProduct product)
            {
                // Navigate to borrow product details page
                Frame.Navigate(typeof(BorrowProductDetailsPage), product);
            }
        }
    }
}