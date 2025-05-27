using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.Interfaces;
using BusinessLogicLayer.ViewModel;
using MarketMinds.ViewModels;
using ProductCategory = MarketMinds.Shared.Models.Category;
using ProductCondition = MarketMinds.Shared.Models.Condition;

namespace MarketMinds.Views
{
    public partial class FilterDialog : ContentDialog
    {
        private readonly SortAndFilterViewModel<IProductService> sortAndFilterViewModel;
        private readonly ProductTagViewModel productTagViewModel;
        private readonly ProductConditionViewModel productConditionViewModel;
        private readonly ProductCategoryViewModel productCategoryViewModel;

        // Full lists
        private List<Category> fullCategories;
        private List<ProductTag> fullTags;

        // Displayed lists for filtering with pagination
        public ObservableCollection<Condition> ProductConditions { get; set; }
        public ObservableCollection<Category> DisplayedCategories { get; set; }
        public ObservableCollection<ProductTag> DisplayedTags { get; set; }

        // Pagination counts
        private int initialDisplayCount = 5;
        private int additionalDisplayCount = 10;

        public FilterDialog(SortAndFilterViewModel<IProductService> sortAndFilterViewModel)
        {
            this.InitializeComponent();
            this.sortAndFilterViewModel = sortAndFilterViewModel;

            // Initialize the view models.
            productConditionViewModel = MarketMinds.App.ProductConditionViewModel;
            productCategoryViewModel = MarketMinds.App.ProductCategoryViewModel;
            productTagViewModel = MarketMinds.App.ProductTagViewModel;

            // Initialize full lists
            ProductConditions = new ObservableCollection<Condition>(productConditionViewModel.GetAllProductConditions());
            fullCategories = productCategoryViewModel.GetAllProductCategories();
            fullTags = productTagViewModel.GetAllProductTags();

            // Initialize displayed lists (with pagination)
            DisplayedCategories = new ObservableCollection<Category>(fullCategories.Take(initialDisplayCount));
            DisplayedTags = new ObservableCollection<ProductTag>(fullTags.Take(initialDisplayCount));

            // Bind to ListViews
            ConditionListView.ItemsSource = ProductConditions;
            CategoryListView.ItemsSource = DisplayedCategories;
            TagListView.ItemsSource = DisplayedTags;

            // Pre-select active filters from view model
            PreselectActiveFilters();

            // Update View More buttons if needed
            UpdateViewMoreButtons();

            this.PrimaryButtonClick += FilterDialog_PrimaryButtonClick;
        }

        private void PreselectActiveFilters()
        {
            // Pre-select Conditions
            foreach (var condition in sortAndFilterViewModel.SelectedConditions)
            {
                var item = ProductConditions.FirstOrDefault(c => c.DisplayTitle == condition.DisplayTitle);
                if (item != null && !ConditionListView.SelectedItems.Contains(item))
                {
                    ConditionListView.SelectedItems.Add(item);
                }
            }

            // Pre-select Categories (if they are in fullCategories)
            foreach (var category in sortAndFilterViewModel.SelectedCategories)
            {
                var item = fullCategories.FirstOrDefault(c => c.DisplayTitle == category.DisplayTitle);
                if (item != null && !CategoryListView.SelectedItems.Contains(item))
                {
                    CategoryListView.SelectedItems.Add(item);
                }
            }

            // Pre-select Tags
            foreach (var tag in sortAndFilterViewModel.SelectedTags)
            {
                var item = fullTags.FirstOrDefault(t => t.DisplayTitle == tag.DisplayTitle);
                if (item != null && !TagListView.SelectedItems.Contains(item))
                {
                    TagListView.SelectedItems.Add(item);
                }
            }
        }

        private void UpdateViewMoreButtons()
        {
            ViewMoreCategoriesButton.Visibility = fullCategories.Count > DisplayedCategories.Count ? Visibility.Visible : Visibility.Collapsed;
            ViewMoreTagsButton.Visibility = fullTags.Count > DisplayedTags.Count ? Visibility.Visible : Visibility.Collapsed;
        }

        private void FilterDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs contentDialogButtonClickEventArgs)
        {
            // Clear existing selections in the view model
            sortAndFilterViewModel.HandleClearAllFilters();

            // Add currently selected Conditions
            foreach (ProductCondition condition in ConditionListView.SelectedItems)
            {
                sortAndFilterViewModel.HandleAddProductCondition(condition);
            }
            // Add selected Categories
            foreach (ProductCategory category in CategoryListView.SelectedItems)
            {
                sortAndFilterViewModel.HandleAddProductCategory(category);
            }
            // Add selected Tags
            foreach (ProductTag tag in TagListView.SelectedItems)
            {
                sortAndFilterViewModel.HandleAddProductTag(tag);
            }
        }

        private void ClearAllButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            // Clear selections in UI and view model
            ConditionListView.SelectedItems.Clear();
            CategoryListView.SelectedItems.Clear();
            TagListView.SelectedItems.Clear();
            sortAndFilterViewModel.HandleClearAllFilters();
        }

        // Category Search handling
        private void CategorySearchBox_TextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            var query = CategorySearchBox.Text.ToLower();
            var filtered = fullCategories.Where(category => category.DisplayTitle.ToLower().Contains(query)).ToList();
            DisplayedCategories.Clear();
            foreach (var category in filtered.Take(initialDisplayCount))
            {
                DisplayedCategories.Add(category);
            }
            UpdateViewMoreButtons();
        }

        // Tag Search handling
        private void TagSearchBox_TextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            var query = TagSearchBox.Text.ToLower();
            var filtered = fullTags.Where(tag => tag.DisplayTitle.ToLower().Contains(query)).ToList();
            DisplayedTags.Clear();
            foreach (var tag in filtered.Take(initialDisplayCount))
            {
                DisplayedTags.Add(tag);
            }
            UpdateViewMoreButtons();
        }

        // View More for Categories
        private void ViewMoreCategoriesButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            var currentCount = DisplayedCategories.Count;
            var query = CategorySearchBox.Text.ToLower();
            var filtered = fullCategories.Where(category => category.DisplayTitle.ToLower().Contains(query)).ToList();
            foreach (var cat in filtered.Skip(currentCount).Take(additionalDisplayCount))
            {
                DisplayedCategories.Add(cat);
            }
            UpdateViewMoreButtons();
        }

        // View More for Tags
        private void ViewMoreTagsButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            var currentCount = DisplayedTags.Count;
            var query = TagSearchBox.Text.ToLower();
            var filtered = fullTags.Where(tag => tag.DisplayTitle.ToLower().Contains(query)).ToList();
            foreach (var tag in filtered.Skip(currentCount).Take(additionalDisplayCount))
            {
                DisplayedTags.Add(tag);
            }
            UpdateViewMoreButtons();
        }
    }
}
