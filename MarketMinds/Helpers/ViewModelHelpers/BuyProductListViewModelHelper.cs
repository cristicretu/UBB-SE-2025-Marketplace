using System.Collections.Generic;
using System.Linq;
using MarketMinds.Shared.Models;
using ViewModelLayer.ViewModel;
using BusinessLogicLayer.ViewModel;
using MarketMinds.Shared.Services.ProductPaginationService;
using MarketMinds.Shared.Services.BuyProductsService;
using MarketMinds.Shared.Services;
using MarketMinds.Shared.Services.Interfaces;

namespace MarketMinds.Helpers.ViewModelHelpers
{
    public class BuyProductListViewModelHelper
    {
        private readonly SortAndFilterViewModel<IProductService> sortAndFilterViewModel;
        private readonly BuyProductsViewModel buyProductsViewModel;

        public BuyProductListViewModelHelper(SortAndFilterViewModel<IProductService> sortAndFilterViewModel, BuyProductsViewModel buyProductsViewModel)
        {
            this.sortAndFilterViewModel = sortAndFilterViewModel;
            this.buyProductsViewModel = buyProductsViewModel;
        }

        public List<Product> GetFilteredProducts()
        {
            return sortAndFilterViewModel.HandleSearch();
        }

        public void ClearAllFilters()
        {
            sortAndFilterViewModel.HandleClearAllFilters();
        }

        public void UpdateSortCondition(ProductSortType sortCondition)
        {
            sortAndFilterViewModel.HandleSortChange(sortCondition);
        }

        public void UpdateSearchQuery(string searchQuery)
        {
            sortAndFilterViewModel.HandleSearchQueryChange(searchQuery);
        }

        public void AddProductCondition(Condition condition)
        {
            sortAndFilterViewModel.HandleAddProductCondition(condition);
        }

        public void RemoveProductCondition(Condition condition)
        {
            sortAndFilterViewModel.HandleRemoveProductCondition(condition);
        }

        public void AddProductCategory(Category category)
        {
            sortAndFilterViewModel.HandleAddProductCategory(category);
        }

        public void RemoveProductCategory(Category category)
        {
            sortAndFilterViewModel.HandleRemoveProductCategory(category);
        }

        public void AddProductTag(ProductTag tag)
        {
            sortAndFilterViewModel.HandleAddProductTag(tag);
        }

        public void RemoveProductTag(ProductTag tag)
        {
            sortAndFilterViewModel.HandleRemoveProductTag(tag);
        }

        public (IEnumerable<BuyProduct> currentPageProducts, int totalPages, List<BuyProduct> fullList) GetBuyProductsPage(
            BuyProductsViewModel buyProductsViewModel,
            SortAndFilterViewModel<IProductService> sortAndFilterViewModel,
            int currentPage)
        {
            var filteredProducts = sortAndFilterViewModel.HandleSearch();
            var fullList = filteredProducts.Cast<BuyProduct>().ToList();

            var paginationService = new ProductPaginationService();
            var (currentPageProducts, totalPages) = paginationService.GetPaginatedProducts(fullList, currentPage);
            return (currentPageProducts, totalPages, fullList);
        }
    }
}