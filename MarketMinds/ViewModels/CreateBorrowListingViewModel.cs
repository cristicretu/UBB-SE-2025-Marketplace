using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.BorrowProductsService;

namespace ViewModelLayer.ViewModel
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
