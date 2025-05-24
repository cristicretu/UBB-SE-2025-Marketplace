// File: ../WebMarketplace/Models/HomeViewModel.cs
using MarketMinds.Shared.Models;
using System.Collections.Generic;

namespace MarketMinds.Web.Models
{
    public class HomeViewModel
    {
        public HomeViewModel()
        {
        }

        public IEnumerable<BuyProduct> BuyProducts { get; set; } = new List<BuyProduct>();
        public IEnumerable<AuctionProduct> AuctionProducts { get; set; } = new List<AuctionProduct>();
        public IEnumerable<BorrowProduct> BorrowProducts { get; set; } = new List<BorrowProduct>();
    }
}
