using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.BorrowProductsService;

namespace MarketMinds.ViewModels
{
    public class CreateBorrowListingViewModel : CreateListingViewModelBase
    {
        private IBorrowProductsService borrowProductsService;

        public BorrowProductsService BorrowProductsService
        {
            get => borrowProductsService as BorrowProductsService;
            set => borrowProductsService = value;
        }

        public override void CreateListing(Product product)
        {
            borrowProductsService.CreateListing(product);
        }
    }
}
