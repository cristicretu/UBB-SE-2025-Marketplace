namespace MarketMinds.Helpers
{
    using MarketMinds.Views;

    public static class ViewFactory
    {
        public static MarketMinds.Views.AuctionProductListView CreateAuctionProductListView()
        {
            return new MarketMinds.Views.AuctionProductListView(MarketMinds.App.AuctionProductSortAndFilterViewModel);
        }

        public static MarketMinds.Views.BorrowProductListView CreateBorrowProductListView()
        {
            return new MarketMinds.Views.BorrowProductListView(MarketMinds.App.BorrowProductSortAndFilterViewModel);
        }

        public static MarketMinds.Views.BuyProductListView CreateBuyProductListView()
        {
            return new MarketMinds.Views.BuyProductListView(MarketMinds.App.BuyProductSortAndFilterViewModel);
        }

        public static MarketMinds.Views.FilterDialog CreateAuctionFilterDialog()
        {
            return new MarketMinds.Views.FilterDialog(MarketMinds.App.AuctionProductSortAndFilterViewModel);
        }

        public static MarketMinds.Views.FilterDialog CreateBorrowFilterDialog()
        {
            return new MarketMinds.Views.FilterDialog(MarketMinds.App.BorrowProductSortAndFilterViewModel);
        }

        public static MarketMinds.Views.FilterDialog CreateBuyFilterDialog()
        {
            return new MarketMinds.Views.FilterDialog(MarketMinds.App.BuyProductSortAndFilterViewModel);
        }
    }
}