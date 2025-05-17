using System;
using System.Collections.Generic;
using System.Linq;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.AuctionProductsService;
using BusinessLogicLayer.ViewModel;
using ViewModelLayer.ViewModel;

namespace MarketMinds.Helpers.ViewModelHelpers
{
    public class AuctionProductListViewModelHelper
    {
        private const int NO_ITEMS = 0;
        private readonly SortAndFilterViewModel<AuctionProductsService> sortAndFilterViewModel;
        private readonly AuctionProductsViewModel auctionProductsViewModel;

        public AuctionProductListViewModelHelper(SortAndFilterViewModel<AuctionProductsService> sortAndFilterViewModel, AuctionProductsViewModel auctionProductsViewModel)
        {
            this.sortAndFilterViewModel = sortAndFilterViewModel;
            this.auctionProductsViewModel = auctionProductsViewModel;
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

        public (List<AuctionProduct> pageItems, int totalPages, List<AuctionProduct> fullList) GetAuctionProductsPage(
            AuctionProductsViewModel auctionProductsViewModel,
            SortAndFilterViewModel<AuctionProductsService> sortAndFilterViewModel,
            int currentPage,
            int itemsPerPage)
        {
            var filteredProducts = sortAndFilterViewModel.HandleSearch().Cast<AuctionProduct>().ToList();
            var fullList = filteredProducts;

            int totalPages = (int)Math.Ceiling((double)fullList.Count / itemsPerPage);

            var pageItems = fullList
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToList();

            return (pageItems, totalPages, fullList);
        }

        public bool ShouldShowEmptyMessage(List<AuctionProduct> pageItems)
        {
            return pageItems.Count == NO_ITEMS;
        }

        public (bool canGoToPrevious, bool canGoToNext) GetPaginationButtonState(int currentPage, int totalPages, int basePage)
        {
            bool canGoToPrevious = currentPage > basePage;
            bool canGoToNext = currentPage < totalPages;
            return (canGoToPrevious, canGoToNext);
        }

        public string GetPaginationText(int currentPage, int totalPages)
        {
            return totalPages == NO_ITEMS ?
                $"Page {currentPage} of 1" :
                $"Page {currentPage} of {totalPages}";
        }
    }
}