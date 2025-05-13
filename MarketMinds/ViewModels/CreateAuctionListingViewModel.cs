using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.AuctionProductsService;
using MarketMinds.Shared.ProxyRepository;
using ViewModelLayer.ViewModel;

namespace MarketMinds.ViewModels
{
    public class CreateAuctionListingViewModel : CreateListingViewModelBase
    {
        private readonly AuctionProductsService auctionProductsService;

        public CreateAuctionListingViewModel(AuctionProductsService auctionProductsService)
        {
            this.auctionProductsService = auctionProductsService;
        }

        public override void CreateListing(Product product)
        {
            auctionProductsService.CreateListing(product);
        }
    }
}
