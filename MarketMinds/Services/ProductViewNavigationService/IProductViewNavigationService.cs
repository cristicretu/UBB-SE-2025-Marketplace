using MarketMinds.Shared.Models;
using Windows.UI.Xaml;

namespace MarketMinds.Services
{
    /// <summary>
    /// Interface for ProductViewNavigationService to manage navigation to product-related views.
    /// </summary>
    public interface IProductViewNavigationService
    {
        /// <summary>
        /// Creates a detail view for a specific product.
        /// </summary>
        /// <param name="product">The product for which the detail view is created.</param>
        /// <returns>A Window containing the product detail view.</returns>
        Microsoft.UI.Xaml.Window CreateProductDetailView(Product product);

        /// <summary>
        /// Creates a view to display reviews for a specific seller.
        /// </summary>
        /// <param name="seller">The seller whose reviews are to be displayed.</param>
        /// <returns>A Window containing the seller reviews view.</returns>
        Microsoft.UI.Xaml.Window CreateSellerReviewsView(User seller);
    }
}
