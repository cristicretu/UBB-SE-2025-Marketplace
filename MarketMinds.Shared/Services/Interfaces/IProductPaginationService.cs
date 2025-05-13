using System;
using System.Collections.Generic;

namespace MarketMinds.Shared.Services
{
    /// <summary>
    /// Interface for ProductPaginationService to manage product pagination and filtering.
    /// </summary>
    public interface IProductPaginationService
    {
        /// <summary>
        /// Retrieves a paginated list of products.
        /// </summary>
        /// <typeparam name="T">The type of the products.</typeparam>
        /// <param name="allProducts">The complete list of products.</param>
        /// <param name="currentPage">The current page number.</param>
        /// <returns>A tuple containing the current page of products and the total number of pages.</returns>
        (List<T> CurrentPage, int TotalPages) GetPaginatedProducts<T>(List<T> allProducts, int currentPage);

        /// <summary>
        /// Applies a filter to a list of products.
        /// </summary>
        /// <typeparam name="T">The type of the products.</typeparam>
        /// <param name="products">The list of products to filter.</param>
        /// <param name="filterPredicate">The filter predicate to apply.</param>
        /// <returns>A filtered list of products.</returns>
        List<T> ApplyFilters<T>(List<T> products, Func<T, bool> filterPredicate);
    }
}
