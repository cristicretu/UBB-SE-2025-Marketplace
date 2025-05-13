using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using BusinessLogicLayer.ViewModel;
using ViewModelLayer.ViewModel;
using MarketMinds.Shared.Services.AuctionProductsService;
using MarketMinds.Shared.Services.BorrowProductsService;
using MarketMinds.Shared.Services.BuyProductsService;
using UiLayer;

namespace MarketMinds.Helpers
{
    public static class ViewFactory
    {
        public static AuctionProductListView CreateAuctionProductListView()
        {
            return new AuctionProductListView(MarketMinds.App.AuctionProductSortAndFilterViewModel);
        }

        public static BorrowProductListView CreateBorrowProductListView()
        {
            return new BorrowProductListView(MarketMinds.App.BorrowProductSortAndFilterViewModel);
        }

        public static BuyProductListView CreateBuyProductListView()
        {
            return new BuyProductListView(MarketMinds.App.BuyProductSortAndFilterViewModel);
        }

        public static FilterDialog CreateAuctionFilterDialog()
        {
            return new FilterDialog(MarketMinds.App.AuctionProductSortAndFilterViewModel);
        }

        public static FilterDialog CreateBorrowFilterDialog()
        {
            return new FilterDialog(MarketMinds.App.BorrowProductSortAndFilterViewModel);
        }

        public static FilterDialog CreateBuyFilterDialog()
        {
            return new FilterDialog(MarketMinds.App.BuyProductSortAndFilterViewModel);
        }
    }
}