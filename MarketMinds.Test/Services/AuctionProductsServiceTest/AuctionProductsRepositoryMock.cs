using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketMinds.Shared.Models;
using MarketMinds.Shared.Services.AuctionProductsService;
using MarketMinds.Shared.Services.Interfaces;

namespace MarketMinds.Test.Services.Mocks
{
    public class AuctionProductsRepositoryMock : IAuctionProductsService
    {
        private readonly List<AuctionProduct> _auctionProducts = new List<AuctionProduct>();
        private readonly List<Bid> _bids = new List<Bid>();

        public void CreateListing(Product product)
        {
            if (!(product is AuctionProduct auctionProduct))
            {
                throw new ArgumentException("Product must be an AuctionProduct.");
            }

            auctionProduct.Id = _auctionProducts.Count + 1;
            _auctionProducts.Add(auctionProduct);
        }

        public void PlaceBid(AuctionProduct auction, User bidder, double bidAmount)
        {
            var existingAuction = _auctionProducts.FirstOrDefault(a => a.Id == auction.Id);
            if (existingAuction == null)
            {
                throw new KeyNotFoundException("Auction not found");
            }

            if (bidAmount <= existingAuction.CurrentPrice)
            {
                throw new InvalidOperationException("Bid amount must be higher than current price");
            }

            var bid = new Bid(bidder.Id, auction.Id, bidAmount);
            _bids.Add(bid);
            existingAuction.CurrentPrice = bidAmount;
            existingAuction.Bids.Add(bid);
        }

        public void ConcludeAuction(AuctionProduct auction)
        {
            var productToRemove = _auctionProducts.FirstOrDefault(a => a.Id == auction.Id);
            if (productToRemove != null)
            {
                _auctionProducts.Remove(productToRemove);
            }
        }

        public List<AuctionProduct> GetProducts()
        {
            return _auctionProducts.ToList();
        }

        public AuctionProduct GetProductById(int id)
        {
            return _auctionProducts.FirstOrDefault(a => a.Id == id);
        }

        public string GetTimeLeft(AuctionProduct auction)
        {
            var timeLeft = auction.EndTime - DateTime.Now;
            return timeLeft > TimeSpan.Zero ? timeLeft.ToString(@"dd\:hh\:mm\:ss") : "Auction Ended";
        }

        public bool IsAuctionEnded(AuctionProduct auction)
        {
            return DateTime.Now >= auction.EndTime;
        }

        public List<Product> GetSortedFilteredProducts(List<Condition> selectedConditions, List<Category> selectedCategories, List<ProductTag> selectedTags, ProductSortType sortCondition, string searchQuery)
        {
            var products = _auctionProducts.Cast<Product>().ToList();

            // Filtering logic
            if (selectedConditions != null && selectedConditions.Any())
            {
                products = products.Where(p => selectedConditions.Any(c => c.Id == p.Condition?.Id)).ToList();
            }

            if (selectedCategories != null && selectedCategories.Any())
            {
                products = products.Where(p => selectedCategories.Any(c => c.Id == p.Category?.Id)).ToList();
            }

            if (selectedTags != null && selectedTags.Any())
            {
                products = products.Where(p => p.Tags != null && p.Tags.Any(t => selectedTags.Any(st => st.Id == t.Id))).ToList();
            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                products = products.Where(p => p.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Sorting logic
            if (sortCondition != null)
            {
                var propertyInfo = typeof(Product).GetProperty(sortCondition.InternalAttributeFieldTitle);
                if (propertyInfo != null)
                {
                    products = sortCondition.IsAscending
                        ? products.OrderBy(p => propertyInfo.GetValue(p)).ToList()
                        : products.OrderByDescending(p => propertyInfo.GetValue(p)).ToList();
                }
            }

            return products;
        }

        // Async methods
        public Task<List<AuctionProduct>> GetAllAuctionProductsAsync()
        {
            return Task.FromResult(_auctionProducts.ToList());
        }

        public Task<AuctionProduct> GetAuctionProductByIdAsync(int id)
        {
            return Task.FromResult(_auctionProducts.FirstOrDefault(a => a.Id == id));
        }

        public Task<bool> CreateAuctionProductAsync(AuctionProduct auctionProduct)
        {
            try
            {
                CreateListing(auctionProduct);
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public Task<bool> PlaceBidAsync(int auctionId, int bidderId, double bidAmount)
        {
            try
            {
                var auction = GetProductById(auctionId);
                var bidder = new User { Id = bidderId };
                PlaceBid(auction, bidder, bidAmount);
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public Task<bool> UpdateAuctionProductAsync(AuctionProduct auctionProduct)
        {
            try
            {
                var existing = GetProductById(auctionProduct.Id);
                if (existing != null)
                {
                    _auctionProducts.Remove(existing);
                    _auctionProducts.Add(auctionProduct);
                }
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public Task<bool> DeleteAuctionProductAsync(int id)
        {
            try
            {
                var product = GetProductById(id);
                if (product != null)
                {
                    _auctionProducts.Remove(product);
                }
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public void ValidateBid(AuctionProduct auction, User bidder, double bidAmount)
        {
            throw new NotImplementedException();
        }

        public void ExtendAuctionTime(AuctionProduct auction)
        {
            throw new NotImplementedException();
        }
    }
}