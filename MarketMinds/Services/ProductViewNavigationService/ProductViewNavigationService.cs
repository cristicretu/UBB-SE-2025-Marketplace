using System;
using MarketMinds.Shared.Models;
using MarketMinds.Views.Pages;

namespace MarketMinds.Services
{
    public class ProductViewNavigationService : IProductViewNavigationService
    {
        public Microsoft.UI.Xaml.Window CreateProductDetailView(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            Microsoft.UI.Xaml.Window detailView;

            if (product is AuctionProduct auctionProduct)
            {
                detailView = new AuctionProductView(auctionProduct);
            }
            else if (product is BorrowProduct borrowProduct)
            {
                detailView = new BorrowProductView(borrowProduct);
            }
            else if (product is BuyProduct buyProduct)
            {
                detailView = new BuyProductView(buyProduct);
            }
            else
            {
                throw new ArgumentException($"Unknown product type: {product.GetType().Name}");
            }

            return detailView;
        }

        public Microsoft.UI.Xaml.Window CreateSellerReviewsView(User seller)
        {
            if (seller == null)
            {
                throw new ArgumentNullException(nameof(seller));
            }

            var window = new Microsoft.UI.Xaml.Window();
            window.Content = new SeeSellerReviewsView(App.SeeSellerReviewsViewModel);
            App.SeeSellerReviewsViewModel.Seller = seller;
            return window;
        }
    }
}