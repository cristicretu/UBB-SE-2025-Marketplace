using System.Collections.Generic;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services;

namespace BusinessLogicLayer.ViewModel
{
    public class SortAndFilterViewModel<TService>
        where TService : IProductService
    {
        private readonly TService productService;

        public List<Condition> SelectedConditions { get; set; }
        public List<Category> SelectedCategories { get; set; }
        public List<ProductTag> SelectedTags { get; set; }
        public ProductSortType? SortCondition { get; set; }
        public string SearchQuery { get; set; }

        public SortAndFilterViewModel(TService productService)
        {
            this.productService = productService;
            this.SelectedConditions = new List<Condition>();
            this.SelectedCategories = new List<Category>();
            this.SelectedTags = new List<ProductTag>();
            this.SortCondition = null;
            this.SearchQuery = string.Empty;
        }

        public List<Product> HandleSearch()
        {
            return productService.GetSortedFilteredProducts(
                SelectedConditions,
                SelectedCategories,
                SelectedTags,
                SortCondition,
                SearchQuery);
        }

        public void HandleClearAllFilters()
        {
            this.SelectedConditions.Clear();
            this.SelectedCategories.Clear();
            this.SelectedTags.Clear();
            this.SortCondition = null;
            this.SearchQuery = string.Empty;
        }

        public void HandleSortChange(ProductSortType newSortCondition)
        {
            this.SortCondition = newSortCondition;
        }

        public void HandleSearchQueryChange(string searchQuery)
        {
            this.SearchQuery = searchQuery;
        }

        public void HandleAddProductCondition(Condition condition)
        {
            this.SelectedConditions.Add(condition);
        }

        public void HandleRemoveProductCondition(Condition condition)
        {
            this.SelectedConditions.Remove(condition);
        }

        public void HandleAddProductCategory(Category category)
        {
            this.SelectedCategories.Add(category);
        }

        public void HandleRemoveProductCategory(Category category)
        {
            this.SelectedCategories.Remove(category);
        }

        public void HandleAddProductTag(ProductTag tag)
        {
            this.SelectedTags.Add(tag);
        }

        public void HandleRemoveProductTag(ProductTag tag)
        {
            this.SelectedTags.Remove(tag);
        }
    }
}
