using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.BorrowProductsService;
using BusinessLogicLayer.ViewModel;
using ViewModelLayer.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MarketMinds.Helpers.ViewModelHelpers
{
    public class BorrowProductListViewModelHelper
    {
        private const int ItemsPerPage = 20;
        private readonly SortAndFilterViewModel<BorrowProductsService> sortAndFilterViewModel;
        private readonly BorrowProductsViewModel borrowProductsViewModel;

        public BorrowProductListViewModelHelper(SortAndFilterViewModel<BorrowProductsService> sortAndFilterViewModel, BorrowProductsViewModel borrowProductsViewModel)
        {
            this.sortAndFilterViewModel = sortAndFilterViewModel;
            this.borrowProductsViewModel = borrowProductsViewModel;
        }

        public (List<BorrowProduct> pageItems, int totalPages, List<BorrowProduct> fullList) GetBorrowProductsPage(
            BorrowProductsViewModel borrowProductsViewModel,
            SortAndFilterViewModel<BorrowProductsService> sortAndFilterViewModel,
            int currentPage)
        {
            // Retrieve filtered and sorted products
            var filteredProducts = sortAndFilterViewModel.HandleSearch()
                                        .Cast<BorrowProduct>().ToList();
            var fullList = filteredProducts;
            int totalPages = (int)Math.Ceiling((double)fullList.Count / ItemsPerPage);
            // Get current page items
            var pageItems = fullList
                .Skip((currentPage - 1) * ItemsPerPage)
                .Take(ItemsPerPage)
                .ToList();
            return (pageItems, totalPages, fullList);
        }

        public (bool hasPrevious, bool hasNext) GetPaginationState(int currentPage, int totalPages)
        {
            return (currentPage > 1, currentPage < totalPages);
        }

        public string GetPaginationText(int currentPage, int totalPages)
        {
            return totalPages == 0 ?
                $"Page {currentPage} of 1" :
                $"Page {currentPage} of {totalPages}";
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
    }
}