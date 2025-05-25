using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MarketMinds.Shared.Models;
using ViewModelLayer.ViewModel;
using BusinessLogicLayer.ViewModel;
using MarketMinds;
using MarketMinds.Helpers.ViewModelHelpers;
using MarketMinds.Views.Pages;
using MarketMinds.Shared.Services.BorrowSortTypeConverterService;
using MarketMinds.Shared.Services.BorrowProductsService;
using MarketMinds.Shared.Services.Interfaces;

namespace MarketMinds.Views
{
    public sealed partial class BorrowProductListView : Window
    {
        private readonly BorrowProductsViewModel borrowProductsViewModel;
        private readonly SortAndFilterViewModel<BorrowProductsService> sortAndFilterViewModel;
        private ObservableCollection<BorrowProduct> borrowProducts;
        private CompareProductsViewModel compareProductsViewModel;
        private readonly BorrowProductListViewModelHelper borrowProductListViewModelHelper;
        private readonly BorrowSortTypeConverterService sortTypeConverterService;

        // Pagination variables
        private int current_page = 1; // Current page number, default to 1
        private int total_page_count = 1;
        private const int FIRST_PAGE = 1;
        private List<BorrowProduct> currentFullList;

        public BorrowProductListView(SortAndFilterViewModel<BorrowProductsService> sortAndFilterViewModel)
        {
            this.InitializeComponent();

            // Initialize view models and services
            borrowProductsViewModel = MarketMinds.App.BorrowProductsViewModel;
            this.sortAndFilterViewModel = sortAndFilterViewModel;
            compareProductsViewModel = MarketMinds.App.CompareProductsViewModel;
            borrowProductListViewModelHelper = new BorrowProductListViewModelHelper(sortAndFilterViewModel, borrowProductsViewModel);
            sortTypeConverterService = new BorrowSortTypeConverterService();

            borrowProducts = new ObservableCollection<BorrowProduct>();
            currentFullList = borrowProductsViewModel.GetAllProducts();
            RefreshProductList();
        }

        private void BorrowListView_ItemClick(object sender, ItemClickEventArgs itemClickEventArgs)
        {
            var selectedProduct = itemClickEventArgs.ClickedItem as BorrowProduct;
            if (selectedProduct != null)
            {
                // Create and show the detail view
                // var detailView = new BorrowProductView(selectedProduct);
                var detailView = new BorrowProductWindow(selectedProduct.Id);
                detailView.Activate();
            }
        }

        private void RefreshProductList()
        {
            var (pageItems, newTotalPages, fullList) = borrowProductListViewModelHelper.GetBorrowProductsPage(
                borrowProductsViewModel, sortAndFilterViewModel, current_page);
            currentFullList = fullList;
            total_page_count = newTotalPages;
            borrowProducts.Clear();
            foreach (var item in pageItems)
            {
                borrowProducts.Add(item);
            }
            // Update UI elements
            EmptyMessageTextBlock.Visibility = borrowProducts.Count == 0 ?
                Visibility.Visible : Visibility.Collapsed;
            UpdatePaginationDisplay();
        }

        private void UpdatePaginationDisplay()
        {
            PaginationTextBlock.Text = borrowProductListViewModelHelper.GetPaginationText(current_page, total_page_count);
            var (hasPrevious, hasNext) = borrowProductListViewModelHelper.GetPaginationState(current_page, total_page_count);
            PreviousButton.IsEnabled = hasPrevious;
            NextButton.IsEnabled = hasNext;
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            if (current_page > FIRST_PAGE)
            {
                current_page--;
                RefreshProductList();
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            if (current_page < total_page_count)
            {
                current_page++;
                RefreshProductList();
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs routedEventArgs)
        {
            sortAndFilterViewModel.HandleSearchQueryChange(SearchTextBox.Text);
            current_page = FIRST_PAGE; // Reset to first page on new search
            RefreshProductList();
        }

        private async void FilterButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            // Cast to IProductService version for FilterDialog compatibility
            SortAndFilterViewModel<IProductService> dialogViewModel = (SortAndFilterViewModel<IProductService>)(object)sortAndFilterViewModel;
            FilterDialog filterDialog = new FilterDialog(dialogViewModel);
            filterDialog.XamlRoot = Content.XamlRoot;
            var result = await filterDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                current_page = FIRST_PAGE; // Reset to first page on new filter
                RefreshProductList();
            }
        }

        private void SortButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            SortingComboBox.Visibility = SortingComboBox.Visibility == Visibility.Visible ?
                                         Visibility.Collapsed : Visibility.Visible;
        }

        private void SortingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            if (SortingComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                var sortTag = selectedItem.Tag.ToString();
                var sortType = sortTypeConverterService.Convert(sortTag);
                if (sortType != null)
                {
                    sortAndFilterViewModel.HandleSortChange(sortType);
                    current_page = FIRST_PAGE; // Reset to first page on new sort
                    RefreshProductList();
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
