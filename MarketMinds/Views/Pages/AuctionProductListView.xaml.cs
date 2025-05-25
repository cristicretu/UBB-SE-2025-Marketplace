using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MarketMinds.Shared.Models;
using MarketMinds.ViewModels;
using BusinessLogicLayer.ViewModel;
using MarketMinds;
using MarketMinds.Views.Pages;
using MarketMinds.Helpers.ViewModelHelpers;
using MarketMinds.Shared.Services.AuctionSortTypeConverterService;
using MarketMinds.Shared.Services.AuctionProductsService;
using MarketMinds.Shared.Services.Interfaces;

namespace MarketMinds.Views
{
    public sealed partial class AuctionProductListView : Window
    {
        private readonly AuctionProductsViewModel auctionProductsViewModel;
        private readonly SortAndFilterViewModel<AuctionProductsService> sortAndFilterViewModel;
        private ObservableCollection<AuctionProduct> auctionProducts;
        private CompareProductsViewModel compareProductsViewModel;
        private readonly AuctionProductListViewModelHelper auctionProductListViewModelHelper;
        private readonly AuctionSortTypeConverterService sortTypeConverterService;

        // Pagination variables
        private int currentPage = 1;
        private int itemsPerPage = 20;
        private int totalPages = 1;
        private const int BASE_PAGE = 1;
        private const int NO_ITEMS = 0;
        private List<AuctionProduct> currentFullList;

        public AuctionProductListView(SortAndFilterViewModel<AuctionProductsService> sortAndFilterViewModel)
        {
            this.InitializeComponent();

            auctionProductsViewModel = MarketMinds.App.AuctionProductsViewModel;
            this.sortAndFilterViewModel = sortAndFilterViewModel;
            compareProductsViewModel = MarketMinds.App.CompareProductsViewModel;
            auctionProductListViewModelHelper = new AuctionProductListViewModelHelper(sortAndFilterViewModel, auctionProductsViewModel);
            sortTypeConverterService = new AuctionSortTypeConverterService();

            auctionProducts = new ObservableCollection<AuctionProduct>();
            // Initially load all auction products
            currentFullList = auctionProductsViewModel.GetAllProducts();
            ApplyFiltersAndPagination();
        }

        private void AuctionListView_ItemClick(object sender, ItemClickEventArgs itemClickEventArgs)
        {
            var selectedProduct = itemClickEventArgs.ClickedItem as AuctionProduct;
            if (selectedProduct != null)
            {
                // Create and show the detail view
                var detailView = new AuctionProductView(selectedProduct);
                detailView.Activate();
            }
        }

        // Call this method whenever a filter, sort, or search query changes.
        private void ApplyFiltersAndPagination()
        {
            var (pageItems, newTotalPages, fullList) = auctionProductListViewModelHelper.GetAuctionProductsPage(
                auctionProductsViewModel, sortAndFilterViewModel, currentPage, itemsPerPage);
            currentFullList = fullList;
            totalPages = newTotalPages;
            auctionProducts.Clear();
            foreach (var item in pageItems)
            {
                auctionProducts.Add(item);
            }

            // Show the empty message if no items exist
            EmptyMessageTextBlock.Visibility = auctionProductListViewModelHelper.ShouldShowEmptyMessage(pageItems)
                ? Visibility.Visible
                : Visibility.Collapsed;

            UpdatePaginationDisplay();
        }

        private void UpdatePaginationDisplay()
        {
            PaginationTextBlock.Text = auctionProductListViewModelHelper.GetPaginationText(currentPage, totalPages);
            var (canGoToPrevious, canGoToNext) = auctionProductListViewModelHelper.GetPaginationButtonState(
                currentPage, totalPages, BASE_PAGE);
            PreviousButton.IsEnabled = canGoToPrevious;
            NextButton.IsEnabled = canGoToNext;
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            if (currentPage > BASE_PAGE)
            {
                currentPage--;
                ApplyFiltersAndPagination();
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            if (currentPage < totalPages)
            {
                currentPage++;
                ApplyFiltersAndPagination();
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            // Update the search query in the view model and reapply filters
            sortAndFilterViewModel.HandleSearchQueryChange(SearchTextBox.Text);
            currentPage = BASE_PAGE;
            ApplyFiltersAndPagination();
        }

        private async void FilterButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            // Cast to IProductService version for FilterDialog compatibility
            SortAndFilterViewModel<IProductService> dialogViewModel = (SortAndFilterViewModel<IProductService>)(object)sortAndFilterViewModel;
            // Show a ContentDialog for filtering
            FilterDialog filterDialog = new FilterDialog(dialogViewModel);
            filterDialog.XamlRoot = Content.XamlRoot;
            var result = await filterDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                // Filters have been applied in the dialog; reapply them.
                currentPage = BASE_PAGE;
                ApplyFiltersAndPagination();
            }
        }

        private void SortButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            // Toggle the sorting dropdown visibility
            SortingComboBox.Visibility = SortingComboBox.Visibility == Visibility.Visible ?
                                         Visibility.Collapsed : Visibility.Visible;
        }

        private void SortingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            if (SortingComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                // Use the Tag to determine which sort to apply
                var sortTag = selectedItem.Tag.ToString();
                var sortType = sortTypeConverterService.Convert(sortTag);
                if (sortType != null)
                {
                    sortAndFilterViewModel.HandleSortChange(sortType);
                    currentPage = BASE_PAGE; // Reset to first page on new sort
                    ApplyFiltersAndPagination();
                }
            }
        }

        private void AddToCompare_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            var button = sender as Button;
            var selectedProduct = button.DataContext as Product;
            if (selectedProduct != null)
            {
                bool twoAdded = compareProductsViewModel.AddProductForCompare(selectedProduct);
                if (twoAdded == true)
                {
                    // Create a compare view
                    var compareProductsView = new CompareProductsView(compareProductsViewModel);
                    // Create a window to host the CompareProductsView page
                    var compareWindow = new Window();
                    compareWindow.Content = compareProductsView;
                    compareProductsView.SetParentWindow(compareWindow);
                    compareWindow.Activate();
                }
            }
        }
    }
}
