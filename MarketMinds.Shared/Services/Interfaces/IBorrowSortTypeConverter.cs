using MarketMinds.Shared.Models;

namespace MarketMinds.Shared.Services.BorrowSortTypeConverterService
{
    /// <summary>
    /// Interface for BorrowSortTypeConverterService to manage sort type conversions.
    /// </summary>
    public interface IBorrowSortTypeConverterService
    {
        /// <summary>
        /// Converts a sort tag into a ProductSortType.
        /// </summary>
        /// <param name="sortTag">The sort tag to be converted.</param>
        /// <returns>A ProductSortType object corresponding to the sort tag.</returns>
        ProductSortType Convert(string sortTag);
    }
}