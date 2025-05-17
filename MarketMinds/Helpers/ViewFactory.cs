namespace MarketMinds.Helpers
{
    using MarketMinds.Views;
    using MarketMinds.Shared.Services.Interfaces;
    using BusinessLogicLayer.ViewModel;

    public static class ViewFactory
    {
        public static MarketMinds.Views.AuctionProductListView CreateAuctionProductListView()
        {
            var viewModel = MarketMinds.App.AuctionProductSortAndFilterViewModel;
            return new MarketMinds.Views.AuctionProductListView(viewModel);
        }

        public static MarketMinds.Views.BorrowProductListView CreateBorrowProductListView()
        {
            var viewModel = MarketMinds.App.BorrowProductSortAndFilterViewModel;
            return new MarketMinds.Views.BorrowProductListView(viewModel);
        }

        public static MarketMinds.Views.BuyProductListView CreateBuyProductListView()
        {
            // Cast to IProductService version for BuyProductListView compatibility
            var sourceViewModel = MarketMinds.App.BuyProductSortAndFilterViewModel;
            var viewModel = (SortAndFilterViewModel<IProductService>)(object)sourceViewModel;
            return new MarketMinds.Views.BuyProductListView(viewModel);
        }

        public static MarketMinds.Views.FilterDialog CreateAuctionFilterDialog()
        {
            var viewModel = (SortAndFilterViewModel<IProductService>)(object)MarketMinds.App.AuctionProductSortAndFilterViewModel;
            return new MarketMinds.Views.FilterDialog(viewModel);
        }

        public static MarketMinds.Views.FilterDialog CreateBorrowFilterDialog()
        {
            var viewModel = (SortAndFilterViewModel<IProductService>)(object)MarketMinds.App.BorrowProductSortAndFilterViewModel;
            return new MarketMinds.Views.FilterDialog(viewModel);
        }

        public static MarketMinds.Views.FilterDialog CreateBuyFilterDialog()
        {
            var viewModel = (SortAndFilterViewModel<IProductService>)(object)MarketMinds.App.BuyProductSortAndFilterViewModel;
            return new MarketMinds.Views.FilterDialog(viewModel);
        }
    }
}