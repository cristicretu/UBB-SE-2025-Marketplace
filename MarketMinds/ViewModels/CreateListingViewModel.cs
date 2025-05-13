﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModelLayer.ViewModel;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.ListingCreationService;

namespace ViewModelLayer.ViewModel
{
    public class CreateListingViewModel
    {
        private readonly ListingCreationService listingCreationService;
        public Product Product { get; set; }
        public string ListingType { get; set; }

        public CreateListingViewModel(ListingCreationService listingCreationService)
        {
            this.listingCreationService = listingCreationService;
        }

        public void CreateMarketPlaceListing()
        {
            listingCreationService.CreateMarketListing(Product, ListingType);
        }
    }
}
