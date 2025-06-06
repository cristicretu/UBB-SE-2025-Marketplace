using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BusinessLogicLayer.ViewModel;
using MarketMinds;
using MarketMinds.Helpers.ViewModelHelpers;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;
using MarketMinds.Shared.Services.BuyProductsService;
using MarketMinds.Shared.Services.Interfaces;
using MarketMinds.Shared.Services.ProductPaginationService;
using MarketMinds.Views.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MarketMinds.ViewModels;

namespace MarketMinds.Views
{
    public sealed partial class BuyProductListView : Window
    {
        private readonly BuyProductsViewModel buyProductsViewModel;
        private readonly SortAndFilterViewModel<IProductService> sortAndFilterViewModel;
        private readonly ProductPaginationService paginationService;
        private ObservableCollection<BuyProduct> buyProducts;
        private CompareProductsViewModel compareProductsViewModel;
        private readonly BuyProductListViewModelHelper buyProductListViewModelHelper;

        // Pagination variables
        private int current_page = 1; // Current page number, default to 1
        private int total_page_count = 1;
        private List<BuyProduct> currentFullList;

        private const int FIRST_PAGE = 1;

        public BuyProductListView(SortAndFilterViewModel<IProductService> sortAndFilterViewModel)
        {
            this.InitializeComponent();

            // Initialize services and view models
            buyProductsViewModel = MarketMinds.App.BuyProductsViewModel;
            this.sortAndFilterViewModel = sortAndFilterViewModel;
            compareProductsViewModel = MarketMinds.App.CompareProductsViewModel;
            paginationService = new ProductPaginationService();
            buyProductListViewModelHelper = new BuyProductListViewModelHelper(sortAndFilterViewModel, buyProductsViewModel);

            buyProducts = new ObservableCollection<BuyProduct>();
            BuyListView.ItemsSource = buyProducts;
            ApplyFiltersAndPagination();
        }

        private void BuyListView_ItemClick(object sender, ItemClickEventArgs itemClickEventArgs)
        {
            var selectedProduct = itemClickEventArgs.ClickedItem as BuyProduct;
            if (selectedProduct != null)
            {
                var detailView = new BuyProductView(selectedProduct);
                detailView.Activate();
            }
        }

        private void ApplyFiltersAndPagination()
        {
            var (currentPageProducts, newTotalPages, fullList) = buyProductListViewModelHelper.GetBuyProductsPage(buyProductsViewModel, sortAndFilterViewModel, current_page);
            currentFullList = fullList;
            total_page_count = newTotalPages;
            buyProducts.Clear();
            foreach (var product in currentPageProducts)
            {
                buyProducts.Add(product);
            }

            // Restore sort selection after applying filters
            RestoreSortSelection();
        }

        private void RestoreSortSelection()
        {
            if (sortAndFilterViewModel.SortCondition != null)
            {
                var matchingItem = SortingComboBox.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(item => item.Tag.ToString() == sortAndFilterViewModel.SortCondition.InternalAttributeFieldTitle +
                        (sortAndFilterViewModel.SortCondition.IsAscending ? "Asc" : "Desc"));
                if (matchingItem != null)
                {
                    SortingComboBox.SelectedItem = matchingItem;
                }
            }
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            if (current_page < total_page_count)
            {
                current_page++;
                ApplyFiltersAndPagination();
            }
        }

        private void PreviousPageButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            if (current_page > FIRST_PAGE)
            {
                current_page--;
                ApplyFiltersAndPagination();
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            if (current_page < total_page_count)
            {
                current_page++;
                ApplyFiltersAndPagination();
            }
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            if (current_page > FIRST_PAGE)
            {
                current_page--;
                ApplyFiltersAndPagination();
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            sortAndFilterViewModel.HandleSearchQueryChange(SearchTextBox.Text);
            ApplyFiltersAndPagination();
        }

        private async void FilterButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            FilterDialog filterDialog = new FilterDialog(sortAndFilterViewModel);
            filterDialog.XamlRoot = Content.XamlRoot;
            var result = await filterDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                ApplyFiltersAndPagination();
            }
        }

        private void SortButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            // Toggle the sorting dropdown visibility
            SortingComboBox.Visibility = SortingComboBox.Visibility == Visibility.Visible ?
                                         Visibility.Collapsed : Visibility.Visible;

            // If we're showing the dropdown, ensure we have a selected item
            if (SortingComboBox.Visibility == Visibility.Visible)
            {
                RestoreSortSelection();
            }
        }

        private void SortingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs routedEventArgs)
        {
            if (SortingComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                var sortTag = selectedItem.Tag.ToString();
                var converter = new SortTypeConverterService();
                var sortType = converter.Convert(sortTag);
                if (sortType != null)
                {
                    sortAndFilterViewModel.HandleSortChange(sortType);
                    ApplyFiltersAndPagination();
                }
            }
        }

        private void AddToCompare_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            if (sender is Button button && button.DataContext is Product selectedProduct)
            {
                bool twoAdded = compareProductsViewModel.AddProductForCompare(selectedProduct);
                if (twoAdded)
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
