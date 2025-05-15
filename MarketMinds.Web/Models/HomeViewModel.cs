// File: ../WebMarketplace/Models/HomeViewModel.cs
using System.Collections.Generic;

namespace WebMarketplace.Models
{
    public class HomeViewModel
    {
        public HomeViewModel()
        {
            BuyerFamilySyncItems = new List<BuyerFamilySyncItemViewModel>();
            BuyerFamilySyncs = new List<BuyerFamilySyncViewModel>();
            BuyerBadges = new List<BuyerBadgeViewModel>();
            BuyerAddresses = new List<BuyerAddressViewModel>();
            BuyerShippingAddresses = new List<BuyerAddressViewModel>();
        }

        public List<BuyerFamilySyncItemViewModel> BuyerFamilySyncItems { get; set; }
        public List<BuyerFamilySyncViewModel> BuyerFamilySyncs { get; set; }
        public List<BuyerBadgeViewModel> BuyerBadges { get; set; }
        public List<BuyerAddressViewModel> BuyerAddresses { get; set; }
        public List<BuyerAddressViewModel> BuyerShippingAddresses { get; set; }
    }
}
